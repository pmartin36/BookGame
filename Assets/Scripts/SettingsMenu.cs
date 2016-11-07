using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using System;

public class SettingsMenu : Submenu {

	int index = 0;

	private bool disableMoveCursor = false;
	private List<Text> options = new List<Text>();
	private List<Slider> sliders = new List<Slider>();

	private List<string> screenOptions;
	private List<Resolution> resolutions;

	public Material activeMaterial;
	public Material inactiveMaterial;

	// Use this for initialization
	void Start () {
		activeMaterial = Resources.Load<Material> ("Materials/Graphic/Menu Select");
		inactiveMaterial = Resources.Load<Material> ("Materials/Graphic/Menu Select Inactive");

		List<Text> texts = new List<Text>(GetComponentsInChildren<Text> ());
		options = new List<Text> ();
		Text music = texts.FirstOrDefault (t => t.name == "Music");
		options.Add (music);
		sliders.Add (music.GetComponentInChildren<Slider> ());

		Text soundfx = texts.FirstOrDefault (t => t.name == "SoundFX");
		options.Add (soundfx);
		sliders.Add (soundfx.GetComponentInChildren<Slider> ());

		options.Add (texts.FirstOrDefault (t => t.name == "Fullscreen"));
		options.Add (texts.FirstOrDefault (t => t.name == "Resolution"));

		screenOptions = new List<string> () { "Fullscreen", "Windowed" };
		resolutions = new List<Resolution>(Screen.resolutions);

		base.Start ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public override void moveCursor(float vertical){
		if (disableMoveCursor && Mathf.Abs (vertical) < 0.05f) {
			disableMoveCursor = false;
		}
		else if (!disableMoveCursor  && Mathf.Abs(vertical) > 0.9f) {
			int newIndex = index + (int)Mathf.Sign (-vertical);
			if (newIndex < 0)
				newIndex = 0;
			else if (newIndex >= options.Count)
				newIndex = options.Count - 1;

			StartCoroutine (PauseMoveCursor());

			setCursorPosition (newIndex);
		}
	}

	public void IncreaseScrollerClicked(int newIndex){
		setCursorPosition (newIndex);
		useHorizontal (1f);
	}

	public void DecreaseScrollerClicked(int newIndex){
		setCursorPosition (newIndex);
		useHorizontal (-1f);
	}

	public override void useHorizontal(float horizontal){
		if(disableHorizontalMove && Mathf.Abs (horizontal) < 0.05f) {
			disableHorizontalMove = false;
		}

		if (index == 0 || index == 1) {
			sliders [index].value += horizontal * 0.02f;
		}
		else if (index == 2) {
			if (Mathf.Abs (horizontal) > 0.5f && !disableHorizontalMove) {
				int currentIndex = screenOptions.FindIndex (s => s == options [index].text);
				int newIndex = (currentIndex + screenOptions.Count + Math.Sign (horizontal)) % screenOptions.Count;
				options [index].text = screenOptions [newIndex];
				StartCoroutine(PauseHorizontalMove());
			}
		}
		else if(index == 3) {
			if (Mathf.Abs (horizontal) > 0.5f && !disableHorizontalMove) {
				int currentIndex = resolutions.FindIndex (s => ReformatResolutions(s.ToString()) == options [index].text);
				int newIndex = (currentIndex + resolutions.Count + Math.Sign (horizontal)) % resolutions.Count;
				options[index].text = ReformatResolutions(resolutions[newIndex].ToString());
				StartCoroutine(PauseHorizontalMove());
			}
		}
	}

	private string ReformatResolutions(string r){
		return r.Split ('@') [0].Trim();
	}

	public void setCursorPosition(int _index){
		if (_index != index) {
			options [index].color = new Color(0.5f, 0.5f, 0.5f, 1f);
			index = _index;
			options [index].color = new Color (0.1f, 0.72f, 1f, 1f);
		}
	}

	public override void openSubmenu(){
		gameObject.SetActive (true);
		if (Screen.fullScreen) {
			options [2].text = "Fullscreen";
		}
		else {
			options [2].text = "Windowed";
		}
		sliders [0].value = SettingsManager.Instance.MusicVolume;
		sliders [1].value = SettingsManager.Instance.SfxVolume;
	}

	public void SaveSettings(){
		int width, height;
		if (!options [3].text.ToLower ().Contains ("resolution")) {
			string[] reso = options [3].text.Split ('x');
			width = Int32.Parse (reso [0].Trim ());
			height = Int32.Parse (reso [1].Trim ());
		}
		else {
			width = Screen.width;
			height = Screen.height;
		}

		string screenType = options [2].text;

		if (screenType.ToLower ().Contains ("fullscreen")) {
			Screen.SetResolution (width, height, true);
		}
		else {
			Screen.SetResolution (width, height, false);
		}

		SettingsManager.Instance.MusicVolume = sliders [0].value;
		SettingsManager.Instance.SfxVolume = sliders[1].value;

	}

	IEnumerator PauseMoveCursor(){
		disableMoveCursor = true;
		float t = Time.realtimeSinceStartup;
		while (disableMoveCursor && Time.realtimeSinceStartup - t < 0.3f) {
			yield return null;
		}
		disableMoveCursor = false;
	}

	IEnumerator PauseHorizontalMove(){
		disableHorizontalMove = true;
		float t = Time.realtimeSinceStartup;
		while (disableHorizontalMove && Time.realtimeSinceStartup - t < 0.5f) {
			yield return null;
		}
		disableHorizontalMove = false;
	}
}
