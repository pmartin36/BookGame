using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;


public class GameManagerNew : MonoBehaviour {

	ButtonUI buttonUI;
	private int currentLevel = 0;
	public static int letters_active = 0;

	void Awake(){
		
	}

	// Use this for initialization
	void Start () {
		buttonUI = GameObject.FindObjectOfType<ButtonUI> ();

		PlayerController player = GameObject.FindGameObjectWithTag ("Player").GetComponent<PlayerController> ();
		//player.PowerupChange += buttonUI.c_ItemChangeEvent;
	}
	
	// Update is called once per frame
	void Update () {
		/*
		GameObject[] p = GameObject.FindGameObjectsWithTag ("Player");
		PlayerPhysics wtf = p[0].GetComponent<PlayerPhysics> ();
		Debug.Log (p.Length);
		*/
	}

	public void ResetLevel(){
		letters_active = 0;
		SceneManager.LoadScene (SceneManager.GetActiveScene().buildIndex);
	}

	public void Exit(){
		Application.Quit ();
	}

	void OnDestroy(){
		StopAllCoroutines ();
	}

	public static bool IsPOTD(string l){
		l = l.ToUpper ();
		return l == "B" || l == "C" || l == "E" || l == "H" || l == "K" || l == "L" || l == "M" || l == "S"; //add POTD as we go along
	}

	public static bool IsPowerupModifier(string l){
		l = l.ToUpper ();
		return l == "R" || l == "N";
	}

	public static bool IsPassive(string l){
		l = l.ToUpper ();
		return l == "I" || l == "F" || l == "J" || l == "O" || l == "Q";
	}

	public static Vector2 angleToVector(float angle){
		return new Vector2(Mathf.Cos(Mathf.Deg2Rad * angle), Mathf.Sin(Mathf.Deg2Rad * angle));
	}

	public static float vectorToAngle(Vector2 vector){
		return Mathf.Atan(vector.y/vector.x) * Mathf.Rad2Deg;
	}

	public static void signUpForNewLetterEvent(LetterEventInterface lei){
		PlayerPhysics pp = GameObject.FindGameObjectWithTag ("Player").GetComponent<PlayerPhysics> ();
		//pp.NewLetter += lei.c_newLetterEvent;
	}

	public static void removeSignUpForNewLetterEvent(LetterEventInterface lei){
		PlayerPhysics pp = GameObject.FindGameObjectWithTag ("Player").GetComponent<PlayerPhysics> ();
		//pp.NewLetter -= lei.c_newLetterEvent;
	}

	public static void increment_letters_active(){
		++letters_active;
	}

	public static void decrement_letters_active(){
		if (--letters_active <= 0) {
			GameObject.FindGameObjectWithTag ("GameManager").GetComponent<GameManagerNew>().startOpenBook();

		}
	}

	public static void endLevel(){
		GameObject.FindGameObjectWithTag ("GameManager").GetComponent<GameManagerNew> ().startCloseBook ();
	}

	void startCloseBook(){
		StartCoroutine (closeBookEndLevel());
	}

	void startOpenBook(){
		StartCoroutine (openBook());
	}

	IEnumerator closeBookEndLevel(){
		Book book = GameObject.FindGameObjectWithTag ("Book").GetComponent<Book> ();
		PlayerController player = GameObject.FindGameObjectWithTag ("Player").GetComponent<PlayerController> ();

		book.setOpen (false);
		while (book.isOpen) {
			yield return new WaitForSeconds (1);
		}
		SceneManager.LoadScene (SceneManager.GetActiveScene().buildIndex + 1);
	}

	IEnumerator openBook(){
		Book book = GameObject.FindGameObjectWithTag ("Book").GetComponent<Book> ();
		PlayerController player = GameObject.FindGameObjectWithTag ("Player").GetComponent<PlayerController> ();
		CameraController cc = Camera.main.GetComponent<CameraController> ();

		//disable player
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
}
