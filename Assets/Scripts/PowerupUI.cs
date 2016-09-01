using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PowerupUI : MonoBehaviour {

	public Image image { get; set; }
	public int count { get; set; }
	public string mappedButton {get;set;}
	public string letter { get; set;}
	public bool isPOTD { get; private set; }

	// Use this for initialization
	void Start () {
		
	}

	public void initialize(string _letter){
		image = GetComponent<Image> ();
		count = 1;
		letter = _letter;
		mappedButton = "";

		isPOTD = GameManager.IsPOTD(_letter);
	}

	public void setSprite(Sprite sprite){
		image.overrideSprite = sprite;
	}
		
	public Color getColor(){
		if (mappedButton == "X") {
			return new Color (0.2f, 0.4f, 0.8f);
		} 
		else if (mappedButton == "Y") {
			return new Color (0.5f, 0.5f, 0.0f);
		} 
		else {
			return Color.gray;
		}
	}

	// Update is called once per frame
	void Update () {
	
	}
}
