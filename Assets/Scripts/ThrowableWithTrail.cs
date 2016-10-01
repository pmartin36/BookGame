using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ThrowableWithTrail : ThrowableNew {

	ParticleSystem throwablerope;
	ParticleSystem playerrope;

	GameObject trail_prefab;
	GameObject ropeSegment_prefab;

	Vector2 launchPosition;

	float emissionDistance;
	GameObject linkingRope;

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
		}
	}

	public void FixedUpdate(){
		if (IsLaunched) {
			float angle = Vector3.Angle (Vector3.up, rigid.velocity);
			angle *= Mathf.Sign (Vector3.Cross (Vector3.up, rigid.velocity).z);
			float modifier = Mathf.Abs(rigid.velocity.x) < 0.1f ? 1 : Mathf.Sign (rigid.velocity.x);

			float vectorAngle = GameManager.vectorToAngle (new Vector2 (rigid.velocity.x, -rigid.velocity.y)) - 90;
			throwablerope.startRotation =  vectorAngle * Mathf.Deg2Rad;

			this.transform.localRotation = Quaternion.Euler (0, 0, angle + 35 * modifier);

			Vector2 pos_change = playerrope.transform.localPosition;
			playerrope.startRotation = (GameManager.vectorToAngle (new Vector2 (Thrower.getVelocity ().x, -Thrower.getVelocity ().y)) - 90) * Mathf.Deg2Rad;

			if (linkingRope == null && Vector3.Distance(Thrower.transform.position, launchPosition) > emissionDistance) {
				linkingRope = Instantiate (ropeSegment_prefab);
				linkingRope.name = "Linking Rope";
				linkingRope.transform.position = launchPosition;


				linkingRope.transform.localRotation = Quaternion.Euler (0, 0, Vector3.Angle (rigid.velocity, Thrower.getVelocity())/2f);
			}
		}
	}

	public void Init(bool isGrapple){
		trail_prefab = Resources.Load<GameObject> ("Prefabs/ThrowableTrail");
		if (isGrapple) {
			ropeSegment_prefab = Resources.Load<GameObject> ("Prefabs/RopeSegmentRed");
		}
		else {
			ropeSegment_prefab = Resources.Load<GameObject> ("Prefabs/RopeSegmentYellow");
		}
			
		base.Init ();
	}

	GameObject AddTrailToObject(Transform p, Vector3 initial_position){
		GameObject trail = Instantiate (trail_prefab);
		trail.transform.parent = p;
		trail.transform.localScale = Vector3.one;
		trail.transform.localPosition = initial_position;
		launchPosition = trail.transform.position;
		trail.SetActive (true);
		return trail;
	}

	public override void UseEffect(Letter effector){
		float direction = GameManager.vectorToAngle (this.transform.position - Thrower.transform.position)-90;
		Vector2 rope_pos = Thrower.transform.position;

		float distance = Vector2.Distance (this.transform.position, Thrower.transform.position);
	
		List<RopeSegment> ropes = new List<RopeSegment> ();
		for(float i = 0; i <= distance; i+=0.1f){
			rope_pos = Vector2.Lerp (Thrower.transform.position, transform.position, i / distance);

			GameObject ropeobject = Instantiate (ropeSegment_prefab, rope_pos, Quaternion.Euler (0, 0, direction)) as GameObject;
			RopeSegment ropesegment = ropeobject.GetComponent<RopeSegment> ();
			ropesegment.SqrDistanceFromStart = Mathf.Pow(i,2);

			ropes.Add (ropesegment);
		}
		if (linkingRope != null)
			Destroy (linkingRope);

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
		Destroy (playerrope.gameObject);
		Destroy (this.gameObject);
	}

	public override void launch(){
		this.transform.localRotation = Quaternion.Euler (0, 0, 35);
		this.transform.localScale = new Vector2 (.25f, .25f);

		BoxCollider2D bc = GetComponent<BoxCollider2D> ();
		bc.offset = new Vector2 (-0.45f, -0.625f);
		bc.size = Vector2.one * 0.25f;

		GameObject player_trail = AddTrailToObject (Thrower.transform, this.transform.localPosition);
		playerrope = player_trail.GetComponent<ParticleSystem> ();
		Thrower.GetComponent<PlayerController> ().trailingRope = playerrope;
		emissionDistance = 1f / playerrope.emission.rate.constant;

		GameObject throwable_trail = AddTrailToObject (this.transform, Vector3.zero);
		throwablerope = throwable_trail.GetComponent<ParticleSystem> ();

		if (Letter == "G") {
			//change rope color
			Material m = Resources.Load<Material>("Materials/Graphic/GrappleRope");
			playerrope.GetComponent<ParticleSystemRenderer> ().sharedMaterial = m;
			throwablerope.GetComponent<ParticleSystemRenderer>().sharedMaterial = m;
		}

		base.launch ();
	}

	void OnDestroy(){
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
