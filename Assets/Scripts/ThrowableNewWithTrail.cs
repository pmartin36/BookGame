using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class ThrowableNewWithTrail : ThrowableNew {

	GameObject ropeSegment_prefab;
	BoxCollider2D box;
	List<RopeSegment> ropes;
	Transform ropeContainer;
	Vector2 launchPosition;

	public LayerMask Raymask;

	// Use this for initialization
	void Start () {
		
	}

	public override void Update(){
		//if throwable is outside the level bounds (aside from up since it will always fall back down)
		if ( transform.position.x > boundingBox.max.x || transform.position.x < boundingBox.min.x || transform.position.y < boundingBox.min.y ) {
			DestroyAll ();
			return;
		}

		if (IsLaunched) {
			if (Time.time - LaunchTime > 5) {
				DestroyAll ();
			}
			else {
				float angle = Vector3.Angle (Vector3.up, rigid.velocity);
				angle *= Mathf.Sign (Vector3.Cross (Vector3.up, rigid.velocity).z);
				float modifier = Mathf.Abs(rigid.velocity.x) < 0.1f ? 1 : Mathf.Sign (rigid.velocity.x);
				this.transform.localRotation = Quaternion.Euler (0, 0, angle + 35 * modifier);

				ModifyTrailBetweenPlayerAndThrowable();
			}
		}
	}

	public void FixedUpdate(){
		RaycastHit2D [] hits = Physics2D.RaycastAll (box.bounds.center, rigid.velocity.normalized, Mathf.Sqrt (Vector2.SqrMagnitude (rigid.velocity))*Time.deltaTime, Raymask); 
		if (hits.Length > 0) {
			RaycastHit2D h = hits.First (x => x.distance == hits.Min (d => d.distance));
			Debug.DrawLine (box.bounds.center, h.point, Color.cyan);
			rigid.velocity = new Vector2 (h.point.x - box.bounds.center.x, h.point.y - box.bounds.center.y) / Time.deltaTime;
		}
	}

	public void Init(bool isGrapple){
		base.Init ();

		if (isGrapple) {
			ropeSegment_prefab = Resources.Load<GameObject> ("Prefabs/RopeSegmentRed");
		}
		else {
			ropeSegment_prefab = Resources.Load<GameObject> ("Prefabs/RopeSegmentYellow");
		}
			
		Raymask = 1 << LayerMask.NameToLayer ("Letter") | 1 << LayerMask.NameToLayer ("Walkable") | 1 << LayerMask.NameToLayer("Book");

		ropeContainer = new GameObject ("RopeContainer").transform;
		ropes = new List<RopeSegment> ();

		box = GetComponent<BoxCollider2D> ();

		startingVelocity = 25;
		displayVelocity = 7;
		gravityModifier = 0;
		rigid.gravityScale = 0;
	}

	public void ModifyTrailBetweenPlayerAndThrowable(){
		foreach (Transform child in ropeContainer) {
			Destroy (child.gameObject);
		}

		float direction = GameManager.vectorToAngle (this.transform.position - Thrower.transform.position)-90;
		Vector2 rope_pos = Thrower.transform.position;

		float distance = Vector2.Distance (this.transform.position, Thrower.transform.position);

		ropes.Clear ();
		for(float i = 0; i <= distance; i+=0.1f){
			rope_pos = Vector2.Lerp (Thrower.transform.position, transform.position, i / distance);

			GameObject ropeobject = Instantiate (ropeSegment_prefab, rope_pos, Quaternion.Euler (0, 0, direction)) as GameObject;
			ropeobject.transform.parent = ropeContainer; 
			RopeSegment ropesegment = ropeobject.GetComponent<RopeSegment> ();
			ropesegment.SqrDistanceFromStart = Mathf.Pow(i,2);

			ropes.Add (ropesegment);
		}
	}

	public List<RopeSegment> CreateNewTrailBetweenPlayerAndThrowable(){
		List<RopeSegment> returnropes = new List<RopeSegment> ();

		float direction = GameManager.vectorToAngle (this.transform.position - Thrower.transform.position)-90;
		Vector2 rope_pos = Thrower.transform.position;

		float distance = Vector2.Distance (this.transform.position, Thrower.transform.position);

		for(float i = 0; i <= distance; i+=0.1f){
			rope_pos = Vector2.Lerp (Thrower.transform.position, transform.position, i / distance);

			GameObject ropeobject = Instantiate (ropeSegment_prefab, rope_pos, Quaternion.Euler (0, 0, direction)) as GameObject;
			ropeobject.transform.parent = ropeContainer; 
			RopeSegment ropesegment = ropeobject.GetComponent<RopeSegment> ();
			ropesegment.SqrDistanceFromStart = Mathf.Pow(i,2);

			returnropes.Add (ropesegment);
		}

		return returnropes;
	}

	public override void UseEffect(Letter effector){
		ModifyTrailBetweenPlayerAndThrowable ();

		switch (Letter.ToUpper ()) {
			case "G":
				Thrower.SetGrappleDirection (transform.position, ropes);
				break;
			case "Y":
				Thrower.SetYank (effector, this.gameObject, ropes);
				break;
			default:
				Debug.Log (Letter + " is not a  throwable item");
				break;
		}
		DestroyAll ();
	}

	public void DestroyAll(){
		foreach (Transform child in ropeContainer) {
			Destroy (child.gameObject);
		}
		Destroy (this.gameObject);
	}

	public override void launch(){
		this.transform.localRotation = Quaternion.Euler (0, 0, 35);
		this.transform.localScale = new Vector2 (.25f, .25f);

		BoxCollider2D bc = GetComponent<BoxCollider2D> ();
		bc.offset = new Vector2 (-0.45f, -0.625f);
		bc.size = Vector2.one * 0.25f;

		base.launch ();
	}

	void OnDestroy(){
		if(ropeContainer != null)
			Destroy (ropeContainer.gameObject);
		StopAllCoroutines ();
	}

	/**** PHYSICS ****/
	public override void OnTriggerEnter2D(Collider2D col){
		Letter l;
		if (col.tag == "Walkable") {
			l = col.transform.parent.gameObject.GetComponent<Letter> ();
		}
		else if (col.tag == "Letter") {
			l = col.gameObject.GetComponent<Letter> ();
		}
		else {
			return;
		}

		if (IsLaunched) {
			UseEffect (l);	
		}
	}
}
