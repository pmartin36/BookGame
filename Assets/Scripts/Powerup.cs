using UnityEngine;
using System.Collections;

public class Powerup {

	public Letter letter { get; set; }
	public bool available { get; set;}
	public bool inUse { get; set; }
	public string buttonMapping { get; set; }
	public int count { get; set; }

	public Powerup(Letter _letter){
		letter = _letter;
		available = true;
		inUse = false;
		buttonMapping = "";
		count = 1;
	}

}
