using UnityEngine;
using System.Collections;

public class Spring : MonoBehaviour {


	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter2D(Collider2D other){
		if (other.tag == "Player") {
			Rigidbody2D rigid_player = other.gameObject.GetComponent<Rigidbody2D> ();
			rigid_player.velocity = new Vector2(rigid_player.velocity.x, Mathf.Abs(2*rigid_player.velocity.y));
		}
	}
}
