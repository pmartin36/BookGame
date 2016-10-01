using UnityEngine;
using System.Collections;

public class RotatingLetter : MonoBehaviour {

	Letter parentLetter;
	PolygonCollider2D poly;

	public void SetParentLetter(Letter l){
		parentLetter = l;
		transform.parent = l.transform;
		transform.localPosition = Vector2.zero;
		transform.localRotation = Quaternion.Euler(new Vector3 (0, 0, 0.5f));

		if (poly == null) {
			poly = gameObject.AddComponent<PolygonCollider2D> ();
			poly.isTrigger = true;
			poly.points = l.GetComponent<PolygonCollider2D> ().points;
		}
	}

	public void OnTriggerEnter2D(Collider2D col){
		bool validObject = col.tag == "Letter" || col.tag == "Book" || col.tag == "Walkable";
		if (col.transform != parentLetter.transform && validObject) {
			parentLetter.SetRotating (false);
		}
	}
}
