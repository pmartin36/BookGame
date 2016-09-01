using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Book : MonoBehaviour {

	int seed = 0;
	SpriteRenderer sr;
	public List<Sprite> openingSprites;
	public List<Texture> openingTextureMaps;

	public List<Sprite> closingSprites;
	public List<Texture> closingTextureMaps;

	public bool isOpen;

	private BoxCollider2D box;
	private EdgeCollider2D edge;

	private List<AudioSource> audio;

	// Use this for initialization
	void Start () {
		sr = GetComponent<SpriteRenderer> ();
		isOpen = false;

		box = GetComponent<BoxCollider2D> ();
		box.enabled = false;
		edge = GetComponent<EdgeCollider2D> ();

		audio = new List<AudioSource>(GetComponents<AudioSource> ());
	}
	
	// Update is called once per frame
	void Update () {
		sr.material.SetInt ("_Seed", Mathf.RoundToInt (++seed / 3));
	}

	public void setOpen(bool open){
		if (open != isOpen) {
			StartCoroutine (openClose (open));
		}
	}

	IEnumerator openClose(bool open){
		float waitTime = .1f;
		if (open) {
			playAudio (0);
			for (int i = 1; i < openingSprites.Count; i++) {
				sr.sprite = openingSprites [i];
				sr.material.SetTexture ("_EffectMap", openingTextureMaps [i]);
				yield return new WaitForSeconds (waitTime);
			}	
				
			edge.points = new Vector2[] { 
				new Vector2 (-4f, -3.5f),
				new Vector2 (-4.46f, -4.46f),
				new Vector2 (4.45f, -4.45f),
				new Vector2 (4f, -3.5f),
				new Vector2 (-4f, -3.5f)
			};

			isOpen = open;
			box.enabled = true;
		}
		else {
			//this.transform.localScale = new Vector3 (transform.localScale.x, transform.localScale.y, 1);
			for (int i = 1; i < closingSprites.Count; i++) {
				sr.sprite = closingSprites [i];
				sr.material.SetTexture ("_EffectMap", closingTextureMaps [i]);
				yield return new WaitForSeconds (waitTime);
			}
			playAudio (1);
			isOpen = open;
		}

		yield return null;
	}

	public void playAudio(int index){
		audio [index].Play ();
	}

	//THIS IS BAD
	//TELL PLAYER TO START HOVERING TOWARDS MIDDLE OF BOOK
	void OnTriggerEnter2D(Collider2D other){		
		if (other.tag == "Player" && isOpen) {
			box.enabled = false;
			other.GetComponent<PlayerController> ().hoverTowardsPoint (new Vector3 (this.transform.position.x, this.transform.position.y - sr.bounds.extents.y / 2, other.transform.position.z));
		}
	}

	void OnDestroy(){
		StopAllCoroutines ();
	}
}
