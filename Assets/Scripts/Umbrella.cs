using UnityEngine;
using System.Collections;

public class Umbrella : StringItem {

	// Use this for initialization
	void Awake () {
		rigid = GetComponent<Rigidbody2D> ();
		spriteRenderer = GetComponent<SpriteRenderer> ();
		boundingBox = GameObject.FindGameObjectWithTag ("Background").GetComponent<SpriteRenderer>().bounds;
	}

	// Update is called once per frame
	void FixedUpdate () {
		rigid.velocity = new Vector2 (rigid.velocity.x, -2);

		if (attached != null) {
			attached.velocity = rigid.velocity;
			attached.transform.position = new Vector3(transform.position.x, transform.position.y-StringGrabOffset, attached.transform.position.z);
		}

		base.ConfineToBoundingBox ();
	}

	public void OnCollisionEnter2D(Collision2D col){	
		if (col.collider.tag == "Letter" || col.collider.tag == "Walkable" || col.collider.tag == "TempWalkable") {
			BoxCollider2D collider = GetComponent<BoxCollider2D> ();
			ContactPoint2D contact = col.contacts [0];

			//hit the bottom of the umbrella
			if (contact.normal.y >= contact.normal.x) {
				//turn umbrella up instead of destroying it?

				if (attached != null) {
					PlayerPhysics pc = attached.transform.GetComponent<PlayerPhysics> ();
					if (pc != null) {
						pc.exitedStringItemState ();
					}
				}

				attached = null;
				Destroy (this.gameObject);
			}

		}
	}
}
