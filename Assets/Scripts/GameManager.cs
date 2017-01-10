using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;


public class GameManager : MonoBehaviour {

	ButtonUI buttonUI;
	private int currentLevel = 0;
	public static int letters_active = 0;
	public static bool MenuOpen {get; set;}
	public static bool LevelComplete { get; set; }

	public GameObject levelCompleteContainer;
	public GameObject levelOngoingContainer;

	void Awake(){
		
	}

	// Use this for initialization
	void Start () {
		buttonUI = GameObject.FindObjectOfType<ButtonUI> ();

		PlayerController player = GameObject.FindGameObjectWithTag ("Player").GetComponent<PlayerController> ();
		player.PowerupChange += buttonUI.c_ItemChangeEvent;

		LevelComplete = false;
		levelCompleteContainer = GameObject.FindGameObjectWithTag ("LevelCompleteContainer");
		levelOngoingContainer = GameObject.FindGameObjectWithTag ("LevelOngoingContainer");

		levelCompleteContainer.SetActive (false);
		//levelOngoingContainer.SetActive(false);

		if(!SettingsManager.Instance.LevelHasBeenLoaded){
			CameraController cc = Camera.main.GetComponent<CameraController> ();
			cc.PlayerSpawnFadeIn ();
			SettingsManager.Instance.LevelHasBeenLoaded = true;
		}
	}
	
	// Update is called once per frame
	void Update () {

	}

	public static void ResetLevel(){
		letters_active = 0;
		SceneManager.LoadScene (SceneManager.GetActiveScene().buildIndex);
	}

	public static void sExitToMenu(){
		GameObject.FindGameObjectWithTag ("GameManager").GetComponent<GameManager> ().ExitToMenu ();
	}

	public void ExitToMenu(){
		SetMenuOpen (false);
		SettingsManager.Instance.LevelHasBeenLoaded = false;
		SceneManager.LoadScene (0);
	}

	public static void QuitApplication(){
		Application.Quit ();
	}

	void OnDestroy(){
		StopAllCoroutines ();
	}

	public static bool IsPOTD(string l){
		l = l.ToUpper ();
		return l == "B" || l == "E" || l == "F" || l == "G" || l == "H" || l == "K" || l == "L" || l == "M" || l == "Y"; //add POTD as we go along
	}

	public static bool IsPowerupModifier(string l){
		l = l.ToUpper ();
		return l == "C" || l == "N" || l == "X";
	}

	public static bool IsPassive(string l){
		l = l.ToUpper ();
		return l == "I" || l == "J" || l == "O" || l == "Q" || l == "S" || l == "T" || l == "V";
	}

	public static Vector2 angleToVector(float angle){
		return new Vector2(Mathf.Cos(Mathf.Deg2Rad * angle), Mathf.Sin(Mathf.Deg2Rad * angle));
	}

	public static float vectorToAngle(Vector2 vector){
		return Mathf.Atan(vector.y/vector.x) * Mathf.Rad2Deg;
	}

	public static void signUpForNewLetterEvent(LetterEventInterface lei){
		PlayerPhysics pp = GameObject.FindGameObjectWithTag ("Player").GetComponent<PlayerPhysics> ();
		pp.NewLetter += lei.c_newLetterEvent;
	}

	public static void removeSignUpForNewLetterEvent(LetterEventInterface lei){
		PlayerPhysics pp = GameObject.FindGameObjectWithTag ("Player").GetComponent<PlayerPhysics> ();
		pp.NewLetter -= lei.c_newLetterEvent;
	}

	public static void increment_letters_active(){
		++letters_active;
	}

	public static void decrement_letters_active(){
		if (--letters_active <= 0) {
			GameObject.FindGameObjectWithTag ("GameManager").GetComponent<GameManager>().startOpenBook();

		}
	}

	public static void endLevel(){
		GameObject.FindGameObjectWithTag ("GameManager").GetComponent<GameManager> ().startCloseBook ();
	}

	void startCloseBook(){
		StartCoroutine (closeBookEndLevel());
	}

	void startOpenBook(){
		StartCoroutine (openBook());
	}

	public static void LoadNextLevel(){
		SceneManager.LoadScene (SceneManager.GetActiveScene().buildIndex + 1);
	}

	public static void LoadPreviousLevel(){
		Scene scene = SceneManager.GetActiveScene ();
		if (scene.buildIndex > 0) {
			SceneManager.LoadScene (scene.buildIndex - 1);
		}
	}

	IEnumerator closeBookEndLevel(){
		Book book = GameObject.FindGameObjectWithTag ("Book").GetComponent<Book> ();
		PlayerController player = GameObject.FindGameObjectWithTag ("Player").GetComponent<PlayerController> ();
		CameraController cc = Camera.main.GetComponent<CameraController> ();

		/*
		book.setOpen (false);
		while (book.isOpen) {
			yield return new WaitForSeconds (1);
		}
		*/

		cc.EndLevelFade(); //finishes in 1 second
		yield return new WaitForSeconds(.75f);
		yield return StartCoroutine (book.openClose (false)); //finishes in .5 seconds
		book.playAudio (1);
		 
		//go to showing line of book -> wait for input -> end level
		SettingsManager.Instance.LevelHasBeenLoaded = false;
		GameManager.LevelComplete = true;
		levelCompleteContainer.SetActive (true);
		levelOngoingContainer.SetActive (false);
	}

	IEnumerator openBook(){
		Book book = GameObject.FindGameObjectWithTag ("Book").GetComponent<Book> ();
		PlayerController player = GameObject.FindGameObjectWithTag ("Player").GetComponent<PlayerController> ();
		CameraController cc = Camera.main.GetComponent<CameraController> ();

		//disable player
		player.playerHasControl = false;
		player.setPlayerActive(false);

		//pan to book
		cc.setPan(Camera.main.transform.position,book.transform.position);
		while (cc.isPanning) {
			yield return new WaitForSeconds (1);
		}
		//open book
		book.setOpen (true);
		while (!book.isOpen) {
			yield return new WaitForSeconds (1);
		}

		//pan to player
		cc.freezeCamera = false;
		cc.setLocation(player.transform.position.x, player.transform.position.y);

		//enable the player
		player.setPlayerActive(true);
		player.playerHasControl = true;
	}

	public static string letterToPowerupName(string letter){
		string powerupName = "";
		switch (letter.ToUpper()) {
		case "A":
			//set image for A item

			break;
		case "B":
			//set image for B item

			break;
		case "C":
			
			break;
		case "D":
			
			break;
			//case E->Z
		case "F":
			
			break;
		case "G":
			
			break;
		case "H":
			
			break;
		case "I":
			
			break;
		case "J":
			
			break;
		case "K":
			
			break;
		case "S":
			
			break;
		case "U":
			
			break;
		case "V":
			
			break;
		case "Z":
			
			break;
		default:
			
			break;
		}
		return powerupName;
	}

	public static void SetMenuOpen(bool open){
		MenuOpen = open;
		SetTimeScale (1f);
	}

	public static void SetTimeScale(float timeScale){
		if (MenuOpen) {
			Time.timeScale = 0.0000001f;
		}
		else {
			Time.timeScale = timeScale;
		}
	}
}
