using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SettingsManager : Singleton<SettingsManager> {

	public float MusicVolume { get; set; }
	public float SfxVolume { get; set;}
	public int CurrentLevel { get; set; }

	public List<string> collectedLetters;

	void Awake(){
		//get settings from file

		//else set to defaults 
		MusicVolume = 0.5f;
		SfxVolume = 0.5f;
		CurrentLevel = 0;

		collectedLetters = new List<string> ();
	}
}
