using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public class CompendiumMenu : Submenu {

	private Dictionary<string, CompendiumEntry> entries;

	private Text letterSelector;
	private Text powerupNameField;
	private Text descriptionField;
	private Image illustrationField;

	int index = 0;

	// Use this for initialization
	void Start () {
		
		Text[] t = gameObject.GetComponentsInChildren<Text> ();
		powerupNameField = t.First(v => v.name == "PowerupName");
		descriptionField = t.First(v => v.name == "Description");
		letterSelector = t.First(v => v.name == "Letter");

		Image[] im = gameObject.GetComponentsInChildren<Image> ();
		illustrationField = im.First (v => v.name == "Illustration");

		entries = new Dictionary<string, CompendiumEntry> ();

		base.Start ();
	}

	public void GoToLetter(string s){
		LoadCompendiumEntry (s);
	}

	public void IncreaseClicked(){
		ChangeSelector (1);
		StartCoroutine(WaitBeforeLoading ());
	}

	public void DecreaseClicked(){
		ChangeSelector (-1);
		StartCoroutine(WaitBeforeLoading ());
	}

	public void ChangeSelector(int direction){
		/*
		string currentText = letterSelector.text;
		if (direction < 0 && currentText == "A") {
			letterSelector.text = "Z";
		}
		else if (direction > 0 && currentText == "Z") {
			letterSelector.text = "A";
		}
		else {
			letterSelector.text = ((char)((int)char.Parse (currentText) + direction)).ToString();
		}
		*/

		int settingsCount = SettingsManager.Instance.collectedLetters.Count;
		if (settingsCount > 0) {
			index = (index + direction) % settingsCount;
			letterSelector.text = SettingsManager.Instance.collectedLetters [index];
		}
	}

	public override void useHorizontal(float horizontal){
		if (disableHorizontalMove && Mathf.Abs (horizontal) < 0.05f) {
			disableHorizontalMove = false;
		}

		if (!disableHorizontalMove && Mathf.Abs(horizontal) > 0.5f) {
			ChangeSelector ((int)Mathf.Sign (horizontal));
			StartCoroutine (PauseHorizontalMove ());

			StopCoroutine (WaitBeforeLoading ());
			StartCoroutine (WaitBeforeLoading ());
		}
	}

	private void LoadCompendiumEntry(string s){
		if (!entries.ContainsKey(s)) {
			//create new 
			entries.Add(s, new CompendiumEntry(s));
		}

		letterSelector.text = s;
		powerupNameField.text = entries [s].PowerupName;
		descriptionField.text = entries [s].Description;
		illustrationField.overrideSprite = entries [s].Illustration;
	}

	IEnumerator PauseHorizontalMove(){
		disableHorizontalMove = true;
		float t = Time.realtimeSinceStartup;
		while (disableHorizontalMove && Time.realtimeSinceStartup - t < 0.1f) {
			yield return null;
		}
		disableHorizontalMove = false;
	}

	IEnumerator WaitBeforeLoading(){
		string currentText = letterSelector.text;
		float t = Time.realtimeSinceStartup;
		while (Time.realtimeSinceStartup - t < 0.5f) {
			yield return null;
		}

		if (currentText == letterSelector.text) {
			LoadCompendiumEntry (currentText);
		}
	}
}
