using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class WalkableGeneratorNew : MonoBehaviour {

	private class Hit{
		public Hit(RaycastHit2D _hit, int _depth, int _i){
			hit = _hit;
			depth = _depth;
			i = _i;
		}

		public RaycastHit2D hit;
		public int depth;
		public int i;
	}

	private class WalkablePoint{
		public WalkablePoint(Vector2 _point, Vector2 _slope, int _i){
			point =  _point;
			slope = _slope;
			i = _i;
		}

		public Vector2 point;
		public Vector2 slope;
		public int i;
	}
		
	List<Hit> hits = new List<Hit>();
	List<WalkablePoint> wp = new List<WalkablePoint> ();
	List<Vector2> points = new List<Vector2> ();
	SpriteRenderer sr;
	float playerHeight;

	public static float normalRejectPoint = 0.8f;
	public float platformHeight = 0.2f;

	public GameObject meshPlatform;

	private List<GameObject> existingWalkables = new List<GameObject> ();

	void Start(){
		sr = gameObject.GetComponent<SpriteRenderer> ();
		SpriteRenderer psr = GameObject.FindGameObjectWithTag ("Player").GetComponent<SpriteRenderer> ();
		playerHeight = Mathf.Abs(psr.bounds.max.y - psr.bounds.min.y);
		meshPlatform = Resources.Load<GameObject> ("Prefabs/Walkable");

		GenerateWalkable (true);
	}

	public void GenerateWalkable(bool useEdgeCollider){
		hits.Clear ();
		wp.Clear ();
		if (useEdgeCollider) {
			foreach (EdgeCollider2D ec in GetComponents<EdgeCollider2D>()) {
				points = new List<Vector2> (ec.points);
				points [points.Count - 1] = points [0];

				GetPointsNew ();
				GenerateMeshFromHitsNew ();

				PolygonCollider2D pc = gameObject.AddComponent<PolygonCollider2D> ();
				pc.sharedMaterial = ec.sharedMaterial;
				pc.points = ec.points;

				ec.enabled = false;
			}
		}
		else {
			foreach (PolygonCollider2D poly in GetComponents<PolygonCollider2D>()) {
				points = new List<Vector2> (poly.points);
				points [points.Count - 1] = points [0];

				GetPointsNew ();
				GenerateMeshFromHitsNew ();
			}
		}
	}

	void GetPointsNew(){
		float max_y = points.Max (p => (transform.localRotation * p).y);
		int max_y_point_index = points.FindIndex (p => (transform.localRotation * p).y == max_y);  
		float topOrBottomAngle = 0;
		Vector2 previousdifference = Vector2.zero;
		for (int i = max_y_point_index; i < points.Count + max_y_point_index + 1; i++) {
			int thisIndex = i % points.Count;
			int nextIndex = (i + 1) % points.Count;
			Vector2 thisPoint = Vector2.Scale( transform.localRotation * points [thisIndex], transform.lossyScale );
			Vector2 nextPoint = Vector2.Scale( transform.localRotation * points [nextIndex], transform.lossyScale );
			Vector2 difference = nextPoint - thisPoint;

			if (nextIndex != 0) {
				//ccw is positive
				float distance = Vector2.Distance (thisPoint, nextPoint);
				float angle = Vector2.Angle (Vector2.up, difference);

				if (previousdifference == Vector2.zero) {
					if (difference.x > 0) {
						previousdifference = Vector2.right;
					}
					else {
						previousdifference = Vector2.left;
					}
				}

				//clockwise is negative z
				Vector3 cross = Vector3.Cross (previousdifference, difference).normalized;
				float difference_angle = Vector2.Angle (previousdifference, difference);
				topOrBottomAngle += difference_angle * Mathf.Sign (cross.z);

				float absAngle = Mathf.Abs (topOrBottomAngle);

				if (angle >= 60 && angle <= 120 && (absAngle <= 30 || 360 - absAngle <= 30)) {
					//do stuff
					Vector2 absolute_position = new Vector2 (transform.position.x + thisPoint.x, transform.position.y + thisPoint.y);
					if (!wp.Any (p => p.i == thisIndex)) {
						wp.Add (new WalkablePoint (absolute_position, difference, thisIndex));
					}

					Vector2 next_absolute_position = new Vector2 (transform.position.x + nextPoint.x, transform.position.y + nextPoint.y);
					if (!wp.Any (p => p.i == nextIndex)) {
						wp.Add (new WalkablePoint (next_absolute_position, difference, nextIndex));
					}
				}
				previousdifference = difference;
			}
		}
	}

	void AddVerticesNew(List<Vector3> vertices, int hitIndex){
		Vector3 hitpoint = wp [hitIndex].point;
		vertices.Add (hitpoint);
		vertices.Add (hitpoint - Vector3.up * platformHeight);
		//vertices.Add (new Vector2(hitpoint.x, hitpoint.y) - hits[hitIndex].hit.normal * platform_height);
	}

	void GenerateMeshFromHitsNew(){
		int meshCount = 0;

		wp = wp.OrderBy (w => w.i).ToList();
		for (int i = 0; i < wp.Count; i++) {
			List<Vector3> vertices = new List<Vector3> ();
			List<int> triangles = new List<int> ();

			WalkablePoint hit = wp [i];

			//always add the first point
			vertices.Add (hit.point);

			for (i = i + 1; i < wp.Count; i++) {
				//if the next hit should be connected to the current hit
				if (wp [i].i == hit.i + 1) {
					hit = wp [i];
					vertices.Add (hit.point);
				}
				else if (wp [i].i == points.Count - 1 && wp[0].i == 0) {
					//last point should check if the first point should be connected
					hit = wp [wp.Count-1];
					vertices.Add (hit.point);
				}
				else {
					i -= 1;
					break;
				}
			}

			if (vertices.Count >= 2) {
				vertices = vertices.OrderBy (v => v.x).ToList();

				for(int j = 0; j < vertices.Count; j+=2){
					//add a corresponding vertex below this one
					vertices.Insert(j+1, vertices[j] - Vector3.up * platformHeight);
				}

				for (int k = 0; k < vertices.Count - 3; k += 2) {
					triangles.Add (k + 1);
					triangles.Add (k);
					triangles.Add (k + 2);

					triangles.Add (k + 1);
					triangles.Add (k + 2);
					triangles.Add (k + 3);
				}

				if (triangles.Count > 1) {
					Mesh m = new Mesh ();
					m.vertices = vertices.ToArray ();
					m.triangles = triangles.ToArray ();
					m.uv = getUVs (vertices);
					m.RecalculateNormals ();

					GameObject newMesh;
					if (meshCount >= existingWalkables.Count) {
						newMesh = (GameObject)Instantiate (meshPlatform);
						newMesh.transform.parent = this.transform;
						newMesh.GetComponent<EdgeCollider2D> ().points = getColliderArray (vertices);
						newMesh.transform.position = new Vector3 (0, 0, -1f);
						existingWalkables.Add (newMesh);

						MeshRenderer mr = newMesh.GetComponent<MeshRenderer> ();
						mr.sortingLayerName = "Walkable";

						Material mat = new Material (mr.material);

						mat.color = GetComponent<Letter> ().canBeHarvested ? new Color (0.4f, 0.4f, 0.14f) : new Color (0.06f, 0.20f, 0.29f); //will probably need to be changed
						mr.material = mat;
					}
					else {
						newMesh = existingWalkables [meshCount];
						newMesh.transform.localRotation = Quaternion.Euler(-transform.localRotation.eulerAngles);
						newMesh.GetComponent<EdgeCollider2D> ().points = getColliderArray (vertices);
						newMesh.transform.position = new Vector3 (0, 0, -1f);
					}

					MeshFilter mf = newMesh.GetComponent<MeshFilter> ();
					mf.sharedMesh = m;

					meshCount++;
				}
			}
		}
		for (int i = meshCount; i < existingWalkables.Count; i++) {
			DestroyImmediate (existingWalkables [i]);
			existingWalkables.RemoveAt (i);
		}
	}

	private Vector2[] getColliderArray(List<Vector3> v3){
		int count = v3.Count;
		int index;
		Vector2[] array = new Vector2[count+1];

		//evens are the top layer, odds are the bottom layer
		for (int i = 0; i < count; i+=2) {
			index = i / 2;
			array [index] = v3 [i];
		}
		for (int i = 1; i < count; i+=2) {
			index = count - i/2 - 1;
			array [index] = v3 [i];
		}

		array [count] = array [0];
		return array;
	}

	private Vector2[] getUVs(List<Vector3> v3){
		Vector2[] v2 = new Vector2[v3.Count];
		for (int i = 0; i < v3.Count; i++) {
			v2 [i] = Vector2.zero;
			//if on the top row
			if (i % 2 == 0) {
				v2 [i].y = 1;
			}
			//every other column
			if (((i - 2) % 4 == 0) || ((i - 3) % 4 == 0)) {
				v2 [i].x = 1;
			}
		}
		return v2;
	}

	void OnDestroy(){
		foreach (MeshFilter m in gameObject.GetComponentsInChildren<MeshFilter>()) {
			Destroy (m.sharedMesh);
		}
	}

	Color GetColorFromInt(int i){
		switch (i) {
		case 0: return Color.white;
		case 1: return Color.blue;
		case 2: return Color.red;
		case 3: return Color.green;
		case 4: return Color.magenta;
		case 5: return Color.yellow;
		case 6: return Color.white;
		default: return Color.cyan;
		}
	}
}

/*		0	2	4	6
 *		o---o---o---o
 * 		|  /|  /|  /|
 * 		| / | / | / |
 * 		|/  |/  |/	|
 * 		o---o---o---o
 * 		1   3	5	7
*/
