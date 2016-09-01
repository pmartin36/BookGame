using UnityEngine;
using System.Collections;

public class StringItem : MonoBehaviour {

	protected Rigidbody2D rigid;
	protected Rigidbody2D attached;
	protected SpriteRenderer spriteRenderer;
	protected Bounds boundingBox;
	public float StringGrabOffset { get; set; }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void setAttachedRigidbody(Rigidbody2D _attached){
		attached = _attached;
	}

	public void setStringGrabOffset(Vector2 position){
		//reposition if the position is way off
		if (position.y < spriteRenderer.bounds.min.y) {
			float y = attached.GetComponent<SpriteRenderer> ().bounds.extents.y;
			transform.position = new Vector3 (transform.position.x + 0.1f * attached.transform.localScale.x, position.y + y, transform.position.z);
		}
		StringGrabOffset = this.transform.position.y - position.y;
	}

	public void resetAttachedPosition(){
		if (attached != null) {
			attached.transform.position = new Vector3 (transform.position.x, transform.position.y - StringGrabOffset, transform.position.z);
		}
	}

	protected void ConfineToBoundingBox(){
		if (transform.position.x + spriteRenderer.bounds.extents.x > boundingBox.max.x) {
			transform.position = new Vector3 (boundingBox.max.x - spriteRenderer.bounds.extents.x, transform.position.y, transform.position.z);
			rigid.velocity = new Vector2 (0, rigid.velocity.y);
		}
		if (transform.position.x - spriteRenderer.bounds.extents.x < boundingBox.min.x) {
			transform.position = new Vector3 (boundingBox.min.x + spriteRenderer.bounds.extents.x, transform.position.y, transform.position.z);
			rigid.velocity = new Vector2 (0, rigid.velocity.y);
		}
	}

	public IEnumerator EnableCollisionAfterTime(float time, GameObject other){
		yield return new WaitForSeconds (time);
		foreach(Collider2D c in other.GetComponents<Collider2D> ()) {
			foreach (Collider2D p in this.GetComponents<Collider2D>()) {
				Physics2D.IgnoreCollision (c, p, false);
			}
		}
		yield return null;
	}

	void OnDestroy(){
		StopAllCoroutines ();
	}
}
