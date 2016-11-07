using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public class Menu : MonoBehaviour {

	private enum Menutype{ MAIN, COMPENDIUM, CONTROLS, SETTINGS };

	private bool disableMoveCursor = false;
	private int index = 0;
	private List<Button> options = new List<Button>();

	public Material activeMaterial;
	public Material inactiveMaterial;

	float effectPosition = 0;
	Menutype currentMenu = Menutype.MAIN;

	PlayerInput inputController;

	GameObject main;
	SettingsMenu settings;
	CompendiumMenu compendium;
	ControlsMenu controls;

	// Use this for initialization
	void Start () {
		settings = GetComponentInChildren<SettingsMenu> ();
		compendium = GetComponentInChildren<CompendiumMenu> ();
		controls = GetComponentInChildren<ControlsMenu> ();
		settings.gameObject.SetActive (false);
		compendium.gameObject.SetActive (false);
		controls.gameObject.SetActive (false);

		GameObject[] optionlist = GameObject.FindGameObjectsWithTag ("Option");
		foreach (GameObject option in optionlist) {
			options.Add (option.GetComponent<Button> ());
		}
		options = options.OrderBy (y => y.transform.position.y).Reverse().ToList();

		main = GameObject.Find ("Main");

		PlayerController player = GameObject.FindGameObjectWithTag ("Player").GetComponent<PlayerController> ();
		player.PowerupChange += this.c_ItemChangeEvent;

		gameObject.SetActive (false);
	}
	
	// Update is called once per frame
	void Update () {
		effectPosition += .01f;
		options [index].image.material.SetFloat ("_EffectOffset", effectPosition);
	}

	public void moveCursor(float vertical){
		if (currentMenu == Menutype.MAIN) {
			if (disableMoveCursor && Mathf.Abs (vertical) < 0.05f) {
				disableMoveCursor = false;
			}
			else if (!disableMoveCursor && Mathf.Abs (vertical) > 0.9f) {
				int newIndex = index + (int)Mathf.Sign (-vertical);
				if (newIndex < 0)
					newIndex = 0;
				else if (newIndex >= options.Count)
					newIndex = options.Count - 1;

				StartCoroutine (PauseMoveCursor());

				setCursorPosition (newIndex);
			}
		}
		else if (currentMenu == Menutype.SETTINGS) {
			settings.moveCursor (vertical);
		}
	}

	public void useHorizontal (float horizontal){
		switch (currentMenu) {
		case Menutype.SETTINGS:
			settings.useHorizontal (horizontal);
			break;
		case Menutype.COMPENDIUM:
			compendium.useHorizontal (horizontal);
			break;
		case Menutype.CONTROLS:
			controls.useHorizontal (horizontal);
			break;
		default:
			break;
		}
	}

	public void setCursorPosition(int _index){
		if (_index != index) {
			options [index].image.material = inactiveMaterial;
			index = _index;
			options [index].image.material = activeMaterial;
		}
	}

	//returns whether the menu is still open
	public bool select(){
		bool menuStatus = true;
		if (Menutype.MAIN == currentMenu) {
			if (index == 0) {
				openClose (false);
				menuStatus = false;
			}
			else if (index == 1) {
				switchMenu ((int)Menutype.COMPENDIUM);
			}
			else if (index == 2) {
				switchMenu ((int)Menutype.CONTROLS);
			}
			else if (index == 3) {
				switchMenu ((int)Menutype.SETTINGS);
			}
			else {
				GameManager.sExitToMenu ();
			}
		}
		else if(Menutype.COMPENDIUM == currentMenu){

		}
		else if(Menutype.SETTINGS == currentMenu){

		}
		return menuStatus;
	}

	public bool back(){
		bool menuStatus = true;
		switch(currentMenu){
		case Menutype.MAIN:
			openClose (false);
			menuStatus = false;
			break;
		case Menutype.SETTINGS:
			settings.SaveSettings ();
			switchMenu ((int)Menutype.MAIN);
			break;
		case Menutype.COMPENDIUM:
			switchMenu ((int)Menutype.MAIN);
			break;
		case Menutype.CONTROLS:
			switchMenu ((int)Menutype.MAIN);
			break;
		}
		return menuStatus;
	}

	public void switchMenu(int type){
		Menutype mt = (Menutype)type;
		switch(mt){
		case Menutype.MAIN:
			Submenu s = settings.gameObject.activeInHierarchy ? (Submenu)settings : 
				(compendium.gameObject.activeInHierarchy ? (Submenu)compendium : (Submenu)controls);
			s.gameObject.SetActive (false);
			main.SetActive (true);
			break;
		case Menutype.SETTINGS:
			settings.openSubmenu ();
			main.SetActive (false);
			break;
		case Menutype.COMPENDIUM:
			compendium.openSubmenu ();
			main.SetActive (false);
			break;
		case Menutype.CONTROLS:
			controls.openSubmenu ();
			main.SetActive (false);
			break;
		}
		currentMenu = mt;
	}

	private PlayerInput GetInputController(){
		return GameObject.FindGameObjectWithTag ("Input").GetComponent<PlayerInput> ();
	}

	public void openClose(bool shouldOpen){
		switchMenu ((int)Menutype.MAIN);
		gameObject.SetActive (shouldOpen);
		GameManager.SetMenuOpen (shouldOpen);

		if (shouldOpen) {
			options [index].image.material = activeMaterial;
		}
		else {
			options [index].image.material = inactiveMaterial;
			index = 0;
		}
	}

	IEnumerator PauseMoveCursor(){
		disableMoveCursor = true;
		//yield return new WaitForSeconds(0.1f * Time.deltaTime);
		float t = Time.realtimeSinceStartup;
		while (disableMoveCursor && Time.realtimeSinceStartup - t < 0.3f) {
			yield return null;
		}
		disableMoveCursor = false;
	}

	public void c_ItemChangeEvent(object sender, NewLetterEvent e){
		string s = e.PassedLetter.letter;
		if (!SettingsManager.Instance.collectedLetters.Contains (s)) {
			SettingsManager.Instance.collectedLetters.Add (s);
			openClose (true);
			switchMenu ((int)Menutype.COMPENDIUM);
			compendium.GoToLetter (s);

		}
	}
}
