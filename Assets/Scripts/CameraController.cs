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

	public bool EndLevel {get; set;}
	public bool FadeInProgress {get;set;}
	Material fadeEffect;

	public bool DeathInProgress {get;set;}
	Material deathEffect;

	public bool PlayerSpawning {get;set;}
	public bool PlayerDying { get; set; }

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

		EndLevel = false;
		fadeEffect = Resources.Load<Material> ("Materials/Graphic/EndLevelFade");

		DeathInProgress = false;
		deathEffect = Resources.Load<Material> ("Materials/Graphic/CameraDeath");
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

	public void FindCameraCoordsInsideBox(ref Vector3 cc){
		float width = cam.orthographicSize * cam.aspect;

		if (cc.x + width > boundingBox.max.x) {
			cc.x = boundingBox.max.x - width;
		}
		else if (cc.x - width < boundingBox.min.x) {
			cc.x = boundingBox.min.x + width;
		}

		if (cc.y + cam.orthographicSize > boundingBox.max.y) {
			cc.y = boundingBox.max.y - cam.orthographicSize;
		}
		else if (cc.y - cam.orthographicSize < boundingBox.min.y) {
			cc.y = boundingBox.min.y + cam.orthographicSize;
		}
	}

	public void setPan(Vector2 _start, Vector2 _end){
		start = new Vector3 (_start.x, _start.y, z_pos);
		end = new Vector3 (_end.x, _end.y, z_pos);
		FindCameraCoordsInsideBox(ref end);

		panStartTime = Time.time;
		freezeCamera = true;
		isPanning = true;
	}

	public void setLocation(float x, float y){
		if (!freezeCamera) {
			if (cam == null)
				init ();

			Vector3 cc = new Vector3 (x, y, z_pos);
			FindCameraCoordsInsideBox (ref cc);
				
			transform.position = cc;
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
		float levelSizeY = boundingBox.extents.y;
		float levelSizeX = boundingBox.extents.x / cam.aspect;

		StartCoroutine (Resize (Mathf.Min(levelSizeX, levelSizeY), true));
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

	public void EndLevelFade(){
		EndLevel = true;
		FadeInProgress = true;
		StartCoroutine(Fade(true, 1, fadeEffect));
	}

	public void PlayerSpawnFadeIn(){
		FadeInProgress = true;
		StartCoroutine(Fade(false, .2f, fadeEffect));
	}

	public IEnumerator Fade(bool isFadingOut, float timeToFade, Material m){
		float startTime = Time.time;
		float opacity, startOpacity, endOpacity;
		if (isFadingOut) {
			opacity = 0;
			startOpacity = 0;
			endOpacity = 1;
		}
		else {
			opacity = 1f;
			startOpacity = 1f;
			endOpacity = 0;
		}

		while (Mathf.Abs (opacity - endOpacity) > 0.01) {
			opacity = Mathf.Lerp (startOpacity, endOpacity, Mathf.SmoothStep(0,1,(Time.time - startTime) / timeToFade));
			m.SetColor ("_Color", new Color (0, 0, 0, opacity));
			yield return new WaitForSeconds(timeToFade/100);
		}

		FadeInProgress = false;
	}

	public void SetFadeOpacity(float o){
		fadeEffect.SetColor ("_Color", new Color (0, 0, 0, o));
	}

	public void UpdateDeathEffect(float time, Vector3 pos){
		deathEffect.SetFloat ("_TimeDiff", time);

		Vector4 v = new Vector4 (pos.x, pos.y, 0, 1);
		deathEffect.SetVector ("_Center", v);
	}

	void OnRenderImage(RenderTexture src, RenderTexture dst){
		if (FadeInProgress || EndLevel) {
			Graphics.Blit (src, dst, fadeEffect);
		}
		else if (DisplayVapor) {
			Graphics.Blit (src, dst, vaporEffect);
		}
		else if(DeathInProgress){
			Graphics.Blit (src, dst, deathEffect);
		}
	}
}
