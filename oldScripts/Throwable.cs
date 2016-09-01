using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Throwable : MonoBehaviour {

	//is the throwable a trace or should it have an effect on impact
	public bool Trace{ get; private set; }
	public string Letter{ get; private set; }
	public float LaunchTime { get; private set; }
	public Vector2 TrajectoryAngle { get; private set; }
	public Vector2 BoyStartingPosition { get; private set; }

	public bool IsLaunched { get; private set; }

	private Rigidbody2D rigid;

	private List<Vector3> vertices = new List<Vector3> ();
	private Mesh trailMesh;
	private MeshFilter mf;
	private MeshRenderer mr;

	private GameObject trail;

	private Bounds boundingBox;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Time.time - LaunchTime > 2 && Trace) {
			DestroyImmediate (this.gameObject);
			return;
		}

		//if throwable is outside the level bounds (aside from up since it will always fall back down)
		if (transform.position.x > boundingBox.max.x || transform.position.x < boundingBox.min.x || transform.position.y < boundingBox.min.y) {
			DestroyImmediate (this.gameObject);
			return;
		}

		if (IsLaunched) {
			transform.Rotate (0, 0, 2);
		}

		UpdateTrail ();
	}

	public void Init(){
		if (rigid == null) {
			rigid = GetComponent<Rigidbody2D> ();
		}

		boundingBox = GameObject.FindGameObjectWithTag ("Background").GetComponent<SpriteRenderer> ().bounds;

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

	public void setTrace(bool isTrace){
		Trace = isTrace;
		if (Trace) {
			GetComponent<SpriteRenderer> ().color = new Color (0, 0, 1, 0.0f);
			setKinematic (false);

			/*
			//create trail for trace
			trail = new GameObject ("Trail");
			trail.transform.parent = this.transform;
			trail.transform.localPosition = Vector3.zero;

			GameObject trailChild = new GameObject ("TrailChild");
			trailChild.transform.parent = trail.transform;
			trailChild.transform.localPosition = Vector3.zero;

			TrailRenderer tr = trailChild.AddComponent<TrailRenderer> ();
			tr.startWidth = 0.05f;
			tr.endWidth = 0.05f;
			tr.material = new Material(Shader.Find("Sprites/Default"));
			*/
		}
	}

	//traces should call this
	public void launch(float angle, Vector2 bsp){
		BoyStartingPosition = bsp;
		launch (angle);
	}

	//actual projectile should call this
	public void launch(float angle){
		setKinematic (false);
		TrajectoryAngle = Throwable.ComputeTrajectoryAngle (angle);
		GetComponent<Rigidbody2D> ().AddForce (350*TrajectoryAngle);

		this.transform.parent = null;
		LaunchTime = Time.time;
		IsLaunched = true;

		//store initial position/velocity
		UpdateTrail();
	}

	public void AdjustTrailIfTrace(Vector2 currentBoyPosition){
		if (Trace) {
			Vector3 move_amount = currentBoyPosition - BoyStartingPosition;
			mf.transform.position = move_amount;
		}
	}

	private void setSprite(){
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
		case "M":
			sr.sprite = Resources.Load<Sprite> ("Sprites/make_usable");
			break;
		default:
			Debug.Log (Letter + " is not a  throwable item");
			break;
		}
	}

	private void UseEffect(Letter effector){
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
		
			break;
		default:
			Debug.Log (Letter + " is not a  throwable item");
			break;
		}
	}

	/**** TRAIL ****/
	void UpdateTrail(){
		if (LaunchTime > 0 && (vertices.Count < 2 || vertices[vertices.Count-2] != transform.position) && Trace) {
			vertices.Add (transform.position);
			vertices.Add (transform.position + (Quaternion.Euler (0, 0, -90f) * rigid.velocity.normalized * 0.15f)); //.2f is the width of the trail, trail is 90 degrees off the velocity

			if (vertices.Count == 2) {
				if (trailMesh == null) {
					trailMesh = new Mesh ();

					GameObject newMesh = Instantiate(Resources.Load<GameObject> ("Prefabs/ThrowPath")) as GameObject;
					//newMesh.transform.parent = this.transform;
					//newMesh.transform.localPosition = new Vector3 (0,0,transform.position.z);

					mr = newMesh.GetComponent<MeshRenderer> ();
					mr.sortingLayerName = "FrontItem";
					mr.material.color = Color.blue;

					mf = newMesh.GetComponent<MeshFilter> ();
					mf.sharedMesh = trailMesh;
				}

				//need to have uvs for each vertex
				List<Vector2> uvs = new List<Vector2> (trailMesh.uv);
				uvs.Add(new Vector2(0, 1));
				uvs.Add(new Vector2(0, 0));

				trailMesh.vertices = vertices.ToArray ();
				trailMesh.uv = uvs.ToArray();
				trailMesh.RecalculateNormals ();

				mf.sharedMesh = trailMesh;

			}
			//create a trail if there are more than 2 vertices
			else if (vertices.Count > 2) {
				List<int> triangles = new List<int>(trailMesh.triangles);
				List<Vector2> uvs = new List<Vector2> (trailMesh.uv);

				//need to add 2 triangles (6 points) and two uvs
				int k = vertices.Count - 1;
				triangles.Add (k - 2); triangles.Add (k - 3); triangles.Add (k-1);
				triangles.Add (k - 2); triangles.Add (k - 1); triangles.Add (k);

				int x = (k - 1) % 4 == 0 ? 0 : 1;
				uvs.Add(new Vector2(x, 1));
				uvs.Add(new Vector2(x, 0));	

				trailMesh.vertices = vertices.ToArray ();
				trailMesh.triangles = triangles.ToArray ();
				trailMesh.uv = uvs.ToArray ();
				trailMesh.RecalculateNormals ();

				mf.sharedMesh = trailMesh;

				Color c = mr.material.color;
				c.a = 1f / (1 + Time.time - LaunchTime);
				mr.material.color = c;
			}
		}
	}
		


	/**** PHYSICS ****/

	void OnTriggerEnter2D(Collider2D col){
		if (!Trace) {
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
		/*
		foreach (Transform child in transform) {
			Destroy (child.gameObject);
		}
		*/
		if (IsLaunched) {
			Destroy (this.gameObject);
		}
	}

	void OnDestroy(){
		trailMesh = null;
		if (mf != null) {
			Destroy (mf.gameObject);
		}
	}
}
