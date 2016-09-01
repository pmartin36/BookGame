using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Trace : MonoBehaviour {

	private bool showingTrace = false;
	List<GameObject> tr = new List<GameObject>();
	Color32[] pixels;

	public float velocity_magnitude;

	Vector2 pixel_position;
	public Vector2 velocity;
	float angle;

	int pixelwidth;
	int pixelheight;
	float localHeight;
	float localWidth;
	float pixels2units;

	public GameObject trailPrefab;
	public Transform trailDumpster;

	bool isinit = false;
	// Use this for initialization
	void Start () {
		init ();
	}

	void init(){
		trailDumpster = GameObject.Find ("Trail Dumpster").transform;
		trailPrefab = (transform.parent.GetComponent<Letter>().canBeHarvested ? Resources.Load ("Prefabs/LetterTraceTrail") : Resources.Load ("Prefabs/LetterTraceTrail_Dull")) as GameObject;

		SpriteRenderer sr = transform.parent.GetComponent<SpriteRenderer> ();
		Sprite sprite = sr.sprite;
		sr.sortingLayerName = "LetterEffects";
		pixels = sprite.texture.GetPixels32 ();
		pixels2units = sprite.rect.width / sprite.bounds.size.x;
		pixelwidth = (int)sprite.rect.width;
		pixelheight = (int)sprite.rect.height;
		localWidth = sprite.bounds.size.x;
		localHeight = sprite.bounds.size.y;

		angle = Random.value*360;

		float x = Mathf.Sin (Mathf.Deg2Rad * angle);
		float y = Mathf.Cos (Mathf.Deg2Rad * angle);
		Vector2 starting_position; //from -1 to 1
		if (Mathf.Abs (x) >= Mathf.Abs (y)) {
			starting_position = new Vector2 (Mathf.Sign (x), y / Mathf.Abs (x));
		} 
		else {
			starting_position = new Vector2 (x / Mathf.Abs (y), Mathf.Sign (y));

		}

		//convert to world space to get starting position
		this.transform.localPosition = new Vector2 (starting_position.x * localWidth/2, starting_position.y * localHeight/2);

		//convert to pixel space
		//								-1 to 1 -> 0 to 2    ->      0 to (rect width-1)
		pixel_position =  new Vector2((starting_position.x + 1) * (pixelwidth/2-1), (starting_position.y + 1) * (pixelheight/2-1));

		//in pixel space
		velocity = new Vector2(-x * velocity_magnitude, -y * velocity_magnitude); //head toward the center

		CheckNewTrail ();
		isinit = true;
	}

	Vector2 pixeltoLocal(Vector2 pixel){
		//					 (       0      to      width     )  
		return new Vector2 ((pixel.x / pixelwidth * localWidth) - (localWidth/2), (pixel.y / pixelheight * localHeight) - (localHeight/2));
	}
	Vector2 LocaltoPixel(Vector2 local){
		//								 (     0        to            1    )
		return new Vector2 (Mathf.Round((local.x + localWidth/2)/ localWidth * pixelwidth - 1), Mathf.Round((local.y + localHeight/2)/ localHeight * pixelheight - 1));
	}

	// Update is called once per frame
	void Update () {
		if (!isinit)
			init ();

		float seed = Random.value * 100;
		if (seed > 99) {
			//turn
			angle += (Random.value * 60) - 60; //-60 to 60
			float x = Mathf.Sin (Mathf.Deg2Rad * angle);
			float y = Mathf.Cos (Mathf.Deg2Rad * angle);
			velocity.x = -x * velocity_magnitude;
			velocity.y = -y * velocity_magnitude;
		}

		pixel_position.x = pixel_position.x + (velocity.x * Time.deltaTime * 100);
		pixel_position.y = pixel_position.y + (velocity.y * Time.deltaTime * 100);
		this.transform.localPosition = pixeltoLocal (pixel_position);

		CheckNewTrail ();
	}

	void CreateNewTrail(){
		GameObject newtrail = Instantiate (trailPrefab);
		newtrail.transform.parent = this.transform;
		newtrail.GetComponent<Renderer> ().sortingLayerName = "LetterEffects";
		newtrail.transform.localPosition = new Vector3(0,0,-0.9f);
		tr.Add (newtrail);
	}

	void CheckNewTrail(){
		//destroy this object
		Vector2 compPixel = new Vector2(Mathf.Round(pixel_position.x),Mathf.Round(pixel_position.y));
		if (compPixel.x >= pixelwidth || compPixel.x < 0 || compPixel.y >= pixelheight || compPixel.y < 0) {
			DestroyImmediate (this.gameObject);
			return;
		}

		bool shouldShow = pixels [(int)(compPixel.y * pixelwidth) + (int)compPixel.x].a > 0.5f;
		if (shouldShow && !showingTrace) {
			CreateNewTrail ();
			showingTrace = true;
		}
		else if (!shouldShow && showingTrace) {
			var children = GetComponentInChildren<Transform> ();
			foreach (Transform t in children)
				t.parent = trailDumpster;
			showingTrace = false;
		}
	}

	public void setVelocity(float newVelocity){
		velocity = velocity * (newVelocity / velocity_magnitude);
		velocity_magnitude = newVelocity;
	}

	void OnDestroy(){
		foreach (GameObject child in tr) {
			Destroy (child);
		}
	}
}
