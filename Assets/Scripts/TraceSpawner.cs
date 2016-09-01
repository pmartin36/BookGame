using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TraceSpawner : MonoBehaviour{

	public int numTraces;
	private List<Trace> traces = new List<Trace> ();
	private int checkIndex = 0;
	public GameObject tracePrefab;
	private float spawnVelocity = 2f;

	// Use this for initialization
	void Start () {
		for (int i = 0; i < numTraces; i++) {
			traces.Add (spawnNewTrace ());
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (traces [checkIndex] == null) {
			traces [checkIndex] = spawnNewTrace ();
		}
		checkIndex = ++checkIndex % numTraces;
	}

	Trace spawnNewTrace(){
		GameObject newtrace = Instantiate(tracePrefab);
		newtrace.transform.parent = this.transform;
		Trace newt = newtrace.GetComponent<Trace> ();
		newt.setVelocity (spawnVelocity);
		bool canbeharvested = GetComponent<Letter> ().canBeHarvested;
		newt.trailPrefab = (canbeharvested ? Resources.Load ("Prefabs/LetterTraceTrail") : Resources.Load ("Prefabs/LetterTraceTrail_Dull")) as GameObject;
		return newt;
	}

	public void setVelocity(float newVelocity){
		spawnVelocity = newVelocity;
		foreach (Trace trace in traces)
			trace.setVelocity (newVelocity);
	}
}
