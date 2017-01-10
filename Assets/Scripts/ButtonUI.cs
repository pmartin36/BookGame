using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class ButtonUI : MonoBehaviour, LetterEventInterface {

	public string harvestLetterDisplayed = ""; //which letter is being presented in the harvest box

	public GameObject itemBox;
	public GameObject itemSprite;

	private Image harvestBox;
	private GameObject trashCan;
	private Image itemBox1;
	private Image itemBox2;

	float distanceBetweenPowerups = 35f;

	int animationInProgress = 0;

	//1 is "harvestable" item, 2 is first slot, 3 is second slot, 4 is garbage slot and would be deleted
	private List<PowerupUI> powerupSprites = new List<PowerupUI> ();

	private Text text1;
	private Text text2;

	private Text newPowerupTextBox;
	private float newPowerupTextBox_initial_offset;
	private RectTransform rt; 

	Transform powerupParent;

	private bool stopBlink = false;
	private bool itemsBlinking = false;
	private bool itemCountsBlinking = false;

	private CameraController cameraController;

	// Use this for initialization
	void Start () {
		harvestBox = gameObject.transform.Find("Panel").Find ("Harvest Box").GetComponent<Image> ();
		trashCan = gameObject.transform.Find ("Panel").Find ("Garbage").gameObject;
		itemBox1 = gameObject.transform.Find("Panel").Find ("Item Box 1").GetComponent<Image> ();
		itemBox2 = gameObject.transform.Find("Panel").Find ("Item Box 2").GetComponent<Image> ();
		text1 = gameObject.transform.Find ("Panel").Find ("Panel1").GetComponentInChildren<Text> ();
		text2 = gameObject.transform.Find ("Panel").Find ("Panel2").GetComponentInChildren<Text> ();
		newPowerupTextBox = GameObject.Find ("NewPowerup").GetComponent<Text> ();
		powerupParent = gameObject.transform.Find ("Panel").Find ("PowerupUIParent");

		newPowerupTextBox_initial_offset = newPowerupTextBox.transform.position.y - this.transform.position.y;
		rt = this.GetComponent<RectTransform> ();

		distanceBetweenPowerups = Mathf.Abs(harvestBox.transform.position.x - itemBox1.transform.position.x);

		cameraController = Camera.main.GetComponent<CameraController> ();

		GameManager.signUpForNewLetterEvent (this);
	}

	// Update is called once per frame
	void Update () {
		if (harvestLetterDisplayed.Length <= 0) {
			harvestBox.color = Color.clear;
		}
	}

	void FixedUpdate(){
		PlacePowerupText ();
	}

	IEnumerator blinkHarvestLetter(){
		Image powerupSprite = powerupSprites [0].GetComponent<Image> ();
		while (harvestLetterDisplayed.Length > 0) {
			Color color = powerupSprite.color;
			color.a = Mathf.PingPong (Time.time/2f, 0.5f) + 0.5f;
			powerupSprite.color = color;
			yield return new WaitForSeconds (0.2f);
		}
		yield return null;
	}

	IEnumerator BlinkAllSprites(){
		itemsBlinking = true;

		List<Image> images = new List<Image> ();
		for(int i = 1; i < powerupSprites.Count; i++) {
			PowerupUI p = powerupSprites [i];
			if (p != null) {
				images.Add (p.GetComponent<Image> ());
			}
		}

		while (!stopBlink) {
			foreach (Image i in images) {
				if (i == null)
					continue;
				Color color = i.color;
				color.a = Mathf.PingPong (Time.time/2f, 0.7f) + 0.3f;
				i.color = color;
			}
			yield return new WaitForSeconds (0.2f);
		}

		foreach (Image i in images) {
			if (i == null)
				continue;
			Color color = i.color;
			color.a = 1f;
			i.color = color;
		}

		stopBlink = false;
		itemsBlinking = false;

		yield return null;
	}

	IEnumerator BlinkAllPOTDCounts(){
		itemCountsBlinking = true;

		List<Text> text = new List<Text> ();
		text.AddRange (this.GetComponentsInChildren<Text> ());
		for(int i = 0; i < text.Count; i++){
			Text t = text [i];

			int n;
			bool isNumber = int.TryParse (t.text, out n);

			if (t == null || !isNumber) {
				text.Remove (t);
				i--;
			}
		}

		while (!stopBlink) {
			foreach (Text i in text) {
				Color color = i.color;
				color.a = Mathf.PingPong (Time.time/2f, 0.7f) + 0.3f;
				i.color = color;
			}
			yield return new WaitForSeconds (0.2f);
		}

		foreach (Text i in text) {
			Color color = i.color;
			color.a = 1f;
			i.color = color;
		}

		stopBlink = false;
		itemCountsBlinking = false;

		yield return null;
	}

	public void updateHarvestItem(string newLetter){
		if (harvestLetterDisplayed.Equals (newLetter)) {
			return;
		}
		else if (GameManager.IsPowerupModifier (newLetter)) {
			if (newLetter == "N") {
				BlinkAllSprites ();
			}
		}
		else {
			harvestLetterDisplayed = newLetter;
		}

		Sprite newSprite = lettertoSprite(newLetter);

		if (newSprite == null) {
			harvestBox.color = Color.clear;
			if(powerupSprites[0] != null) powerupSprites [0].image.color = Color.clear;
			if(animationInProgress==0)
				trashCan.GetComponent<Image> ().color = Color.clear;
		} 
		else {
			harvestBox.color = new Color (1, 0.25f, 0.25f);

			PowerupUI newPowerupUI;
			if (powerupSprites.Count < 1) {
				newPowerupUI = CreateNewPowerupUI (newLetter);
				powerupSprites.Insert (0, newPowerupUI);
			} 
			else if(powerupSprites[0] == null) {
				newPowerupUI = CreateNewPowerupUI (newLetter);
				powerupSprites[0] = newPowerupUI;
			}
			
			powerupSprites[0].setSprite(newSprite);
			powerupSprites [0].letter = newLetter;
			powerupSprites [0].image.color = Color.white;

			if (powerupSprites.Count >= 3) {
				trashCan.GetComponent<Image> ().color = Color.white;
			}

			StopCoroutine ("blinkHarvestLetter");
			StartCoroutine ("blinkHarvestLetter");
		}
	}

	private PowerupUI CreateNewPowerupUI(string newLetter, int count = 1){
		GameObject newHarvestItem;
		PowerupUI newPowerupUI;
		newHarvestItem = Instantiate (itemSprite, harvestBox.transform.position, Quaternion.identity) as GameObject;
		newHarvestItem.transform.SetParent (powerupParent);
		newHarvestItem.transform.localScale = Vector3.one;
		newPowerupUI = newHarvestItem.GetComponent<PowerupUI> ();
		newPowerupUI.initialize (newLetter);
		newPowerupUI.count = count;
		return newPowerupUI;
	}

	public void advancePowerups(int i){
		if (i + 1 >= powerupSprites.Count) {
			powerupSprites.Add (powerupSprites [i]);
		} 
		else {
			if (powerupSprites [i + 1] != null) {
				advancePowerups (i + 1);
			}
			powerupSprites [i + 1] = powerupSprites [i];
		}

		if (i + 1 == 1 ) {
			//if we just harvested, add a button mapping to the new
			if (powerupSprites.Count < 3 || powerupSprites [2].mappedButton.Length < 0 || powerupSprites [2].mappedButton.ToUpper () == "Y") {
				powerupSprites [1].mappedButton = "X";
			} 
			else {
				powerupSprites [1].mappedButton = "Y";
			}
		}

		animationInProgress++;
		StartCoroutine (movePowerup (i + 1));
		powerupSprites [0].GetComponent<Image> ().color = Color.white;
		harvestLetterDisplayed = "";

		if (i == 0)
			powerupSprites [0] = null;
	}

	void IncreaseAllPOTDs(){
		stopBlink = true;
		foreach (PowerupUI p in powerupSprites) {
			if (p != null) {
				if (GameManager.IsPOTD (p.letter)) {
					increasePOTDItem (p.letter);
				}
			}
		}
	}

	public void increasePOTDItem(string POTDItem){
		for (int i = 1; i < powerupSprites.Count; i++) {
			if (powerupSprites [i].letter == POTDItem) {
				powerupSprites [i].count++;
				if (powerupSprites [0] != null) {
					StopCoroutine ("blinkHarvestLetter");
					powerupSprites [0].image.color = Color.clear;
					powerupSprites [0] = null;
				}

				if (i == 1) {
					text1.text = powerupSprites [i].count.ToString ();
				} 
				else if (i == 2) {
					text2.text = powerupSprites [i].count.ToString ();
				}
				return;
			}
		}
	}

	//should not be used anymore, see below decreasepotditem
	public void decreasePOTDItem(string POTDItem){
		for (int i = 1; i < powerupSprites.Count; i++) {
			if (powerupSprites [i].letter == POTDItem) {
				/* removing UI when POTD is used up
				powerupSprites [i].count--;
				if (powerupSprites [i].count <= 0) {
					powerupSprites [i].image.color = Color.clear;
					if (i == 1) {
						itemBox1.color = Color.gray;
						text1.text = "--";
					} 
					else if (i == 2) {
						itemBox2.color = Color.gray;
						text2.text = "--";
					}

					powerupSprites [i] = null;
				}
				else{
					if (i == 1) {
						text1.text = powerupSprites [i].count.ToString ();
					} 
					else if (i == 2) {
						text2.text = powerupSprites [i].count.ToString ();
					}
				}
				*/
				powerupSprites [i].count--;
				if (i == 1) {
					text1.text = powerupSprites [i].count.ToString ();
				} 
				else if (i == 2) {
					text2.text = powerupSprites [i].count.ToString ();
				}
				return;
			}
		}
	}

	public void decreasePOTDItem(string POTDItem, int index){
		powerupSprites [index].count--;
		if (index == 1) {
			text1.text = powerupSprites [index].count.ToString ();
		} 
		else if (index == 2) {
			text2.text = powerupSprites [index].count.ToString ();
		}
	}

	void removePowerups(){
		stopBlink = true;
		itemsBlinking = false;
		StopCoroutine ("BlinkAllSprites");
		powerupSprites.Clear ();
		foreach (Transform t in powerupParent) {
			GameObject.Destroy (t.gameObject);
		}
		itemBox1.color = Color.gray;
		itemBox2.color = Color.gray;
		text1.text = "--";
		text2.text = "--";
	}

	IEnumerator movePowerup(int newIndex){
		Vector3 newPosition = harvestBox.transform.position + new Vector3 (newIndex * (distanceBetweenPowerups), 0);
		Vector3 oldPosition = harvestBox.transform.position + new Vector3 ((newIndex-1) * (distanceBetweenPowerups), 0);
		float elapsed = 0;
		while (powerupSprites [newIndex].transform.position != newPosition) {
			powerupSprites [newIndex].transform.position = Vector3.Lerp (oldPosition, newPosition, elapsed);
			elapsed += 0.1f;
			yield return new WaitForSeconds (0.1f);
		}
			
		//careful for race conditions?
		animationInProgress--;
		if (animationInProgress == 0) {
			trashCan.GetComponent<Image> ().color = Color.clear;
		}

		if (newIndex == 3) {
			DestroyImmediate (powerupSprites [newIndex].gameObject);
			powerupSprites.RemoveAt (newIndex);
		} 
		else if (newIndex == 2) {
			PowerupUI newp = powerupSprites [newIndex];
			itemBox2.color = newp.getColor ();
			if (GameManager.IsPOTD (newp.letter)) {
				text2.text = newp.count.ToString ();
			} 
			else if(GameManager.IsPassive(newp.letter)){
				text2.text = "P";
			}
			else {
				text2.text = "--";
			}

		}
		else if (newIndex == 1) {
			PowerupUI newp = powerupSprites [newIndex];
			itemBox1.color = newp.getColor ();
			if (GameManager.IsPOTD (newp.letter)) {
				text1.text = newp.count.ToString ();
			} 
			else if(GameManager.IsPassive(newp.letter)){
				text1.text = "P";
			}
			else {
				text1.text = "--";
			}
		}

		yield return null;
	}

	private Sprite lettertoSprite(string letter){
		Sprite returnSprite;

		switch (letter.ToUpper()) {
		case "A":
			//set image for A item
			returnSprite = Resources.Load<Sprite> ("Sprites/bellows");
			break;
		case "B":
			//set image for B item
			returnSprite = Resources.LoadAll<Sprite> ("Sprites/springs")[0];
			break;
		case "C":
			returnSprite = null;
			break;
		case "D":
			returnSprite = Resources.Load<Sprite> ("Sprites/doublejump");
			break;
		case "E":
			returnSprite = Resources.Load<Sprite> ("Sprites/eraser");
			break;
		case "F":
			returnSprite = Resources.Load<Sprite> ("Sprites/Cloud");
			break;
		case "G":
			returnSprite = Resources.Load<Sprite> ("Sprites/Grapple");
			break;
		case "H":
			returnSprite = Resources.Load<Sprite> ("Sprites/higher");
			break;
		case "I":
			returnSprite = Resources.Load<Sprite> ("Sprites/ice");
			break;
		case "J":
			returnSprite = Resources.Load<Sprite> ("Sprites/gravity");
			break;
		case "K":
			returnSprite = Resources.Load<Sprite> ("Sprites/key");
			break;
		case "L":
			returnSprite = Resources.Load<Sprite> ("Sprites/lower");
			break;
		case "M":
			returnSprite = Resources.Load<Sprite> ("Sprites/make_usable");
			break;
		case "N":
			returnSprite = null;
			break;
		case "O":
			returnSprite = Resources.Load<Sprite> ("Sprites/opposite");
			break;
		case "P":
			returnSprite = null;
			break;
		case "Q":
			returnSprite = Resources.Load<Sprite> ("Sprites/fast");
			break;
		case "R":
			returnSprite = Resources.Load<Sprite> ("Sprites/rotate");
			break;
		case "S":
			returnSprite = Resources.Load<Sprite> ("Sprites/quagmire");
			break;
		case "T":
			returnSprite = Resources.Load<Sprite> ("Sprites/timer");
			break;
		case "U":
			returnSprite = Resources.Load<Sprite> ("Sprites/Umbrella");
			break;
		case "V":
			returnSprite = Resources.Load<Sprite> ("Sprites/Vapor_thumb");
			break;
		case "W":
			returnSprite = Resources.Load<Sprite> ("Sprites/wallclimb");
			break;
		case "X":
			returnSprite = null;
			break;
		case "Y":
			returnSprite = Resources.Load<Sprite> ("Sprites/Yank");
			break;
		case "Z":
			returnSprite = Resources.Load<Sprite> ("Sprites/binoculars");
			break;
		default:
			returnSprite = null;
			break;
		}

		return returnSprite;
	}

	private void updateHarvest(Letter letter){
		string newLetter = letter!=null ? letter.letter : "";
		int count = 1;

		//if the letter is a power-up modifier (duplicate/remove/etc)
		if (GameManager.IsPowerupModifier (newLetter) && letter.canBeHarvested) {
			if (newLetter == "C") {
				if (powerupSprites.Count > 1 && powerupSprites [1] != null) {
					count = powerupSprites [1].count;
					newLetter = powerupSprites [1].letter;
				}
				else {
					newLetter = "";
				}
			}
			else if (newLetter == "N") {
				if (!itemsBlinking) {
					StartCoroutine ("BlinkAllSprites");
				}
				return;
			}
			else if (newLetter == "X") {
				//blink POTD item counts
				if (!itemCountsBlinking) {
					StartCoroutine ("BlinkAllPOTDCounts");
				}
				return;
			}
		}

		Sprite newSprite = lettertoSprite (newLetter);
		if (newSprite == null || !letter.canBeHarvested) {
			harvestLetterDisplayed = "";
			harvestBox.color = Color.clear;
			if (powerupSprites.Count >= 1) {
				if (powerupSprites [0] != null)
					powerupSprites [0].image.color = Color.clear;
				if (animationInProgress == 0)
					trashCan.GetComponent<Image> ().color = Color.clear;
			}

			stopBlink = true;
		} 
		else {
			harvestLetterDisplayed = newLetter;
			harvestBox.color = new Color (1, 0.25f, 0.25f);

			PowerupUI newPowerupUI;
			if (powerupSprites.Count < 1) {
				newPowerupUI = CreateNewPowerupUI (newLetter);
				powerupSprites.Insert (0, newPowerupUI);
			} 
			else if (powerupSprites [0] == null) {
				newPowerupUI = CreateNewPowerupUI (newLetter);
				powerupSprites [0] = newPowerupUI;
			}

			powerupSprites [0].setSprite (newSprite);
			powerupSprites [0].letter = newLetter;
			powerupSprites [0].image.color = Color.white;
			powerupSprites [0].count = count;

			if (powerupSprites.Count >= 3) {
				trashCan.GetComponent<Image> ().color = Color.white;
			}

			StopCoroutine ("blinkHarvestLetter");
			StartCoroutine ("blinkHarvestLetter");
		}
	}

	public void setPowerupText(string letter){
		string text = "";
		switch (letter.ToUpper ()) {
		case "A":
			text = "<b>A</b>ir";
			break;
		case "B":
			text = "<b>B</b>ouncy Platform";
			break;
		case "C":
			text = "<b>C</b>opy";
			break;
		case "D":
			text = "<b>D</b>ouble Jump";
			break;
		case "E":
			text = "<b>E</b>raser";
			break;
		case "F":
			text = "<b>F</b>loating Platform";
			break;
		case "G":
			text = "<b>G</b>rapple";
			break;
		case "H":
			text = "<b>H</b>igher";
			break;
		case "I":
			text = "<b>I</b>ce";
			break;
		case "J":
			text = "<b>J</b>ump Higher";
			break;
		case "K":
			text = "<b>K</b>ey";
			break;
		case "L":
			text = "<b>L</b>ower";
			break;
		case "M":
			text = "<b>M</b>ake Usable";
			break;
		case "N":
			text = "<b>N</b>one";
			break;
		case "O":
			text = "<b>O</b>pposite";
			break;
		case "P":
			text = "<b>P</b>lay Again";
			break;
		case "Q":
			text = "<b>Q</b>uick";
			break;
		case "R":
			text = "<b>R</b>otate";
			break;
		case "S":
			text = "<b>S</b>low";
			break;
		case "T":
			text = "<b>T</b>imer";
			break;
		case "U":
			text = "<b>U</b>mbrella";
			break;
		case "V":
			text = "<b>V</b>apor";
			break;
		case "W":
			text = "<b>W</b>all Grab";
			break;
		case "X":
			text = "<b>X</b>tra";
			break;
		case "Y":
			text = "<b>Y</b>ank";
			break;
		case "Z":
			text = "<b>Z</b>oom";
			break;
		default:
			break;
		}
		newPowerupTextBox.text = text;
		StopCoroutine ("ShowAndFadeNewPowerupText");
		StartCoroutine ("ShowAndFadeNewPowerupText");
	}

	IEnumerator ShowAndFadeNewPowerupText(){
		float alpha = 1f;
		PlacePowerupText ();
		newPowerupTextBox.color = new Color(0.4f, 0.4f, 0.14f, 1f);

		for (int i = 0; i < 10; i++) {
			if (cameraController.isPanning || !cameraController.player_in_frame) {
				newPowerupTextBox.color = new Color (0.4f, 0.4f, 0.14f, 0f);
				i--;
			}
			else {
				newPowerupTextBox.color = new Color (0.4f, 0.4f, 0.14f, 1f);
			}

			yield return new WaitForSeconds (.1f);
		}

		while (alpha >= 0) {
			while(cameraController.isPanning || !cameraController.player_in_frame){
				newPowerupTextBox.color = new Color(0.4f, 0.4f, 0.14f, 0f);
				yield return new WaitForEndOfFrame ();
			}

			Color color = newPowerupTextBox.color;
			alpha -= 0.1f;
			color.a = alpha;
			newPowerupTextBox.color = color;

			yield return new WaitForSeconds (0.1f);
		}
		yield return null;
	}

	void PlacePowerupText(){
		Vector3 offset = cameraController.offset_from_player;
		offset.y = (offset.y + newPowerupTextBox_initial_offset) * (rt.rect.height / (2*cameraController.getSize ()));
		offset.x = (offset.x) * (rt.rect.width / (2*cameraController.getSize () * cameraController.getAspect ()));

		newPowerupTextBox.rectTransform.anchoredPosition = new Vector2 (offset.x, offset.y);

		if (cameraController.isPanning || !cameraController.player_in_frame) {
			newPowerupTextBox.color = new Color(0.4f, 0.4f, 0.14f, 0f);
		}
	}

	/***** EVENT HANDLING ********/
	public void c_newLetterEvent(object sender, NewLetterEvent e){
		updateHarvest (e.PassedLetter);
	}

	public void c_ItemChangeEvent(object sender, NewLetterEvent e){
		if (e.ChangeAmount == 0) {
			setPowerupText (e.PassedLetter.letter);
			if (e.PassedLetter.letter == "N") {
				removePowerups ();
			}
			else if (e.PassedLetter.letter == "X") {
				IncreaseAllPOTDs ();
			}
			else {
				advancePowerups (0);
			}
		}
		else if (e.ChangeAmount == 1) {
			increasePOTDItem (e.PassedLetter.letter);
		}
		else if (e.ChangeAmount == -1) {
			decreasePOTDItem (e.PassedLetter.letter, e.Index);
		}
		else if (e.ChangeAmount > 1) {
			//if replicating a number that has more than 1 instance
			powerupSprites[0].count = e.ChangeAmount;
			advancePowerups (0);
		}
		//if < -1, ignore
	}

	/********* CLEAN UP ********/
	void OnDestroy(){
		StopAllCoroutines ();
	}

}
