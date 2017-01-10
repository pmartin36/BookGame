using UnityEngine;
using System.Collections;

public class Submenu : MonoBehaviour {

	protected bool disableHorizontalMove = false;

	// Use this for initialization
	public virtual void Start () {
		transform.localPosition = Vector3.zero;
		//gameObject.SetActive (false);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public virtual void openSubmenu(){
		gameObject.SetActive (true);
	}

	public virtual void moveCursor(float vertical){

	}

	public virtual void useHorizontal(float horizontal){

	}
}
