using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemHandler : MonoBehaviour {

	public string letter;
	private PlayerController pc;
	private PlayerPhysics pp;
	public List<Powerup> Powerups {get; set;}

	void Start(){
		pc = GetComponent<PlayerController> ();
		pp = GetComponent<PlayerPhysics> ();
	}

	void Update(){

	}

	public void Added(){

	}

	public void Removed(){

	}

	public void ButtonUp(){

	}

	public void ButtonDown(){

	}
}
