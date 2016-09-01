using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent (typeof(WalkableGeneratorNew))]
public class Letter : MonoBehaviour,LetterEventInterface {

	private SpriteRenderer spriteRenderer;
	private Rigidbody2D rigid;

	public bool canBeHarvested;
	private bool blinkReddening = false;
	private float colorVal = 1f;
	private bool stopBlinking = false;

	bool player_colliding = false;
	float scroll_speed = 1;

	float effectPosition = 0;
	public string letter;

	Bounds boundingBox;

	public Vector3 MoveAmount { get; private set; }
	public bool HasDisappeared { get; private set; }

	// Use this for initialization
	void Start () {
		spriteRenderer = GetComponent<SpriteRenderer> ();
		rigid = GetComponent<Rigidbody2D> ();

		Material rMat;
		if (canBeHarvested) {
			rMat = new Material (Resources.Load ("Materials/Graphic/Active Letter") as Material);
			GameManager.increment_letters_active ();
		}
		else {
			rMat = new Material (Resources.Load ("Materials/Graphic/Inactive Letter") as Material);
		}
		rMat.SetTexture ("_MainTex", spriteRenderer.sprite.texture);
		spriteRenderer.sharedMaterial = rMat;
		HasDisappeared = false;

		boundingBox = GameObject.FindGameObjectWithTag ("Background").GetComponent<SpriteRenderer> ().bounds;

		GameManager.signUpForNewLetterEvent (this);
	}
	
	// Update is called once per frame
	void Update () {
		effectPosition += Time.deltaTime * (scroll_speed) / 30f;
		spriteRenderer.material.SetFloat ("_EffectOffset", effectPosition);
	}

	void FixedUpdate(){
		if (MoveAmount != Vector3.zero) {
			rigid.interpolation = RigidbodyInterpolation2D.Interpolate;
			float halfheight = spriteRenderer.bounds.extents.y;
			float halfwidth = spriteRenderer.bounds.extents.x;

			Vector3 v = MoveAmount;

			//check if out of bounds
			if (MoveAmount.y > 0 && transform.position.y + halfheight + v.y > boundingBox.max.y) {
				v.y = boundingBox.max.y - (transform.position.y + halfheight);
			}
			if (MoveAmount.y < 0 && transform.position.y - halfheight + v.y < boundingBox.min.y) {
				v.y = boundingBox.min.y - (transform.position.y - halfheight);
			}
			if (MoveAmount.x > 0 && transform.position.x + halfwidth + v.x > boundingBox.max.x) {
				v.x = boundingBox.max.x - (transform.position.x + halfwidth);
			}
			if (MoveAmount.x < 0 && transform.position.x - halfwidth + v.x < boundingBox.min.x) {
				v.x = boundingBox.min.x - (transform.position.x - halfwidth);
			}

			MoveAmount = v;
			if (MoveAmount == Vector3.zero) {
				//remove extra polygon colliders
			}

			transform.Translate (MoveAmount);
		}
		else {
			rigid.interpolation = RigidbodyInterpolation2D.None;
		}
	}

	public void SetMoveAmount(Vector3 v){
		if (v != Vector3.zero) {
			foreach (PolygonCollider2D p in GetComponents<PolygonCollider2D>()) {
				PolygonCollider2D opoly = gameObject.AddComponent<PolygonCollider2D> ();
				opoly.offset = new Vector2(v.x, v.y);
				opoly.isTrigger = true;
				opoly.points = p.points;
			}
		}
		else {
			foreach (PolygonCollider2D p in GetComponents<PolygonCollider2D>()) {
				if (p.isTrigger) {
					Destroy (p);
				}
			}
		}

		MoveAmount = v;
	}

	void OnCollisionEnter2D(Collision2D col){
		//Debug.Log (this.name + " " + col.gameObject.name);
		if (col.collider.tag.Equals ("Player")) {
			if (canBeHarvested) { 
				//StopCoroutine ("SpriteBlink");
				//StartCoroutine ("SpriteBlink");
				//GetComponent<TraceSpawner>().setVelocity(5);
				//spriteRenderer.material.SetInt("_EffectSpeed",5);
			}
		}
		else if(col.collider.tag.Equals("Letter") || col.collider.tag.Equals("Walkable") || col.collider.tag.Equals("TempWalkable")){
			MoveAmount = Vector3.zero;
		}
	}


	void OnCollisionExit2D(Collision2D col){
		if (col.collider.tag.Equals ("Player")) {
			//stopBlinking = true;
			//GetComponent<TraceSpawner>().setVelocity(1);
			//spriteRenderer.material.SetInt("_EffectSpeed",1);
		}
	}

