using UnityEngine;
using System.Collections;

public class PlayerInputNew : MonoBehaviour {

	public PlayerControllerNew pc;
	public Menu menu;
	public GameManager gm;

	public bool MenuOpen { get; set; }

	// Use this for initialization
	void Start () {
		
	}

	// Update is called once per frame
	void Update () {
		float horizontal = Input.GetAxis ("Horizontal");
		float vertical = Input.GetAxis ("Vertical");


		if(Input.GetButtonDown("PlayPause")){
			MenuOpen = !MenuOpen;
			menu.openClose (MenuOpen);
		}

		if (MenuOpen) {
			if (Input.GetButtonDown ("Harvest")) {
				MenuOpen = menu.back ();
			}
			else if (Input.GetButtonDown ("Jump")) {
				MenuOpen = menu.select ();
			}
			else {
				menu.moveCursor (vertical);
			}
			Time.timeScale = 0;
		}
		else {
			Time.timeScale = 1;

			if (Input.GetButtonDown ("Restart")) {
				gm.ResetLevel ();
			}

			bool harvesting = Input.GetButtonDown ("Harvest");
			if (harvesting) {
				pc.startHarvest ();
				return;
			}

			pc.ProcessDirectionalInput (new Vector2(horizontal,vertical));

			if (Input.GetButtonDown ("Jump")) {
				pc.jumpPressed (horizontal);
			}
			if (Input.GetButtonUp ("Jump")) {
				pc.jumpReleased ();
			}

			if (Input.GetButtonDown ("Item1")) {
				pc.Item1Down ();
			}
			if (Input.GetButtonUp ("Item1")) {
				pc.Item1Up ();
			}

			if (Input.GetButtonDown ("Item2")) {
				pc.Item2Down ();
			}
			if (Input.GetButtonUp ("Item2")) {
				pc.Item2Up ();
			}
		}
	}

	void OnDestroy(){
		StopAllCoroutines ();
	}
}
