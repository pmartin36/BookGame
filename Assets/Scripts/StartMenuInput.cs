using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.UI;

public class StartMenuInput : MonoBehaviour {

	//public GameManager gm;
	bool mousetrack = false;
	int index = 0;

	bool disableMoveCursor = false;

	Submenu openSubmenu;

	SettingsMenu settings;
	ControlsMenu ctrls;
	GameObject main;

	List<Button> buttons;

	// Use this for initialization
	void Start () {
		//Cursor.visible = false;
		List<Button> button = new List<Button>(GameObject.FindObjectsOfType<Button>());
		Button newGameb = button.FirstOrDefault (b => b.name == "New Game");
		Button cntinueb = button.FirstOrDefault (b => b.name == "Continue");
		Button settingsb = button.FirstOrDefault (b => b.name == "Settings");
		Button controlsb = button.FirstOrDefault (b => b.name == "Controls");
		Button quitb = button.FirstOrDefault (b => b.name == "Quit");

		cntinueb.GetComponentInChildren<Text> ().color = new Color (0, 0, 0, 0.3f);

		buttons = new List<Button> (){ newGameb, cntinueb, settingsb, controlsb, quitb };

		GameObject canvas = GameObject.Find ("Canvas");
		settings = canvas.GetComponentInChildren<SettingsMenu> ();
		ctrls = canvas.GetComponentInChildren<ControlsMenu> ();

		List<GameObject> r = GameObject.FindGameObjectsWithTag ("Submenu").ToList();
		main = GameObject.FindGameObjectsWithTag ("Submenu").First (s => s.name == "Main");

		StartCoroutine(WriteTitle ());

		settings.gameObject.SetActive (false);
		ctrls.gameObject.SetActive (false);
	}

	// Update is called once per frame
	void Update () {
		float horizontal = Input.GetAxisRaw ("Horizontal");
		float vertical = Input.GetAxisRaw ("Vertical");

		if (Input.GetButtonDown ("Submit")) {
			select ();
		}
		else if (Input.GetButtonDown ("Cancel")) {
			back ();
		}

		if (openSubmenu != null) {
			openSubmenu.moveCursor (vertical);
			openSubmenu.useHorizontal (horizontal);
		}
		else {
			if (horizontal > 0.5f && index < 2) {
				SetIndex (2, false);
			}
			else if (horizontal < -0.5f && index >= 2) {
				SetIndex (0, false);
			}
			else if (Mathf.Abs (vertical) > 0.5f) {
				SetIndex (index - Math.Sign (vertical), false);
			}

			if (Mathf.Abs (horizontal) + Mathf.Abs (vertical) < 0.1f) {
				disableMoveCursor = false;
			}
		}

	}

	public bool IndexInbounds(int newindex){
		return newindex >= 0 && newindex < buttons.Count;
	}

	public void SetIndexFromMouse(int newindex){
		SetIndex (newindex, true);
	}

	public void SetIndex(int _index, bool fromMouse){
		if (fromMouse && !buttons [_index].interactable)
			return;

		if (!disableMoveCursor) {
			while (IndexInbounds (_index) && !buttons [_index].interactable) {
				_index += Math.Sign (_index - index);
			}

			if (!IndexInbounds (_index))
				return;

			buttons [index].GetComponentInChildren<Text> ().color = new Color (0.2f, 0.2f, 0.2f);
			buttons [_index].GetComponentInChildren<Text> ().color = new Color (.1f, .1f, 1f, 1f);

			index = _index;
			StartCoroutine(PauseMoveCursor());
		}
	}

	public void NewGame(){
		GameManager.LoadNextLevel ();
	}

	public void Continue(){
		if (buttons [1].interactable) {

		}
	}

	public void Settings(){
		openSubmenu = settings;
		settings.openSubmenu ();
		main.SetActive (false);
	}

	public void Controls(){
		openSubmenu = ctrls;
		ctrls.openSubmenu ();
		main.SetActive (false);
	}
		
	public void Quit(){
		GameManager.QuitApplication ();
	}

	public void select(){
		if (openSubmenu != null) {

		}
		else {
			switch (index) {
			case 0:
				NewGame ();
				break;
			case 1:
				Continue ();
				break;
			case 2:
				Settings ();
				break;
			case 3:
				Controls ();
				break;
			case 4:
				Quit ();
				break;
			default:
				break;
			}
		}
	}

	public void back(){
		if (openSubmenu != null) {
			if (openSubmenu == settings) {
				settings.SaveSettings ();
			}
			openSubmenu.gameObject.SetActive (false);
			openSubmenu = null;

			main.SetActive (true);
		}
	}

	IEnumerator PauseMoveCursor(){
		disableMoveCursor = true;
		float t = Time.realtimeSinceStartup;
		while (disableMoveCursor && Time.realtimeSinceStartup - t < 0.3f) {
			yield return null;
		}
		disableMoveCursor = false;
	}

	IEnumerator WriteTitle(){
		Material titleMaterial = GameObject.Find ("title").GetComponent<SpriteRenderer> ().sharedMaterial;
		float startTime = Time.time;
		float alpha = 0;
		while (alpha < 1) {
			alpha = Mathf.Lerp (0, 1, (Time.time - startTime) / 2f);
			titleMaterial.SetFloat("_HideRed",alpha);
			yield return new WaitForSeconds (0.01f);
		}
	}

	void OnDestroy(){
		StopAllCoroutines ();
	}
}
