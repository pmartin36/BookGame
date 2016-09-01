using UnityEngine;
using System.Collections;

public class ConstantSpring : MonoBehaviour {

	public Sprite[] sprites;
	bool springInUse = false;
	private SpriteRenderer sr;

	// Use this for initialization
	void Start () {
		sr = GetComponent<SpriteRenderer> ();
	}

	// Update is called once per frame
	void Update () {
	
	}

	void OnCollisionEnter2D(Collision2D other){
		if (other.gameObject.tag == "Player") {
			if (!springInUse) {
				SpriteRenderer otherSprite = other.gameObject.GetComponent<SpriteRenderer> ();
				if (otherSprite.bounds.min.x < sr.bounds.max.x && otherSprite.bounds.max.x > sr.bounds.min.x && other.transform.position.y > gameObject.transform.position.y) {
					StartCoroutine (UseSpring (other));
				}
				else {
					StartCoroutine (DisableSpringForTime());
				}
			}
		}
	}

	void OnCollisionExit2D(Collision2D other){
		if (other.gameObject.tag == "Player") {
			if (springInUse) {
				PlayerController pc = other.gameObject.GetComponent<PlayerController> ();
				pc.horizontal_move (true);
			}
		}
	}

	IEnumerator UseSpring(Collision2D other){
		springInUse = true;
		PlayerPhysics pp = other.gameObject.GetComponent<PlayerPhysics> ();
		other.rigidbody.velocity = new Vector2 (other.rigidbody.velocity.x, 0);
		pp.ApplyForce (Vector3.up * 600);

		sr.sprite = sprites [1];
		yield return new WaitForSeconds (0.1f);
		sr.sprite = sprites [2];
		yield return new WaitForSeconds (0.1f);
		sr.sprite = sprites [0];

		springInUse = false;
		yield return null;
	}

	IEnumerator DisableSpringForTime(){
		BoxCollider2D box = GetComponent<BoxCollider2D> ();
		box.enabled = false;
		yield return new WaitForSeconds (1f);
		box.enabled = true;
		yield return null;
	}
}