	void OnTriggerEnter2D(Collider2D col){
		if (col.tag == "Walkable" || col.tag == "Letter" || col.tag == "Book") {
			SetMoveAmount (Vector3.zero);
		}
		else if (col.tag == "Player") {
			//PlayerPhysics p = col.GetComponent<PlayerPhysics> ();
			//p.CheckForPlayerSquished (MoveAmount);
		}
	}

	IEnumerator SpriteBlink(){
		while (true) {
			int interval = blinkReddening ? 1 : -1;

			colorVal = colorVal + (0.1f * interval);
			setLetterColor (colorVal);

			if (colorVal <= 0) {
				blinkReddening = !blinkReddening;
			}
			else if(colorVal >= 1) {
				blinkReddening = !blinkReddening;
				if (stopBlinking) {
					stopBlinking = false;
					break;
				}
			}

			yield return new WaitForSeconds (0.1f);
		}
		yield return null;
	}

	void setLetterColor(float nonRedValues){
		Color color = spriteRenderer.color;
		color.b = nonRedValues;
		color.g = nonRedValues;
		spriteRenderer.color = color;
	}

	public void setAsHarvested(){
		/*
		canBeHarvested = false;

		//set new material as inactive material
		spriteRenderer.material = new Material (Resources.Load ("Materials/Graphic/Inactive Letter") as Material);
		player_colliding = false;

		//match the walkable
		Color matColor = spriteRenderer.material.GetColor("_EffectColor");
		foreach (Transform child in this.transform) {
			if (child.tag == "Walkable") {
				child.GetComponent<MeshRenderer> ().material.color = spriteRenderer.material.GetColor ("_EffectColor");
			}
		}

		GameManager.decrement_letters_active ();
		*/
		StartCoroutine (TransitionToHarvested ());
	}

	public void startDisappear(){
		StartCoroutine (TransitionToDisappear ());
	}

	IEnumerator TransitionToHarvested(){
		float startTime = Time.time;

		//comparison material (the end material)
		Material compMaterial = new Material (Resources.Load ("Materials/Graphic/Inactive Letter") as Material);

		Color startColor = spriteRenderer.material.GetColor ("_EffectColor");
		Color endColor = compMaterial.GetColor ("_EffectColor");

		//Will need to change colors for the walkable platforms as well
		List<Material> walkable_mats = new List<Material> ();
		foreach (Transform child in this.transform) {
			if (child.tag == "Walkable") {
				walkable_mats.Add (child.GetComponent<MeshRenderer> ().material);
			}
		}

		while (Time.time - startTime <= 1f) {
			float timeElapsed = Time.time - startTime;
			Color newColor = Color.Lerp (startColor, endColor, timeElapsed);

			spriteRenderer.material.SetColor("_EffectColor", newColor);
			scroll_speed = Mathf.Lerp (3f, 1f, timeElapsed);

			//if letter is disappearing, walkable surfaces need to change with it
			newColor.a *= spriteRenderer.material.GetColor ("_ColorMultiplier").a;
			foreach (Material mat in walkable_mats) {
				mat.color = newColor;
			}

			yield return new WaitForEndOfFrame ();
		}

		if (!HasDisappeared) {
			canBeHarvested = false;
			player_colliding = false;
			GameManager.decrement_letters_active ();
		}

		yield return null;
	}

	IEnumerator TransitionToDisappear(){
		MeshRenderer[] mrs = GetComponentsInChildren<MeshRenderer>();
		float startTime = Time.time;

		//fade out
		while (spriteRenderer.material.GetColor("_ColorMultiplier").a > 0) {
			float alpha = Mathf.Lerp (1, 0, Time.time - startTime);

			Color color = spriteRenderer.material.GetColor("_ColorMultiplier");
			color.a = alpha;
			spriteRenderer.material.SetColor ("_ColorMultiplier", color);

			foreach (MeshRenderer mr in mrs) {
				color = mr.material.color;
				color.a = alpha;
				mr.material.color = color;
			}

			yield return new WaitForEndOfFrame ();
		}

		//disable colliders (both walkable and letter)
		foreach (MeshRenderer mr in mrs) {
			mr.GetComponent<EdgeCollider2D> ().enabled = false;
			mr.enabled = false;
		}
		//GetComponent<EdgeCollider2D> ().enabled = false;
		GetComponent<PolygonCollider2D> ().enabled = false;

		//set properties that it's disappeared
		HasDisappeared = true;
		if (canBeHarvested) {
			GameManager.decrement_letters_active ();
		}
		yield return null;
	}

	public void c_newLetterEvent(object sender, NewLetterEvent e){
		if (e.PassedLetter == this && canBeHarvested) {
			player_colliding = true;
			scroll_speed = 3;
		}
		else if (e.PassedLetter == null && player_colliding) {
			player_colliding = false;
			scroll_speed = 1;
		}
	}

	void OnDestroy(){
		StopAllCoroutines ();
	}
}
