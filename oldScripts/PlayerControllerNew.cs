using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class PlayerControllerNew : MonoBehaviour, LetterEventInterface {

	private PlayerPhysicsNew pphysics;
	private SpriteRenderer spriteRenderer;
	private Animator anim;

	public float horizontal;
	public float vertical;

	private int jumpCount = 0;
	private int jumpCountMax = 1;
	private float jumpTime;
	private bool jumpDown = false;

	public Letter harvestableLetter;

	private bool collidingWithLetter = false;

	public List<Powerup> powerups = new List<Powerup>();

	public bool playerHasControl = true;

	private CameraController mainCamera;
	private Vector3 lastPosition = Vector3.zero;
	private bool focusOnPlayer = true;

	private float directionFacing = -1f;
	public event EventHandler<NewLetterEvent> PowerupChange;

	private bool sliding = false;
	public bool onStringItem = false;

	private List<AudioSource> audio;

	//Item effects
	private GameObject bellows;
	private bool bellows_active = false;

	private Throwable throwable;
	private float throwable_angle;
	private string throwable_letter = "";
	[SerializeField]
	private GameObject throwable_prefab;

	public GameObject springPrefab;

	private float baseMovespeed;
	[SerializeField]
	private TrailRenderer mstrail;

	GameObject collidingKeyhole;
	//End Item Effects

	void Awake(){
		
	}

	// Use this for initialization
	void Start () {
		spriteRenderer = GetComponent<SpriteRenderer> ();
		pphysics = GetComponent<PlayerPhysicsNew> ();

		//baseMovespeed = pphysics.movespeed;
		jumpTime = 0;

		mainCamera = Camera.main.GetComponent<CameraController> ();
		mainCamera.setLocation (transform.position.x, transform.position.y);

		//items
		bellows = GameObject.Find("bellows");
		bellows.SetActive (false);

		audio = new List<AudioSource>(GetComponents<AudioSource> ());
		anim = GetComponent<Animator> ();

		GameManager.signUpForNewLetterEvent (this);
	}
	
	// Update is called once per frame
	void Update () {
		if (lastPosition != Vector3.zero) {
			mainCamera.setLocation (transform.position.x, transform.position.y);
		}
		lastPosition = this.transform.position;
	}

	public void resetJumpCount(){

	}

	public void resetItemAvailability(int index){
		//0 is reset 0, 1 is reset 1, 3 is reset both
		if ((index == 1 || index == 3) && powerups.Count > 1 && powerups[1] != null) {
			powerups[1].available = true;
		}
		if ((index == 0 || index == 3) && powerups.Count > 0 && powerups[0] != null) {
			powerups[0].available = true;
		}
	}

	void setBellowsLocation(float horizontal, float vertical){
		if (bellows_active) {
			float currentRotation = this.bellows.transform.rotation.eulerAngles.z;
			float stickRotation = 0;
			Vector3 location;

			if (Mathf.Abs (horizontal) + Mathf.Abs (vertical) > 0.5f) {
				//we will be making changes to the angle
				Vector3 directionalVector = new Vector3 (horizontal, vertical);
				stickRotation = Vector3.Angle (Vector3.right, directionalVector);
				//getting the cross product will tell us whether we are going clockwise or ccw
				//if it's ccw, we set it to -angle
				Vector3 cross = Vector3.Cross (Vector3.right, directionalVector);
				if (cross.z < 0) {
					stickRotation *= -1;
				}

				if (horizontal > 0) {
					transform.localScale = new Vector3 (-Mathf.Abs (transform.localScale.x), transform.localScale.y, transform.localScale.z);
				}
				else if (horizontal < 0) {
					transform.localScale = new Vector3 (Mathf.Abs (transform.localScale.x), transform.localScale.y, transform.localScale.z);
				}

				bellows.transform.Rotate (Vector3.forward, stickRotation - currentRotation);
				location = Vector3.Normalize (new Vector3 (horizontal, vertical, 0));
			}
			else {
				//angle will not change
				Vector2 vector = GameManager.angleToVector(currentRotation);
				location = Vector3.Normalize(new Vector3 (vector.x, vector.y, 0));
			}
				
			bellows.transform.position = transform.position + location;
		}
	}

	void setThrowable(float horizontal, float vertical){
		if (throwable_letter!="") {

			float stickRotation;
			float orientation = Mathf.Sign(transform.localScale.x);
			if (Mathf.Abs (horizontal) + Mathf.Abs (vertical) > 0.5f) {
				//we will be making changes to the angle
				Vector3 directionalVector = new Vector3 (horizontal, vertical);
				stickRotation = Vector3.Angle (Vector3.right, directionalVector);
				//getting the cross product will tell us whether we are going clockwise or ccw
				//if it's ccw, we set it to -angle
				Vector3 cross = Vector3.Cross (Vector3.right, directionalVector);
				if (cross.z < 0) {
					stickRotation *= -1;
				}

				if (horizontal > 0) {
					transform.localScale = new Vector3 (-Mathf.Abs (transform.localScale.x), transform.localScale.y, transform.localScale.z);
				}
				else if (horizontal < 0) {
					transform.localScale = new Vector3 (Mathf.Abs (transform.localScale.x), transform.localScale.y, transform.localScale.z);
				}

				float previousangle = throwable_angle;
				float newAngle = stickRotation + 180;
				//float newAngle = GameManager.vectorToAngle(Throwable.ComputeTrajectoryAngle(stickRotation+180));

				if (throwable == null) {
					//set the new throwable angle
					throwable_angle = newAngle;

					//spawn new
					createThrowable (throwable_letter, true);
				}
				else if ((Mathf.Abs (newAngle - throwable_angle) > 20f && (Time.time - throwable.LaunchTime) > 0.5f) || orientation != Mathf.Sign (transform.localScale.x)) {
					//set the new throwable angle
					throwable_angle = newAngle;

					//destroy current 
					DestroyImmediate (throwable.gameObject);

					//spawn new
					createThrowable (throwable_letter, true);
				}
				else {
					//it's not null and a new one is not getting created
					throwable.AdjustTrailIfTrace(transform.position);
				}
			}
			else if (Mathf.Abs (horizontal) + Mathf.Abs (vertical) < 0.1f) {
				if (throwable != null) {
					//destroy current 
					DestroyImmediate (throwable.gameObject);
				}
			}
		}
	}

	void createThrowable(string l, bool isTrace){
		GameObject t = Instantiate (throwable_prefab) as GameObject;
		t.transform.parent = this.transform;
		t.transform.localPosition = new Vector3 (spriteRenderer.bounds.extents.x, 0, 0);

		Throwable th = t.GetComponent<Throwable> ();
		th.Init ();
		th.setLetter (l);
		th.setTrace (isTrace);

		//if it's a trace, launch it
		if (isTrace) {
			throwable = th;
			th.launch (throwable_angle, transform.position);
		}
	}

	public void setPlayerActive(bool active){
		playerHasControl = active;
	}

	public void startHarvest(){		
		if (harvestableLetter != null && harvestableLetter.canBeHarvested) {

			//add letter power to power array
			StartCoroutine (harvestLetter ());
		}
	}

	public void jumpPressed(float horizontal = 0){
		if (playerHasControl) {
			if (!jumpDown && jumpCount < jumpCountMax) { //jump just starting
				jumpTime = Time.time;
				jumpDown = true;
				jumpCount++;
				//pphysics.jump (horizontal);

				//play audio for jumping
				//playAudio(0);
				anim.SetBool ("isJumping", true);
			}
		}
	}

	public void jumpReleased(){
		jumpDown = false;
	}

	public void horizontal_move(bool jumpCountOverride = false){
		//Vector2 velocity = pphysics.getVelocity();
		if (playerHasControl) {
			if (jumpCount < 1 || jumpCountOverride) {
				if( horizontal * transform.localScale.x < 0){ //different directions, swap player orientation
					transform.localScale = new Vector3(Mathf.Sign(horizontal) * Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
					directionFacing = Mathf.Sign(horizontal);

					if (onStringItem) {
						//pphysics.resetStringPosition ();
					}
				}

				if (!onStringItem) {
					bool shouldOverride = Mathf.Abs (horizontal) > 0.3f && jumpCountOverride;
					//velocity = pphysics.horizontal_move (horizontal, shouldOverride);
				}
			}
		}

//		if (Mathf.Abs(velocity.x) > baseMovespeed ) {
//			mstrail.enabled = true;
//		}
//		else {
//			mstrail.enabled = false;
//		}
	}

	public void ProcessDirectionalInput(Vector2 inputs){
		pphysics.Move (inputs);
	}

	public void c_newLetterEvent(object sender, NewLetterEvent e){
		collidingWithLetter = true;
		Letter newLetter = e.PassedLetter;
		//leaving letter
		if (newLetter == null) {
			collidingWithLetter = false;
			harvestableLetter = null;
		} 
		//landing on letter
		else {
			collidingWithLetter = true;
			harvestableLetter = newLetter.canBeHarvested ? newLetter : null;

			resetJumpCount ();
			resetItemAvailability (3);
		}
	}
		
	void sendHandlerEvent(Letter letter, int changeAmount){
		EventHandler<NewLetterEvent> handler = PowerupChange;
		if (handler != null) {
			handler (this, new NewLetterEvent (letter,changeAmount));
		}
	}

	IEnumerator harvestLetter(){
		//signal the start of the harvest process
		setPlayerActive (false);
		anim.SetBool ("harvesting", true);
		harvestableLetter.setAsHarvested ();

		yield return new WaitForSeconds (1f);

		anim.SetBool ("harvesting", false);

		//if the letter disappeared while we were harvesting
		if (!harvestableLetter.HasDisappeared) {
			bool newPowerup = false;
			string l = harvestableLetter.letter;
			int powerup_count = 1;
			bool check_if_POTD = GameManager.IsPOTD (l); //add checks for all POTD items
			if (check_if_POTD && IncrementPOTDIfExists (l) != null) {	//include all place once and destroy objects 
				EventHandler<NewLetterEvent> handler = PowerupChange;
				if (handler != null) {
					handler (this, new NewLetterEvent (harvestableLetter, 1));
				}
			}
			else { //permanent buffs
				Letter eventLetter = harvestableLetter;
			
				//can only replicate the item in slot 1
				//need to switch the letter to the powerup[0] slot before we send the event
				if (harvestableLetter.letter == "R") {				
					if (powerups.Count > 0 && powerups [0] != null) {
						eventLetter.letter = powerups [0].letter.letter;
						powerup_count = powerups [0].count;
						newPowerup = true;
					}
					sendHandlerEvent (eventLetter, powerups [0].count > 1 ? powerups [0].count : 0);
				}
				//can only use the N powerup with an item in slot 1
				else if (harvestableLetter.letter == "N") {
					if (powerups.Count > 0) {
						while (powerups.Count > 0) {
							PowerupRemoved (powerups [powerups.Count - 1]);
							powerups.RemoveAt (powerups.Count - 1);
						}
					}
					sendHandlerEvent (eventLetter, 0);
				}
				//we're picking up a new powerup
				else {
					newPowerup = true;
					sendHandlerEvent (eventLetter, 0);
				}
			}

			if (newPowerup) {
				Powerup power = new Powerup (harvestableLetter);
				power.count = powerup_count;
				PowerupAdded (power);
				if (powerups.Count > 1 && powerups [0] == null) {
					//if item2 is defined but item1 is null (POTD was used)
					powerups [0] = power;
				}
				else {
					powerups.Insert (0, power);
				}

				while (powerups.Count > 2) {
					PowerupRemoved (powerups [powerups.Count - 1]);
					powerups.RemoveAt (powerups.Count - 1);
				}

				//if something is already mapped to x, store mapping as y
				if (getIndexFromMapping ("X") >= 0) {
					powerups [0].buttonMapping = "Y";
				}
				else {
					powerups [0].buttonMapping = "X";
				}
			}
		}

		harvestableLetter = null;

		if (GameManager.letters_active > 0)
			setPlayerActive (true);
		
		yield return null;
	}

	private Letter IncrementPOTDIfExists(string l){
		l = l.ToUpper ();
		foreach (Powerup p in powerups) {
			if (p.letter.letter == l) {
				p.count++;
				return p.letter;
			}
		}
		return null;
	}

	private void DecreasePOTD(int index){
		if (powerups [index].count > 0) {
			powerups [index].count--;
			EventHandler<NewLetterEvent> handler = PowerupChange;
			if (handler != null) {
				handler (this, new NewLetterEvent (powerups [index].letter, -1, index+1));
			}
		}
	}

	private int getIndexFromMapping(string mapping){
		foreach (Powerup p in powerups) {
			if (p != null && p.buttonMapping == mapping)
				return powerups.IndexOf (p);
		}
		return -1;
	}



	public void Item1Down(){
		if (playerHasControl) {
			int index = getIndexFromMapping ("X");
			if (index < 0) return;

			if (powerups.Count >= 1 && powerups[index].available){
				powerups [index].inUse = true;
				if (!GameManager.IsPOTD(powerups[index].letter.letter)) {
					powerups [index].available = false;
				}
				ItemDown (index);
			}
		}
	}

	public void Item1Up(){
		int index = getIndexFromMapping ("X");
		if (index < 0) return;

		if (powerups[index].inUse) {						
			powerups[index].inUse = false;
			ItemUp (index);
		}
	}

	public void Item2Down(){
		if (playerHasControl) {
			int index = getIndexFromMapping ("Y");
			if (index < 0) return;

			if (powerups.Count >= 1 && powerups[index].available){
				powerups [index].inUse = true;
				if (!GameManager.IsPOTD(powerups[index].letter.letter)) {
					powerups [index].available = false;
				}
				ItemDown (index);
			}
		}
	}

	public void Item2Up(){
		int index = getIndexFromMapping ("Y");
		if (index < 0) return;

		if (powerups[index].inUse) {			
			powerups[index].inUse = false;
			ItemUp (index);
		}
	}

	void ItemDown(int index){
		if (GameManager.IsPOTD (powerups [index].letter.letter) && powerups [index].count <= 0) {
			return;
		}
		switch (powerups[index].letter.letter) {
		case "A":
			setPlayerActive (false);
			bellows_active = true;
			bellows.SetActive (true);
			anim.SetBool ("bellowActive", true);
			setBellowsLocation (0, 0);
			break;
		case "B":
			if (!onStringItem) {
				GameObject balloon = Instantiate (Resources.Load<GameObject> ("Prefabs/balloon"), transform.position + new Vector3 (directionFacing * 0.1f, 0), Quaternion.identity) as GameObject;
				DecreasePOTD (index);
			}
			break;
		case "C":
//			if (pphysics.isAirborn()) {
//				GameObject cloud = Instantiate (Resources.Load<GameObject> ("Prefabs/cloud"), transform.position + new Vector3 (0, -spriteRenderer.bounds.extents.y * 1.1f), Quaternion.identity) as GameObject;
//				DecreasePOTD (index);
//			}
			break;
		case "D":
			jumpDown = false;
			jumpPressed ();
			break;
		case "E":
			initThrowable (powerups [index].letter.letter);
			break;
		case "F":
			break;
		case "G":
			break;
		case "H":
			initThrowable (powerups [index].letter.letter);
			break;
		case "I":
			break;
		case "J":
			break;
		case "K":
			StartCoroutine (useKey (index));
			break;
		case "L":
			initThrowable (powerups [index].letter.letter);
			break;
		case "M":
			initThrowable (powerups [index].letter.letter);
			break;
		case "N":
			break;
		case "O":
			break;
		case "P":
			break;
		case "Q":
			break;
		case "R":
			break;
		case "S":
			//place spring
//			if(pphysics.isAirborn()){
//				GameObject spring = Instantiate (Resources.Load<GameObject>("Prefabs/Spring"), transform.position, Quaternion.identity) as GameObject;
//				SpriteRenderer sr = spring.GetComponent<SpriteRenderer> ();
//				spring.transform.position = spring.transform.position + new Vector3 (0, -spriteRenderer.bounds.extents.y);
//
//				DecreasePOTD (index);
//			}
			break;
		case "T":
			break;
		case "U":
//			if ( !onStringItem && pphysics.isAirborn() ) {
//				GameObject umbrella = Instantiate (Resources.Load<GameObject> ("Prefabs/umbrella"), transform.position + new Vector3 (directionFacing * 0.1f, 0), Quaternion.identity) as GameObject;
//			}
			break;
		case "V":
			
			break;
		case "W":
			break;
		case "X":
			break;
		case "Y":
			break;
		case "Z":
			mainCamera.cameraToLevelSize ();
			break;
		default:
			break;
		}
	}

	void ItemUp(int index){
		switch (powerups[index].letter.letter) {
		case "A":
			StartCoroutine(useBellows ());
			break;
		case "B":
			break;
		case "C":
			break;
		case "D":
			jumpDown = false;
			break;
		case "E":
			StartCoroutine(throwThrowable(index));
			break;
		case "F":
			break;
		case "G":
			break;
		case "H":
			StartCoroutine(throwThrowable(index));
			break;
		case "I":
			break;
		case "J":
			break;
		case "K":
			break;
		case "L":
			StartCoroutine(throwThrowable(index));
			break;
		case "M":
			StartCoroutine(throwThrowable(index));
			break;
		case "N":
			break;
		case "O":
			break;
		case "P":
			break;
		case "Q":
			break;
		case "R":
			break;
		case "S":
			break;
		case "T":
			break;
		case "U":
			//destroy
//			pphysics.exitedStringItemState();
			DestroyImmediate(GameObject.FindObjectOfType<Umbrella>().gameObject);
			break;
		case "V":			
			break;
		case "W":
			break;
		case "X":
			break;
		case "Y":
			break;
		case "Z":
			mainCamera.restoreCamera (true);
			powerups [index].available = true;
			break;
		default:
			break;
		}
	}

	void PowerupAdded(Powerup p){
		//if a letter is not on the list, nothing has to be done when it is added
		switch (p.letter.letter) {
		case "D":
			jumpCountMax++;
			break;
		case "E":
			break;
		case "F":
//			pphysics.movespeed *= 2f;
			mstrail.gameObject.SetActive(true);
			break;
		case "G":
			
			break;
		case "H":
			break;
		case "I":
//			pphysics.IceMove = true;
			break;
		case "J":
//			pphysics.jumpSpeed *= 3f;
			break;
		case "K":
			break;
		case "L":
			break;
		case "M":
			break;
		case "N":
			break;
		case "O":
			break;
		case "P":
			break;
		case "Q":
//			pphysics.movespeed *= 0.5f;
			break;
		case "R":
			break;
		case "S":
			break;
		case "T":
			break;
		case "U":
			break;
		case "V":
			break;
		case "W":
			break;
		case "X":
			break;
		case "Y":
			break;
		case "Z":
			break;
		default:
			break;
		}
	}

	void PowerupRemoved(Powerup p){
		//if a letter is not on the list, nothing has to be done when it is removed
		switch (p.letter.letter) {
		case "D":
			jumpCountMax--;
			break;
		case "E":
			break;
		case "F":
//			pphysics.movespeed /= 2f;
			mstrail.gameObject.SetActive(false);
			break;
		case "G":
			
			break;
		case "H":
			break;
		case "I":
//			pphysics.IceMove = false;
			break;
		case "J":
//			pphysics.jumpSpeed /= 3f;
			break;
		case "K":
			break;
		case "L":
			break;
		case "M":
			break;
		case "N":
			break;
		case "O":
			break;
		case "P":
			break;
		case "Q":
//			pphysics.movespeed /= 0.5f;
			break;
		case "R":
			break;
		case "S":
			break;
		case "T":
			break;
		case "U":
			break;
		case "V":
			break;
		case "W":
			break;
		case "X":
			break;
		case "Y":
			break;
		case "Z":
			mainCamera.restoreCamera (false);
			break;
		default:
			break;
		}
	}

	//This call comes from Book OnTriggerEnter
	public void hoverTowardsPoint(Vector3 point){
		
		setPlayerActive (false);
		StartCoroutine (hoverTowards (point));
	}

	IEnumerator hoverTowards(Vector3 point){
		float startTime = Time.time;
		Vector3 startingPosition = transform.position;
		float startingSize = mainCamera.getSize ();

		//mainCamera.setLocation (point.x, point.y);
		//mainCamera.freezeCamera = true;

		float timeDif = 0;
		while (transform.position != point && (timeDif/1f <= 1f)) {
			timeDif = Time.time - startTime;
			transform.position = Vector3.Lerp (startingPosition, point, timeDif / 1f);
			//mainCamera.setSize(Mathf.Lerp(startingSize,1.5f,Time.time - startTime / 1));
			yield return new WaitForSeconds (0.001f);
		}
		jumpTime = 1000;
//		pphysics.zeroVelocity ();
		GameManager.endLevel ();
		yield return null;
	}

	public void playAudio(int index){
		audio [index].Play ();
	}

	IEnumerator useBellows(){
		SpriteRenderer sr = bellows.GetComponent<SpriteRenderer> ();
		setPlayerActive (true);
		anim.SetBool ("bellowActive", false);

		//we use the opposite of how we calculated the angle because we want to blow in the opposite direction
		//pphysics.ApplyForce (bellows.transform.rotation * Vector3.left * 300);

		bellows_active = false;
		Sprite sprite = Resources.Load<Sprite> ("Sprites/bellows_compressed_with_air");
		sr.sprite = sprite;
		if (jumpCount == 0)
			jumpCount = 1;

		yield return new WaitForSeconds(0.5f);

		sr.sprite = Resources.Load<Sprite> ("Sprites/bellows");
		bellows.SetActive (false);
		yield return null;
	}

	IEnumerator useKey(int index){
		playerHasControl = false;

		if (collidingKeyhole != null) {
			DecreasePOTD (index);
			DestroyImmediate (collidingKeyhole.transform.parent.gameObject);
		}

		playerHasControl = true;
		yield return null;
	}

	void initThrowable(string letter){
		playerHasControl = false;
		createThrowable (letter, false);
		throwable_letter = letter;
	}

	IEnumerator throwThrowable(int index){
		throwable_letter = "";

		if (Mathf.Abs (horizontal) + Mathf.Abs (vertical) > 0.5f) {
			//set player to throw sprite
			//

			//throw
			if (throwable != null) {
				DestroyImmediate (throwable.gameObject);
			}

			GameObject[] throwables = GameObject.FindGameObjectsWithTag ("Throwable");
			foreach (GameObject g in throwables) {
				Throwable t = g.GetComponent<Throwable> ();
				if (t != null && t.getKinematic ()) {
					t.launch (throwable_angle);
				}
			}

			//start throw animation
			//

			DecreasePOTD (index);

			yield return new WaitForSeconds (0.5f);
		}
		else {
			GameObject[] throwables = GameObject.FindGameObjectsWithTag ("Throwable");
			foreach (GameObject g in throwables) {
				DestroyImmediate (g);
			}
		}
		//set player back to idle
		//

		playerHasControl = true;

		yield return null;
	}

	void OnTriggerEnter2D(Collider2D other){
		if (other.tag == "Keyhole") {
			collidingKeyhole = other.gameObject;
		}
	}

	void OnTriggerExit2D(Collider2D other){
		if (other.tag == "Keyhole") {
			collidingKeyhole = null;
		}
	}

	void OnDestroy(){
		StopAllCoroutines ();
	}
}

