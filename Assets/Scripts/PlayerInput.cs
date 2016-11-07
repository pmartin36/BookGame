using UnityEngine;
using System.Collections;

public class PlayerInput : MonoBehaviour {

	public PlayerController pc;
	public Menu menu;
	public GameManager gm;

	bool mousetrack = false;


	public bool MenuOpen { get; set; }

	// Use this for initialization
	void Start () {
		//Cursor.visible = false;
	}

	// Update is called once per frame
	void Update () {

		float horizontal;
		float vertical;
		if (mousetrack && (pc.bellows_active || pc.throwable_active)) {
			Vector3 mpos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
			horizontal = mpos.x;
			vertical = mpos.y;
		}
		else {
			if (GameManager.MenuOpen) {
				horizontal = Input.GetAxisRaw ("Horizontal");
				vertical = Input.GetAxisRaw ("Vertical");
			}
			else {
				horizontal = Input.GetAxis ("Horizontal");
				vertical = Input.GetAxis ("Vertical");
			}
		}

		if(Input.GetButtonDown("PlayPause")){
			mousetrack = false;
			menu.openClose (!GameManager.MenuOpen);
		}

		if (GameManager.MenuOpen) {
			//GameManager.SetTimeScale (0);
			if (Input.GetButtonDown ("Harvest")) {
				menu.back ();
			}
			else if (Input.GetButtonDown ("Jump")) {
				menu.select ();
			}
			else {
				menu.moveCursor (vertical);
				menu.useHorizontal (horizontal);
			}
		}
		else if (GameManager.LevelComplete) {
			if (Input.GetButtonDown ("Jump")) {
				GameManager.LoadNextLevel ();
			}
		}
		else {
			if (Input.GetButtonDown ("Restart")) {
				GameManager.ResetLevel ();
			}

			bool harvesting = Input.GetButtonDown ("Harvest");
			if (harvesting) {
				pc.startHarvest ();
				return;
			}
				
			pc.horizontal = horizontal;
			pc.vertical = vertical;

			pc.horizontal_move ();

			if (Input.GetButtonDown ("Jump")) {
				pc.jumpPressed (horizontal);
			}
			if (Input.GetButtonUp ("Jump")) {
				pc.jumpReleased ();
			}

			if (Input.GetButtonDown ("Item1")) {
				mousetrack = false;
				pc.Item1Down ();
			}
			else if (Input.GetMouseButtonDown (0)) {
				mousetrack = true;
				pc.Item1Down (true);
			}
			if (Input.GetButtonUp ("Item1")) {
				pc.Item1Up ();
				mousetrack = false;
			}
			else if (Input.GetMouseButtonUp (0)) {
				pc.Item1Up ();
				mousetrack = false;
			}

			if (Input.GetButtonDown ("Item2")) {
				mousetrack = false;
				pc.Item2Down ();
			}
			else if (Input.GetMouseButtonDown (1)) {
				mousetrack = true;
				pc.Item2Down (true);
			}
			if (Input.GetButtonUp ("Item2")) {
				mousetrack = false;
				pc.Item2Up ();
			}
			else if (Input.GetMouseButtonUp (1)) {
				mousetrack = false;
				pc.Item2Up ();
			}

			//Cheat Codes
			if (Input.GetButtonDown ("Previous")) {
				GameManager.LoadPreviousLevel ();
			}
			else if (Input.GetButtonDown ("Next")) {
				GameManager.LoadNextLevel ();
			}
		}
	}

	void OnDestroy(){
		StopAllCoroutines ();
	}
}
