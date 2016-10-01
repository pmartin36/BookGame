using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	public bool isPanning = false;
	public bool freezeCamera = false; //freeze camera while panning
	public float z_pos;
	public float panTargetTime;
	float panStartTime;
	Bounds boundingBox;

	public Vector3 offset_from_player{ get; private set; }
	public bool player_in_frame { get; private set;}

	Vector3 start;
	float startSize;
	Vector3 end;

	Camera cam;

	public bool DisplayVapor { get; set; }
	Material vaporEffect;

	// Use this for initialization
	void Start () {
		init ();

	}

	void init(){
		cam = GetComponent<Camera> ();

		z_pos = -10;
		panTargetTime = 1f;

		startSize = cam.orthographicSize;
		start = cam.transform.position;

		boundingBox = GameObject.FindGameObjectWithTag ("Background").GetComponent<SpriteRenderer> ().bounds;

		DisplayVapor = false;
		vaporEffect = Resources.Load<Material> ("Materials/Graphic/Vapor");
	}

	// Update is called once per frame
	void Update () {
		if (isPanning) {
			float difference = Time.time - panStartTime;
			if (difference < panTargetTime) {
				transform.position = Vector3.Lerp (start, end, difference / panTargetTime);
			}
			else {
				isPanning = false;
			}
		}
	}

	public void setPan(Vector2 _start, Vector2 _end){
		start = new Vector3 (_start.x, _start.y, z_pos);
		end = new Vector3 (_end.x, _end.y, z_pos);
		panStartTime = Time.time;
		freezeCamera = true;
		isPanning = true;
	}

	public void setLocation(float x, float y){
		if (!freezeCamera) {
			if (cam == null)
				init ();
			
			float width = cam.orthographicSize * cam.aspect;
			float x_with_offset = x;
			float y_with_offset = y;

			if (x + width > boundingBox.max.x) {
				x_with_offset = boundingBox.max.x - width;
			}
			else if (x - width < boundingBox.min.x) {
				x_with_offset = boundingBox.min.x + width;
			}

			if (y + cam.orthographicSize > boundingBox.max.y) {
				y_with_offset = boundingBox.max.y - cam.orthographicSize;
			}
			else if (y - cam.orthographicSize < boundingBox.min.y) {
				y_with_offset = boundingBox.min.y + cam.orthographicSize;
			}
				
			transform.position = new Vector3 (x_with_offset, y_with_offset, z_pos);
			offset_from_player = new Vector3 (x - transform.position.x, y - transform.position.y, 0);
		}
		else {
			offset_from_player = new Vector3 (x - transform.position.x, y - transform.position.y, 0);
		}

		Vector3 w2s = offset_from_player;
		if (Mathf.Abs(w2s.y) > cam.orthographicSize || Mathf.Abs(w2s.x) > cam.orthographicSize * cam.aspect) {
			player_in_frame = false;
		}
		else {
			player_in_frame = true;
		}

	}

	public void setSize(float size){
		cam.orthographicSize = size;
	}
	public float getSize() {
		return cam.orthographicSize;
	}
	public float getAspect(){
		return cam.aspect;
	}

	public void restoreCamera(bool restorePosition){
		cam.orthographicSize = startSize;
		if (restorePosition) {
			cam.transform.position = start;
		}

		freezeCamera = false;
		StopAllCoroutines ();
	}

	public void panSize(float size){
		StartCoroutine (Resize (size));
	}

	public void cameraToLevelSize(){
		float levelSize = boundingBox.extents.y;
		StartCoroutine (Resize (levelSize, true));
	}

	public IEnumerator StartVapor(){
		float startTime = Time.time;
		Color startColor = new Color (1, 1, 1, 0);
		vaporEffect.SetColor ("_Color", startColor);
		vaporEffect.SetFloat ("_Magnitude", 0);

		DisplayVapor = true;
		while (Time.time - startTime <= 3) {
			float lerpinterval = (Time.time - startTime) / 3;
			vaporEffect.SetColor ("_Color", Color.Lerp (startColor, Color.white, lerpinterval));
			vaporEffect.SetFloat ("_Magnitude",  Mathf.Lerp(0, 0.1f, lerpinterval));
			yield return new WaitForEndOfFrame ();
		}
	}

	public IEnumerator StopVapor(){
		float startTime = Time.time;
		Color endColor = new Color (1, 1, 1, 0);
		vaporEffect.SetColor ("_Color", Color.white);
		vaporEffect.SetFloat ("_Magnitude", 0.1f);
		while (Time.time - startTime <= 3) {
			float lerpinterval = (Time.time - startTime) / 3;
			vaporEffect.SetColor ("_Color", Color.Lerp (Color.white, endColor, lerpinterval));
			vaporEffect.SetFloat ("_Magnitude",  Mathf.Lerp(0.1f, 0, lerpinterval));
			yield return new WaitForEndOfFrame ();
		}
		DisplayVapor = false;
	}

	IEnumerator Resize(float size, bool centerOnLevel = false){		
		startSize = cam.orthographicSize;
		start = transform.position;

		if (centerOnLevel) 
			freezeCamera = true;

		float startTime = Time.time;
		while (cam.orthographicSize < size) {
			cam.orthographicSize = Mathf.Lerp (startSize, size, Time.time - startTime);
			if (centerOnLevel) {
				Vector3 pos = Vector3.Lerp (start, boundingBox.center, Time.time - startTime);
				pos.z = z_pos;
				this.transform.position = pos;
			}
			yield return new WaitForEndOfFrame();
		}

		yield return null;
	}

	void OnRenderImage(RenderTexture src, RenderTexture dst){
		if(DisplayVapor)
			Graphics.Blit (src, dst, vaporEffect);
	}
}
