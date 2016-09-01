using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public class Menu : MonoBehaviour {

	private enum Menutype{ MAIN, SETTINGS, COMPENDIUM, CONTROLS };

	private bool disableMoveCursor = false;
	private int index = 0;
	private List<Button> options = new List<Button>();

	public Material activeMaterial;
	public Material inactiveMaterial;

	float effectPosition = 0;
	Menutype currentMenu = Menutype.MAIN;

	PlayerInput inputController;

	// Use this for initialization
	void Start () {
		GameObject[] optionlist = GameObject.FindGameObjectsWithTag ("Option");
		foreach (GameObject option in optionlist) {
			options.Add (option.GetComponent<Button> ());
		}
		options = options.OrderBy (y => y.transform.position.y).Reverse().ToList();

		gameObject.SetActive (false);
	}
	
	// Update is called once per frame
	void Update () {
		effectPosition += .01f;
		options [index].image.material.SetFloat ("_EffectOffset", effectPosition);
	}

	public void moveCursor(float vertical){
		if (disableMoveCursor && Mathf.Abs (vertical) < 0.2f) {
			disableMoveCursor = false;
		}
		else if (!disableMoveCursor  && Mathf.Abs(vertical) > 0.9f) {
			options [index].image.material = inactiveMaterial;

			index += (int)Mathf.Sign (-vertical);
			if (index < 0)
				index = 0;
			else if (index >= options.Count)
				index = options.Count - 1;

			disableMoveCursor = true;

			options [index].image.material = activeMaterial;
		}
	}

	//returns whether the menu is still open
	public bool select(){
		bool menuStatus = true;
		if (index == 0 && Menutype.MAIN == currentMenu) {
			menuStatus = false;
		}
		options [index].onClick.Invoke ();
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
			
			break;
		case Menutype.SETTINGS:
			
			break;
		case Menutype.COMPENDIUM:
			
			break;
		case Menutype.CONTROLS:

			break;
		}
	}

	public void openClose(bool shouldOpen){
		gameObject.SetActive (shouldOpen);
		if (shouldOpen) {
			options [index].image.material = activeMaterial;
		}
		else {
			options [index].image.material = inactiveMaterial;
			index = 0;


		}
	}
}
