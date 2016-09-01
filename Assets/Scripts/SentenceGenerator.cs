using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SentenceGenerator : MonoBehaviour {

	// Use this for initialization
	void Start () {
		int num = 6;
		int num_sentences = 10;
		//List<int> used = new List<int> ();
		for (int i = -num; i <= 6; i++) {
			GameObject sentence = new GameObject ((i*2).ToString());
			sentence.transform.parent = this.transform;
			sentence.transform.position = new Vector3 (this.transform.position.x, this.transform.position.y + 2 * i, this.transform.position.z);
			SpriteRenderer sr = sentence.AddComponent<SpriteRenderer> ();

			string image = "Sprites/Background Text/" + Random.Range (1, num_sentences);
			sr.sprite = Resources.Load<Sprite> (image);
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
