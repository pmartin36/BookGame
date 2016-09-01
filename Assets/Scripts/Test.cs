using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Test : MonoBehaviour {

	//test creating a mesh and applying a material
	int rows = 2;
	int columns = 4;
	//Item d;
	void Update(){
		transform.Translate(new Vector3(0.1f,0,0));
	}

	/*
	void Start () {
		Vector3[] vertices = new Vector3[rows*columns];
		int[] triangles = new int[3*2*(rows-1)*(columns-1)];

		for (int y = 0; y < rows; y++) {
			for (int x = 0; x < columns; x++) {
				vertices [y * columns + x] = new Vector3 (x, y, 0);
			}
		}

		for (int i = 0; i < triangles.Length; i+=6) {
			int v = i / 6;
			triangles [i] = v;
			triangles [i + 1] = v + columns;
			triangles [i + 2] = v + 1;

			triangles [i + 3] = v + 1;
			triangles [i + 4] = v + columns;
			triangles [i + 5] = v + columns + 1;
		}

		Material mat = (Material)Resources.Load ("Materials/Graphic/Walkable");

		Mesh m = new Mesh ();
		m.vertices = vertices;
		m.triangles = triangles;
		m.uv = v2 (vertices);
		m.RecalculateNormals ();

		MeshFilter mf = gameObject.AddComponent<MeshFilter> ();
		mf.sharedMesh = m;

		MeshRenderer mr = gameObject.AddComponent<MeshRenderer> ();
		mr.sharedMaterial = mat;
	}

	Vector2[] v2 (Vector3[] v3){
		Vector2[] v2 = new Vector2[v3.Length];
		for (int i = 0; i < v3.Length; i++) {
			v2 [i] = Vector2.zero;
			if (i % 2 != 0)
				v2 [i].x = 1;
			if (i < columns)
				v2 [i].y = 1;
		}
		return v2;
	}
	*/

	/*
	Polymorphism example
	Will find whatever type of object d is and use its move method
	void Start () {
		d = GameObject.FindObjectOfType<Item> ();
	}

	void Update(){
		d.move ();
	}
	*/
}
