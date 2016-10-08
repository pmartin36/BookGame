using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ThrowableNew : MonoBehaviour {

	public string Letter{ get; private set; }
	public float LaunchTime { get; private set; }
	public float TrajectoryAngle { get; private set; }
	public bool ValidTrajectory {get; private set;}
	public PlayerPhysics Thrower {get;set;}

	public bool IsLaunched { get; private set; }

	protected Rigidbody2D rigid;
	protected SpriteRenderer sr;

	protected Bounds boundingBox;

	protected float startingVelocity;

	protected GameObject[] trail;

	// Use this for initialization
	void Start () {
	
	}

	public void Init(){
		if (rigid == null) {
			rigid = GetComponent<Rigidbody2D> ();
		}
		if (sr == null) {
			sr = GetComponent<SpriteRenderer> ();
		}
		startingVelocity = 7;

		trail = new GameObject[10];
		GameObject prefab = Resources.Load<GameObject> ("Prefabs/Circle");
		for (int i = 0; i < 10; i++) {
			trail [i] = Instantiate (prefab);
			trail [i].transform.parent = this.transform;
			trail [i].SetActive (false); 
		}

		boundingBox = GameObject.FindGameObjectWithTag ("Background").GetComponent<SpriteRenderer> ().bounds;
	}

	// Update is called once per frame
	public virtual void Update () {
		//if throwable is outside the level bounds (aside from up since it will always fall back down)
		if (transform.position.x > boundingBox.max.x || transform.position.x < boundingBox.min.x || transform.position.y < boundingBox.min.y) {
			DestroyImmediate (this.gameObject);
			return;
		}

		if (IsLaunched) {
			transform.Rotate (0, 0, 2);
		}
	}

	public static Vector2 ComputeTrajectoryAngle(float angle){
		Vector2 vangle = GameManager.angleToVector (angle);
		vangle.y = vangle.y >= .1f ? vangle.y : .1f; //can't throw lower than that
		return vangle;
	}

	public void setKinematic(bool isKine){
		GetComponent<Rigidbody2D> ().isKinematic = isKine;
	}
	public bool getKinematic(){
		return GetComponent<Rigidbody2D> ().isKinematic;
	}

	public void setLetter(string newLetter){
		Letter = newLetter;
		//set appropriate sprite
		setSprite();
	}

	public void SetTrajectoryAngle(float angle){
		if (!IsLaunched) {
			angle = ((int)Mathf.Round (angle / 10f)) * 10; //round to the nearest 10
			angle %= 360;
			if (angle != TrajectoryAngle) {
				//redo position prediction
				Vector2 a = GameManager.angleToVector (angle);
				LayerMask mask = 1 << LayerMask.NameToLayer ("Letter") | 1 << LayerMask.NameToLayer ("Walkable");
				List<PositionPrediction.RigidbodyStatusData> positions = PositionPrediction.predict (transform.position, a * startingVelocity, 0, 50, 0, true, 0.5f, 0, sr.bounds.extents.x, mask);
				TrajectoryAngle = angle;
				for (int i = 5; i < positions.Count; i += 5) {
					//Debug.DrawLine (positions [i].position, positions [i - 5].position, Color.blue, 1f);
					int index = i / 5;
					trail [index].SetActive (true);
					trail [index].transform.position = positions [i].position;
				}
				for (int i = positions.Count; i < 50; i += 5) {
					int index = i / 5;
					trail [index].SetActive (false);
				}
			}
			ValidTrajectory = true;
		}
	}

	void DeleteTrailObjects(){
		foreach (GameObject o in trail) {
			DestroyImmediate (o);
		}
	}

	void ClearTrail(){
		foreach (GameObject o in trail) {
			o.SetActive (false);
		}
	}

	public void ClearTrajectory(){
		TrajectoryAngle = -1000;
		ValidTrajectory = false;
		ClearTrail ();
	}

	//actual projectile should call this
	public virtual void launch(){
		DeleteTrailObjects ();

		setKinematic (false);
		rigid.velocity = GameManager.angleToVector (TrajectoryAngle) * startingVelocity;

		sr.sortingLayerName = "BackItem";

		this.transform.parent = null;
		LaunchTime = Time.time;
		IsLaunched = true;
	}

	protected void setSprite(){
		SpriteRenderer sr = GetComponent<SpriteRenderer> ();
		switch (Letter.ToUpper()) {
		case "E":
			sr.sprite = Resources.Load<Sprite> ("Sprites/eraser");
			break;
		case "H":
			sr.sprite = Resources.Load<Sprite> ("Sprites/higher");
			break;
		case "L":
			sr.sprite = Resources.Load<Sprite> ("Sprites/lower");
			break;
		case "G":
			sr.sprite = Resources.Load<Sprite> ("Sprites/grapple");
			break;
		case "Y":
			sr.sprite = Resources.Load<Sprite> ("Sprites/yankgrapple");
			break;
		case "M":
			sr.sprite = Resources.Load<Sprite> ("Sprites/make_usable");
			break;
		default:
			Debug.Log (Letter + " is not a  throwable item");
			break;
		}
	}

	public virtual void UseEffect(Letter effector){
		switch (Letter.ToUpper()) {
		case "E":
			effector.startDisappear ();
			break;
		case "H":
			effector.SetMoveAmount(Quaternion.Euler (0, 0, -effector.transform.localRotation.eulerAngles.z) * new Vector3 (0, 0.02f, 0));
			break;
		case "L":
			effector.SetMoveAmount(Quaternion.Euler (0, 0, -effector.transform.localRotation.eulerAngles.z) * new Vector3 (0, -0.02f, 0));
			break;
		case "M":
			if (!effector.canBeHarvested) {
				effector.SetUnharvested (false);
			}
			break;
		case "G":
			Thrower.SetGrappleDirection (transform.position, null);
			break;
		case "Y":
			if (Thrower.collisionLetter != effector) {
				int direction = Math.Sign (Thrower.transform.position.x - this.transform.position.x);
				effector.SetMoveAmount (Quaternion.Euler (0, 0, -effector.transform.localRotation.eulerAngles.z) * new Vector3 (direction * 0.02f, 0, 0));
			}
			break;
		default:
			Debug.Log (Letter + " is not a  throwable item");
			break;
		}
	}

	/**** PHYSICS ****/
	public virtual void OnTriggerEnter2D(Collider2D col){
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
			Destroy (this.gameObject);
		}
	}

	void OnDestroy(){
		StopAllCoroutines ();
	}
}
