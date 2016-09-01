using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class WalkableGenerator : MonoBehaviour {

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
		
	List<Hit> hits = new List<Hit>();
	SpriteRenderer sr;
	float resolution = 0;
	public static float normalRejectPoint = 0.55f;

	public GameObject meshPlatform;

	void Start(){
		sr = gameObject.GetComponent<SpriteRenderer> ();
		SpriteRenderer psr = GameObject.FindGameObjectWithTag ("Player").GetComponent<SpriteRenderer> ();
		float playerHeight = Mathf.Abs(psr.bounds.max.y - psr.bounds.min.y);

		float min_x = sr.bounds.min.x;
		float max_x = sr.bounds.max.x;
		float min_y = sr.bounds.min.y;
		float max_y = sr.bounds.max.y + playerHeight;
		resolution = Mathf.Abs((max_x - min_x) / 100f);
		for(float x = min_x; x <= max_x; x+=resolution){
			GetHits(new Vector2(x, max_y), max_y-min_y, 0, (int)((x - min_x) / resolution));
		}
		FinalizeHits ();
	}

	void GetHits(Vector2 start, float distance, int depth, int _i){
		float skipAmount = 0.001f;
		RaycastHit2D[] rays = Physics2D.LinecastAll(start, start - Vector2.up * distance); //need to exclude all that are not letters
		if(rays.Length == 0){
			return;
		}
		else if(rays.Length == 1){
			if(rays[0].collider.gameObject == this.gameObject){
				if (rays [0].normal.y >= normalRejectPoint) {
					hits.Add (new Hit (rays [0], depth, _i));
				}
				Debug.DrawLine (start, rays [0].point, Color.blue, 5);
				GetHits (rays [0].point - (Vector2.up * skipAmount), distance, depth+1, _i);

				return;
			}
			else{
				return;
			}
		}
		else if(rays.Length > 1){
			bool foundRay = false;
			foreach(RaycastHit2D ray in rays){
				if(ray.collider.gameObject == this.gameObject){
					if (ray.normal.y > normalRejectPoint) {
						hits.Add (new Hit (ray, depth, _i));
					}
					Debug.DrawLine (start, rays [0].point,Color.blue, 5);
					GetHits (ray.point - (Vector2.up * skipAmount), distance, depth+1, _i);
					foundRay = true;
					break;
				}
			}
			if(!foundRay){
				return;
			}
		}
	}

	void FinalizeHits(){
		GenerateMeshHits();
	}

	void GenerateMeshHits(){
		//only use every other hit (0 is entering letter, 1 is exiting letter, 2 is entering letter, 3 is exiting letter, etc)
		List<Vector3> vertices = new List<Vector3>();
		List<int> triangles = new List<int> ();

		int max = hits.OrderByDescending (i => i.depth).FirstOrDefault ().depth;

		for (int i = 0; i <= max; i += 2) {
			vertices.Clear();
			triangles.Clear ();
			List<Hit> layerHits = hits.Where (h => h.depth == i).ToList ();
			if (layerHits.Count < 1)
				continue;

			float platform_height = 0.2f;

			//always add the first point
			vertices.Add (layerHits[0].hit.point);
			vertices.Add (layerHits[0].hit.point - Vector2.up * platform_height);

			int sub_j = 1; //will increase for every time that something is not added to layer
			for(int j = 1; j < layerHits.Count; j++){				
				Hit layerHit = layerHits[j];

				//get distance between two points, 
				//if it's > 2*resolution then move it down
				if (Vector2.Distance (vertices [(j - sub_j) * 2], layerHit.hit.point) > 2 * resolution) {
					layerHit.depth += 2;
					sub_j++;
					if (layerHit.depth >= max)
						max += 2;
				}
				//else add vertices
				else {
					vertices.Add (layerHit.hit.point);
					vertices.Add (layerHit.hit.point - Vector2.up * platform_height);

					//check to see if there are adjacent lines that are on a different depth but should be joined
					Hit nextCol = hits.Where(h => h.i == layerHit.i+1 && Mathf.Abs(h.hit.point.y - layerHit.hit.point.y) < 2 * resolution && h.depth != layerHit.depth).FirstOrDefault();
					if (nextCol != null) {
						vertices.Add (nextCol.hit.point);
						vertices.Add (nextCol.hit.point - Vector2.up * platform_height);
					}
				}
			}

			for (int k = 0; k < vertices.Count - 3; k+=2) {
				triangles.Add (k+1);
				triangles.Add (k);
				triangles.Add (k+2);

				triangles.Add (k + 1);
				triangles.Add (k + 2);
				triangles.Add (k + 3);
			}

			if (triangles.Count > 1) {
				Mesh m = new Mesh ();
				m.vertices = vertices.ToArray();
				m.triangles = triangles.ToArray();
				m.uv = getUVs (vertices);
				m.RecalculateNormals ();

				GameObject newMesh = (GameObject)Instantiate (meshPlatform);
				newMesh.transform.parent = this.transform;
				newMesh.GetComponent<EdgeCollider2D> ().points = getColliderArray (vertices);
				newMesh.transform.position = new Vector3(0,0,-1f);

				MeshFilter mf = newMesh.GetComponent<MeshFilter> ();
				mf.sharedMesh = m;

				MeshRenderer mr = newMesh.GetComponent<MeshRenderer> ();
				mr.sortingLayerName = "Walkable";

				Material mat = new Material(mr.material);
				/*
				SpriteRenderer sr = this.GetComponent<SpriteRenderer> ();
				Color matColor = sr.material.GetColor ("_EffectColor");
				mat.color = matColor;
				*/
				mat.color = GetComponent<Letter> ().canBeHarvested ? new Color (0.4f, 0.4f, 0.14f) : new Color (0.06f, 0.20f, 0.29f); //will probably need to be changed
				mr.material = mat;
			}
		}
	}

	private float CalculateStdDev(List<float> values)
	{   
		float ret = 0;
		if (values.Count() > 0) 
		{      
			//Compute the Average      
			float avg = values.Average();
			//Perform the Sum of (value-avg)_2_2      
			float sum = values.Sum(d => Mathf.Pow(d - avg, 2));
			//Put it all together      
			ret = Mathf.Sqrt((sum) / (values.Count()-1));   
		}   
		return ret;
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
}

/*		0	2	4	6
 *		o---o---o---o
 * 		|  /|  /|  /|
 * 		| / | / | / |
 * 		|/  |/  |/	|
 * 		o---o---o---o
 * 		1   3	5	7
*/
