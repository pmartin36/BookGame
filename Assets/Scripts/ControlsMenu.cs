using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Linq;

public class ControlsMenu : Submenu {

	public bool controllerSelected = true;

	Image displayedControl;
	Sprite controller, kbm;

	// Use this for initialization
	void Start () {
		base.Start ();
		controller = Resources.Load<Sprite> ("Sprites/controls_controller");
		kbm = Resources.Load<Sprite> ("Sprites/controls_keyboard");

		displayedControl = GetComponentsInChildren<Image> ().FirstOrDefault (s => s.name == "ControlImage");
	}
	
	public override void useHorizontal(float horizontal){
		if (controllerSelected && horizontal > 0.5f) {
			//switch to keyboard/mouse view
			displayedControl.overrideSprite = kbm;

			//switch text color
			Text [] t = GetComponentsInChildren<Text>();
			t.FirstOrDefault(c => c.transform.parent.name == "Controller").color = new Color(0.5f, 0.5f, 0.5f, 1f);
			t.FirstOrDefault(c => c.transform.parent.name == "Keyboard/Mouse").color = new Color (0.1f, 0.72f, 1f, 1f);
			controllerSelected = false;
		}
		else if (!controllerSelected && horizontal < -0.5f) {
			//switch to controller view
			displayedControl.overrideSprite = controller;

			//switch text color
			Text [] t = GetComponentsInChildren<Text>();
			t.FirstOrDefault(c => c.transform.parent.name == "Controller").color = new Color (0.1f, 0.72f, 1f, 1f);
			t.FirstOrDefault(c => c.transform.parent.name == "Keyboard/Mouse").color = new Color(0.5f, 0.5f, 0.5f, 1f);
			controllerSelected = true;
		}
	}
}
