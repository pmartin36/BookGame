using UnityEngine;
using System.Collections;

public class Balloon : StringItem {

	private float top;
	float height;
	public bool isAtTop = false;

	float velocityYsmooth;
	float velocityXsmooth;

	Vector2 goalVelocity;

	// Use this for initialization
	void Awake () {
		rigid = GetComponent<Rigidbody2D> ();
		spriteRenderer = GetComponent<SpriteRenderer> ();
		boundingBox = GameObject.FindGameObjectWithTag ("Background").GetComponent<SpriteRenderer>().bounds;
		top = boundingBox.max.y;
		height = GetComponent<SpriteRenderer> ().bounds.extents.y;

		goalVelocity = new Vector2 (0, 2);
	}

	// Update is called once per frame
	void FixedUpdate () {
		float slowfactor = 0.4f;	
		float velocityX = Mathf.SmoothDamp (rigid.velocity.x, goalVelocity.x,ref velocityXsmooth, slowfactor);

		if (transform.position.y + height > top) {
			isAtTop = true;
			transform.position = new Vector3 (transform.position.x, top - height, transform.position.z);

			float velocityY = Mathf.SmoothDamp (rigid.velocity.y, 0,ref velocityYsmooth, slowfactor);
			rigid.velocity = new Vector2 (velocityX, 0);
		}
		else {
			isAtTop = false;
			float velocityY = Mathf.SmoothDamp (rigid.velocity.y, goalVelocity.y,ref velocityYsmooth, slowfactor);
			rigid.velocity = new Vector2 (velocityX, velocityY);
		}

		if (attached != null) {
			attached.velocity = rigid.velocity;
			attached.transform.position = new Vector3(transform.position.x, transform.position.y-StringGrabOffset, attached.transform.position.z);
		}

		base.ConfineToBoundingBox ();

	}
		
}
