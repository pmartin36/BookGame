using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class PlayerController : MonoBehaviour, LetterEventInterface {

	private PlayerPhysics pphysics;
	private SpriteRenderer spriteRenderer;
	private Animator anim;

	public float horizontal;
	public float vertical;

	private int jumpCount = 0;
	private int jumpCountMax = 1;
	private float jumpTime;
	public bool jumpDown = false;

	public Letter harvestableLetter;
	private bool harvesting = false;

	public bool collidingWithLetter { get; private set; }

	public List<Powerup> powerups = new List<Powerup>();

	public bool playerHasControl = true;

	private CameraController mainCamera;
	private Vector3 lastPosition = Vector3.zero;
	private Vector3 originalPosition;
	private bool focusOnPlayer = true;

	private float directionFacing = -1f;
	public event EventHandler<NewLetterEvent> PowerupChange;

	private bool sliding = false;
	public bool onStringItem = false;

	private List<AudioSource> audio;

	//Item effects
	public bool inputFromMousePosition;
	private GameObject bellows;
	public bool bellows_active = false;

	//private Throwable throwable;
	private ThrowableNew throwable;
	public bool throwable_active = false;
	[SerializeField]
	private GameObject throwable_prefab;

	public GameObject springPrefab;

	private float baseMovespeed;
	public bool Climbing { get; set; }
	[SerializeField]
	private TrailRenderer mstrail;

	GameObject collidingKeyhole;
	TimerPowerup timer;

	bool oppositeInputs = false;
	public ParticleSystem trailingRope { get;  set; }

	//End Item Effects

	void Awake(){
		
	}

	// Use this for initialization
	void Start () {
		spriteRenderer = GetComponent<SpriteRenderer> ();
		pphysics = GetComponent<PlayerPhysics> ();

		originalPosition = transform.position;

		baseMovespeed = pphysics.movespeed;
		jumpTime = 0;

		mainCamera = Camera.main.GetComponent<CameraController> ();
		mainCamera.setLocation (transform.position.x, transform.position.y);

		//items
		bellows = GameObject.Find("bellows");
		bellows.SetActive (false);

		timer = GameObject.Find ("Timer_TimeLeft").GetComponent<TimerPowerup>();

		audio = new List<AudioSource>(GetComponents<AudioSource> ());
		anim = GetComponent<Animator> ();

		collidingWithLetter = false;

		GameManager.signUpForNewLetterEvent (this);
	}
	
	// Update is called once per frame
	void Update () {
		if (pphysics.isSliding != sliding) {
			//make whatever changes are needed in switch state
			sliding = pphysics.isSliding;
			if (sliding) {
				//if we just started sliding
				spriteRenderer.color = Color.green;
				playerHasControl = false;
			} 
			else {
				//if we just ended sliding
				spriteRenderer.color = Color.white;
				playerHasControl = true;
			}
		}

		if (jumpDown && Time.time - jumpTime < 0.2) {
			pphysics.jump ();
		}
			
		if (lastPosition != Vector3.zero) {
			mainCamera.setLocation (transform.position.x, transform.position.y);
		}
		lastPosition = this.transform.position;

		//items
		setBellowsLocation (horizontal, vertical);
		setThrowable (horizontal, vertical);
	}

	public void resetJumpCount(){
		jumpCount = 0;
		jumpTime = 0;
	}

	public int getJumpCount(){
		return this.jumpCount;
	}

	public void resetItemAvailability(int index){
		//0 is reset 0, 1 is reset 1, 3 is reset both
		if ((index == 1 || index == 3) && powerups.Count > 1 && powerups[1] != null) {
			powerups[1].available = true;
		}
		if ((index == 0 || index == 3) && powerups.Count > 0 && powerups[0] != null) {
			powerups[0].available = true;
		}
	}

	void setBellowsLocation(float horizontal, float vertical){
		if (bellows_active) {
			float currentRotation = this.bellows.transform.rotation.eulerAngles.z;
			float stickRotation = 0;
			Vector3 location;
			Vector3 directionalVector;
			if (inputFromMousePosition) {
				directionalVector = new Vector2 (horizontal - transform.position.x, vertical - transform.position.y);
			}
			else {
				directionalVector = new Vector3 (horizontal, vertical);
			}

			if (Mathf.Abs (directionalVector.x) + Mathf.Abs (directionalVector.y) > 0.5f) {
				//we will be making changes to the angle 
				stickRotation = Vector3.Angle (Vector3.right, directionalVector);
				//getting the cross product will tell us whether we are going clockwise or ccw
				//if it's ccw, we set it to -angle
				Vector3 cross = Vector3.Cross (Vector3.right, directionalVector);
				if (cross.z < 0) {
					stickRotation *= -1;
				}

				if (directionalVector.x > 0) {
					transform.localScale = new Vector3 (-Mathf.Abs (transform.localScale.x), transform.localScale.y, transform.localScale.z);
				}
				else if (directionalVector.x < 0) {
					transform.localScale = new Vector3 (Mathf.Abs (transform.localScale.x), transform.localScale.y, transform.localScale.z);
				}

				bellows.transform.Rotate (Vector3.forward, stickRotation - currentRotation);
				location = Vector3.Normalize (directionalVector);
			}
			else {
				//angle will not change
				Vector2 vector = GameManager.angleToVector (currentRotation);
				location = Vector3.Normalize (new Vector3 (vector.x, vector.y, 0));
			}
			bellows.transform.position = transform.position + location;
		}
	}

	void setThrowable(float horizontal, float vertical){
		if (throwable_active) {
			float stickRotation = 0;
			Vector3 directionalVector;

			if (inputFromMousePosition) {
				if(pphysics.PointInCollider(new Vector3(horizontal, vertical, -2))){
					directionalVector = Vector3.zero;
				}
				else{
					directionalVector = new Vector2 (horizontal - transform.position.x, vertical - transform.position.y);
				}
			}
			else {
				directionalVector = new Vector3 (horizontal, vertical);
			}

			if (Mathf.Abs (directionalVector.x) + Mathf.Abs (directionalVector.y) > 0.5f) {
				//we will be making changes to the angle
				stickRotation = Vector3.Angle (Vector3.right, directionalVector);
				//getting the cross product will tell us whether we are going clockwise or ccw
				//if it's ccw, we set it to -angle
				Vector3 cross = Vector3.Cross (Vector3.right, directionalVector);
				if (cross.z < 0) {
					stickRotation *= -1;
				}

				if (directionalVector.x > 0) {
					transform.localScale = new Vector3 (Mathf.Abs (transform.localScale.x), transform.localScale.y, transform.localScale.z);
				}
				else if (directionalVector.x < 0) {
					transform.localScale = new Vector3 (-Mathf.Abs (transform.localScale.x), transform.localScale.y, transform.localScale.z);
				}

				throwable.SetTrajectoryAngle (stickRotation);
			}
			else {
				if (!throwable.IsLaunched) {
					throwable.ClearTrajectory ();
				}
			}
		}
	}

	bool createThrowable(string l, bool isTrace){
		if (l == "G" || l == "Y") {
			if ((throwable != null && throwable.Letter != "G" && throwable.Letter != "Y") || throwable == null) {
				GameObject t = Instantiate (throwable_prefab) as GameObject;
				t.transform.parent = this.transform;
				t.transform.localPosition = new Vector3 (0, spriteRenderer.bounds.extents.y * .25f, 0);

				ThrowableWithTrail twt = t.AddComponent<ThrowableWithTrail> ();
				twt.Thrower = pphysics;
				twt.setLetter (l);
				twt.Init (l == "G"); 
				throwable = twt;
				return true;
			}
		}
		else {
			GameObject t = Instantiate (throwable_prefab) as GameObject;
			t.transform.parent = this.transform;
			t.transform.localPosition = new Vector3 (0, spriteRenderer.bounds.extents.y * .25f, 0);

			throwable = t.AddComponent<ThrowableNew> ();
			throwable.Thrower = pphysics;
			throwable.setLetter (l);
			throwable.Init ();
			return true;
		}
		return false;
	}

	public void setPlayerActive(bool active){
		pphysics.setKinematic (!active);
		playerHasControl = active;
	}

	public void startHarvest(){		
		if (playerHasControl && harvestableLetter != null && harvestableLetter.canBeHarvested && pphysics.OnStableLocation && trailingRope == null){
			if ( !(GameManager.IsPowerupModifier (harvestableLetter.letter) && powerups.Count <= 0) ) {
				//add letter power to power array
				StartCoroutine (harvestLetter ());
			}
		}
	}

	public void jumpPressed(float horizontal = 0){
		if (playerHasControl) {
			if (!jumpDown && jumpCount < jumpCountMax) { //jump just starting
				jumpTime = Time.time;
				jumpDown = true;
				jumpCount++;
				pphysics.jump (horizontal);

				//play audio for jumping
				//playAudio(0);

				if (anim.GetCurrentAnimatorStateInfo (0).IsName("jump")) {
					anim.Play ("jump", 0, 0f);
				}
				anim.SetBool ("isJumping", true);
			}
		}
	}

	public void jumpReleased(){
		jumpDown = false;
	}

	public void horizontal_move(bool jumpCountOverride = false){
		Vector2 velocity = pphysics.getVelocity();
		if (playerHasControl && !Climbing && !pphysics.Grappling) {
			if (oppositeInputs) {
				horizontal = -horizontal;
			}
			if( horizontal * transform.localScale.x < 0){ //different directions, swap player orientation
				transform.localScale = new Vector3(Mathf.Sign(horizontal) * Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
				directionFacing = Mathf.Sign(horizontal);

				if (onStringItem) {
					pphysics.resetStringPosition ();
				}
			}

			if (!onStringItem) {
				bool shouldOverride = Mathf.Abs (horizontal) > 0.3f && jumpCountOverride;
				velocity = pphysics.horizontal_move (horizontal, false, shouldOverride);
			}
		}

		if (Mathf.Abs(velocity.x) > baseMovespeed ) {
			mstrail.enabled = true;
		}
		else {
			mstrail.enabled = false;
		}
	}

	public void c_newLetterEvent(object sender, NewLetterEvent e){
		collidingWithLetter = true;
		Letter newLetter = e.PassedLetter;
		//leaving letter
		if (newLetter == null) {
			collidingWithLetter = false;
			harvestableLetter = null;
		} 
		//landing on letter
		else {
			collidingWithLetter = true;
			harvestableLetter = newLetter.canBeHarvested ? newLetter : null;

			resetJumpCount ();
			resetItemAvailability (3);
		}
	}
		
	void sendHandlerEvent(Letter letter, int changeAmount){
		EventHandler<NewLetterEvent> handler = PowerupChange;
		if (handler != null) {
			handler (this, new NewLetterEvent (letter,changeAmount));
		}
	}

	IEnumerator harvestLetter(){
		//signal the start of the harvest process
		bool colliding = pphysics.CheckLetterCollision (); //the rays haven't been updated after fixedUpdate...we have to make sure we're still on a letter
		if (!harvesting && harvestableLetter != null && !harvestableLetter.Rotating && colliding) {
			pphysics.horizontal_move (0, true, false);
			setPlayerActive (false);

			harvesting = true;
			anim.SetBool ("harvesting", true);
			harvestableLetter.setAsHarvested ();

			float startTime = Time.time;
			while (Time.time - startTime < 1f) {
				if (harvestableLetter.HasDisappeared) {
					break;
				}
				yield return new WaitForEndOfFrame ();
			}

			anim.SetBool ("harvesting", false);

			//if the letter disappeared while we were harvesting
			if (!harvestableLetter.HasDisappeared) {
				bool newPowerup = false;
				string l = harvestableLetter.letter;
				int powerup_count = 1;
				bool check_if_POTD = GameManager.IsPOTD (l); //add checks for all POTD items
				if (check_if_POTD && IncrementPOTDIfExists (l) != null) {	//include all place once and destroy objects 
					EventHandler<NewLetterEvent> handler = PowerupChange;
					if (handler != null) {
						handler (this, new NewLetterEvent (harvestableLetter, 1));
					}
				}
				else { //permanent buffs
					Letter eventLetter = harvestableLetter;
				
					//can only replicate the item in slot 1
					//need to switch the letter to the powerup[0] slot before we send the event
					if (harvestableLetter.letter == "C") {	
						sendHandlerEvent (eventLetter, powerups [0].count > 1 ? powerups [0].count : 0);
						if (powerups.Count > 0 && powerups [0] != null) {
							eventLetter.letter = powerups [0].letter.letter;
							powerup_count = powerups [0].count;
							newPowerup = true;
						}
					}
					//can only use the N powerup with an item in slot 1
					else if (harvestableLetter.letter == "N") {
						if (powerups.Count > 0) {
							while (powerups.Count > 0) {
								PowerupRemoved (powerups [powerups.Count - 1]);
								powerups.RemoveAt (powerups.Count - 1);
							}
						}
						sendHandlerEvent (eventLetter, 0);
					}
					else if (harvestableLetter.letter == "X") {
						foreach (Powerup p in powerups) {
							if (GameManager.IsPOTD (p.letter.letter)) {
								p.count++;
							}
						}
						sendHandlerEvent (eventLetter, 0);
					}
					else if (harvestableLetter.letter == "P") {
						transform.position = originalPosition;
						List<Letter> letters = GameObject.FindGameObjectsWithTag ("Letter").Select (a => a.GetComponent<Letter> ()).ToList();
						foreach (Letter a in letters) {
							if (a != pphysics.collisionLetter) {
								a.ResetLetter ();
							}
						}

						//should we also destroy in air throwables?
					}
					//we're picking up a new powerup
					else {
						newPowerup = true;
						sendHandlerEvent (eventLetter, 0);
					}
				}

				if (newPowerup) {
					Powerup power = new Powerup (harvestableLetter);
					power.count = powerup_count;
					PowerupAdded (power);
					if (powerups.Count > 1 && powerups [0] == null) {
						//if item2 is defined but item1 is null (POTD was used)
						powerups [0] = power;
					}
					else {
						powerups.Insert (0, power);
					}

					while (powerups.Count > 2) {
						PowerupRemoved (powerups [powerups.Count - 1]);
						powerups.RemoveAt (powerups.Count - 1);
					}

					//if something is already mapped to x, store mapping as y
					if (getIndexFromMapping ("X") >= 0) {
						powerups [0].buttonMapping = "Y";
					}
					else {
						powerups [0].buttonMapping = "X";
					}
				}
			}

			harvestableLetter = null;
			harvesting = false;

			if (GameManager.letters_active > 0) {
				setPlayerActive (true);
			}
		}
		yield return null;
	}

	private Letter IncrementPOTDIfExists(string l){
		l = l.ToUpper ();
		foreach (Powerup p in powerups) {
			if (p.letter.letter == l) {
				p.count++;
				return p.letter;
			}
		}
		return null;
	}

	private void DecreasePOTD(int index){
		if (powerups [index].count > 0) {
			powerups [index].count--;
			EventHandler<NewLetterEvent> handler = PowerupChange;
			if (handler != null) {
				handler (this, new NewLetterEvent (powerups [index].letter, -1, index+1));
			}
		}
	}

	private int getIndexFromMapping(string mapping){
		foreach (Powerup p in powerups) {
			if (p != null && p.buttonMapping == mapping)
				return powerups.IndexOf (p);
		}
		return -1;
	}



	public void Item1Down(bool _inputFromMouse = false){
		if (playerHasControl) {
			inputFromMousePosition = _inputFromMouse;
			int index = getIndexFromMapping ("X");
			if (index < 0) return;

			if (powerups.Count >= 1 && powerups[index].available){
				powerups [index].inUse = true;
				if (!GameManager.IsPOTD(powerups[index].letter.letter)) {
					powerups [index].available = false;
				}
				ItemDown (index);
			}
		}
	}

	public void Item1Up(){
		int index = getIndexFromMapping ("X");
		if (index < 0) return;

		if (powerups[index].inUse) {						
			powerups[index].inUse = false;
			ItemUp (index);
		}
	}

	public void Item2Down(bool _inputFromMouse = false){
		if (playerHasControl) {
			inputFromMousePosition = _inputFromMouse;
			int index = getIndexFromMapping ("Y");
			if (index < 0) return;

			if (powerups.Count >= 1 && powerups[index].available){
				powerups [index].inUse = true;
				if (!GameManager.IsPOTD(powerups[index].letter.letter)) {
					powerups [index].available = false;
				}
				ItemDown (index);
			}
		}
	}

	public void Item2Up(){
		int index = getIndexFromMapping ("Y");
		if (index < 0) return;

		if (powerups[index].inUse) {			
			powerups[index].inUse = false;
			ItemUp (index);
		}
	}

	void ItemDown(int index){
		if (GameManager.IsPOTD (powerups [index].letter.letter) && powerups [index].count <= 0) {
			return;
		}
		switch (powerups[index].letter.letter) {
		case "A":
			bellows_active = true;
			playerHasControl = false;
			if (pphysics.collidingWithLetter) {
				pphysics.zeroVelocity ();
			}
			bellows.SetActive (true);
			anim.SetBool ("bellowActive", true);
			pphysics.StartSlowMotion ();
			setBellowsLocation (0, 0);
			break;
		case "B":
			/*Old Balloon
			if (!onStringItem) {
				GameObject balloon = Instantiate (Resources.Load<GameObject> ("Prefabs/balloon"), transform.position + new Vector3 (directionFacing * 0.1f, spriteRenderer.bounds.extents.y), Quaternion.identity) as GameObject;

				LayerMask mask = (1 << LayerMask.NameToLayer ("Walkable")) | (1 << LayerMask.NameToLayer ("Letter")) | (1 << LayerMask.NameToLayer ("Book"));
				CircleCollider2D c = balloon.GetComponent<CircleCollider2D> ();
				RaycastHit2D r = Physics2D.CircleCast (c.bounds.center, c.radius * balloon.transform.localScale.x, Vector2.zero, 0, mask);
				RaycastHit2D l = Physics2D.Linecast (c.bounds.center, transform.position, mask);

				if (r.collider == null && l.collider == null) {
					pphysics.attachToStringItem (balloon);
					DecreasePOTD (index);
				}
				else {
					DestroyImmediate (balloon);
				}
			}
			*/
			//place bouncy platform
			if(pphysics.isAirborn()){
				GameObject spring = Instantiate (Resources.Load<GameObject>("Prefabs/Spring"), transform.position, Quaternion.identity) as GameObject;
				SpriteRenderer sr = spring.GetComponent<SpriteRenderer> ();
				spring.transform.position = spring.transform.position + new Vector3 (0, -spriteRenderer.bounds.extents.y);

				DecreasePOTD (index);
			}
			break;
		case "C":
			break;
		case "D":
			jumpDown = false;
			jumpPressed ();
			break;
		case "E":
			initThrowable (powerups [index].letter.letter);
			break;
		case "F":
			//place floating platform
			if (pphysics.isAirborn()) {
				GameObject cloud = Instantiate (Resources.Load<GameObject> ("Prefabs/cloud"), transform.position + new Vector3 (0, -spriteRenderer.bounds.extents.y * 1.1f), Quaternion.identity) as GameObject;
				DecreasePOTD (index);
			}
			break;
		case "G":
			initThrowable (powerups [index].letter.letter);
			break;
		case "H":
			initThrowable (powerups [index].letter.letter);
			break;
		case "I":
			break;
		case "J":
			break;
		case "K":
			StartCoroutine (useKey (index));
			break;
		case "L":
			initThrowable (powerups [index].letter.letter);
			break;
		case "M":
			initThrowable (powerups [index].letter.letter);
			break;
		case "N":
			break;
		case "O":
			break;
		case "P":
			break;
		case "Q":
			break;
		case "R":
			if (pphysics.collisionLetter != null) {
				pphysics.collisionLetter.SetRotating (true);
			}
			powerups [index].available = true;
			break;
		case "S":
			break;
		case "T":
			break;
		case "U":
			if ( !onStringItem && pphysics.isAirborn() ) {
				GameObject umbrella = Instantiate (Resources.Load<GameObject> ("Prefabs/umbrella"), transform.position + new Vector3 (directionFacing * 0.1f, spriteRenderer.bounds.extents.y), Quaternion.identity) as GameObject;

				LayerMask mask = (1 << LayerMask.NameToLayer ("Walkable")) | (1 << LayerMask.NameToLayer ("Letter")) | (1 << LayerMask.NameToLayer ("Book"));

				EdgeCollider2D c = umbrella.GetComponent<EdgeCollider2D> ();
				Debug.DrawLine (c.bounds.min, c.bounds.max, Color.blue, 1);
				Collider2D r = Physics2D.OverlapArea (c.bounds.min, c.bounds.max, mask);
				RaycastHit2D l = Physics2D.Linecast (c.bounds.center, transform.position, mask);

				if (r != null || l.collider != null) {
					DestroyImmediate (umbrella);
				}
				else {
					pphysics.attachToStringItem (umbrella);
				}
			}
			powerups [index].available = true;
			break;
		case "V":
			
			break;
		case "W":
			pphysics.SetClimbing ();
			powerups [index].available = true;
			break;
		case "X":
			break;
		case "Y":
			initThrowable (powerups [index].letter.letter);
			break;
		case "Z":
			mainCamera.cameraToLevelSize ();
			break;
		default:
			break;
		}
	}

	void ItemUp(int index){
		switch (powerups[index].letter.letter) {
		case "A":
			StartCoroutine(useBellows ());
			break;
		case "B":
			break;
		case "C":
			break;
		case "D":
			jumpDown = false;
			break;
		case "E":
			StartCoroutine(throwThrowable(index));
			break;
		case "F":
			break;
		case "G":
			StartCoroutine(throwThrowable(index));
			break;
		case "H":
			StartCoroutine(throwThrowable(index));
			break;
		case "I":
			break;
		case "J":
			break;
		case "K":
			break;
		case "L":
			StartCoroutine(throwThrowable(index));
			break;
		case "M":
			StartCoroutine(throwThrowable(index));
			break;
		case "N":
			break;
		case "O":
			break;
		case "P":
			break;
		case "Q":
			break;
		case "R":
			break;
		case "S":
			break;
		case "T":
			break;
		case "U":
			//destroy
			Umbrella u = GameObject.FindObjectOfType<Umbrella> ();
			if (u != null) {
				pphysics.exitedStringItemState ();
				DestroyImmediate (u.gameObject);
			}
			break;
		case "V":			
			break;
		case "W":
			pphysics.ExitClimb ();
			break;
		case "X":
			break;
		case "Y":
			StartCoroutine(throwThrowable(index));
			break;
		case "Z":
			mainCamera.restoreCamera (true);
			powerups [index].available = true;
			break;
		default:
			break;
		}
	}

	void PowerupAdded(Powerup p){
		//if a letter is not on the list, nothing has to be done when it is added
		switch (p.letter.letter) {
		case "D":
			jumpCountMax++;
			break;
		case "E":
			break;
		case "F":
			
			//mstrail.gameObject.SetActive(true);
			break;
		case "G":
			
			break;
		case "H":
			break;
		case "I":
			pphysics.IceMove = true;
			break;
		case "J":
			pphysics.jumpSpeed *= 1.75f;
			break;
		case "K":
			break;
		case "L":
			break;
		case "M":
			break;
		case "N":
			break;
		case "O":
			oppositeInputs = true;
			break;
		case "P":
			break;
		case "Q":
			pphysics.movespeed *= 2f;
			break;
		case "R":
			break;
		case "S":
			pphysics.movespeed *= 0.5f;
			break;
		case "T":
			timer.SetTimer ();
			break;
		case "U":
			break;
		case "V":
			StartCoroutine (mainCamera.StartVapor ());
			break;
		case "W":
			break;
		case "X":
			break;
		case "Y":
			break;
		case "Z":
			break;
		default:
			break;
		}
	}

	void PowerupRemoved(Powerup p){
		//if a letter is not on the list, nothing has to be done when it is removed
		switch (p.letter.letter) {
		case "D":
			jumpCountMax--;
			break;
		case "E":
			break;
		case "F":
			
			break;
		case "G":
			
			break;
		case "H":
			break;
		case "I":
			pphysics.IceMove = false;
			break;
		case "J":
			pphysics.jumpSpeed /= 1.75f;
			break;
		case "K":
			break;
		case "L":
			break;
		case "M":
			break;
		case "N":
			break;
		case "O":
			oppositeInputs = false;
			break;
		case "P":
			break;
		case "Q":
			pphysics.movespeed /= 2f;
			mstrail.gameObject.SetActive(false);
			break;
		case "R":
			break;
		case "S":
			pphysics.movespeed *= 2f;
			break;
		case "T":
			timer.StopTimer ();
			break;
		case "U":
			break;
		case "V":
			StartCoroutine (mainCamera.StopVapor ());
			break;
		case "W":
			break;
		case "X":
			break;
		case "Y":
			break;
		case "Z":
			mainCamera.restoreCamera (false);
			break;
		default:
			break;
		}
	}

	//This call comes from Book OnTriggerEnter
	public void StartLevelEnd(Vector3 point){
		setPlayerActive (false);
		timer.StopTimer ();
		StartCoroutine (hoverTowards (point));
	}

	IEnumerator hoverTowards(Vector3 point){
		float startTime = Time.time;
		Vector3 startingPosition = transform.position;
		float startingSize = mainCamera.getSize ();

		playerHasControl = false;
		pphysics.exitedStringItemState ();
		if (throwable_active) {
			CancelThrowable ();
		}
		if (bellows_active) {
			CancelBellows ();
		}

		//mainCamera.setLocation (point.x, point.y);
		//mainCamera.freezeCamera = true;

		float timeDif = 0;
		while (transform.position != point && (timeDif/1f <= 1f)) {
			timeDif = Time.time - startTime;
			transform.position = Vector3.Lerp (startingPosition, point, timeDif / 1f);
			//mainCamera.setSize(Mathf.Lerp(startingSize,1.5f,Time.time - startTime / 1));
			yield return new WaitForSeconds (0.001f);
		}
		jumpTime = 1000;
		pphysics.zeroVelocity ();
		GameManager.endLevel ();
		yield return null;
	}

	public void playAudio(int index){
		audio [index].Play ();
	}

	IEnumerator useBellows(){
		if (bellows_active) {
			SpriteRenderer sr = bellows.GetComponent<SpriteRenderer> ();
			anim.SetBool ("bellowActive", false);

			//we use the opposite of how we calculated the angle because we want to blow in the opposite direction
			pphysics.zeroVelocity ();
			pphysics.ApplyForce (bellows.transform.rotation * Vector3.left * 300);

			bellows_active = false;
			Sprite sprite = Resources.Load<Sprite> ("Sprites/bellows_compressed_with_air");
			sr.sprite = sprite;

			if (pphysics.collidingWithLetter) {
				resetItemAvailability (3);
				resetJumpCount ();
			}
			else if (jumpCount == 0) {
				jumpCount = 1;
			}

			pphysics.InSlowMotion = false;

			yield return new WaitForSeconds (0.2f);

			playerHasControl = true;
			sr.sprite = Resources.Load<Sprite> ("Sprites/bellows");
			bellows.SetActive (false);
		}
		yield return null;
	}

	IEnumerator useKey(int index){
		playerHasControl = false;

		if (collidingKeyhole != null) {
			DecreasePOTD (index);
			DestroyImmediate (collidingKeyhole.transform.parent.gameObject);
		}

		playerHasControl = true;
		yield return null;
	}

	void initThrowable(string letter){
		bool creationSuccessful = createThrowable (letter, false);
		if (creationSuccessful) {
			playerHasControl = false;
			throwable_active = true;
			anim.SetBool ("throwableActive", true);

			if (pphysics.collidingWithLetter) {
				pphysics.zeroVelocity ();
			}

			pphysics.StartSlowMotion ();
		}
	}

	IEnumerator throwThrowable(int index){
		anim.SetBool ("throwableActive", false);
		pphysics.InSlowMotion = false;

		if(throwable_active){
			throwable_active = false;
			if (throwable.ValidTrajectory) {
				//set player to throw sprite
				//

				throwable.launch ();

				//start throw animation
				//

				DecreasePOTD (index);
				yield return new WaitForSeconds (0.2f);

			}
			else {
				DestroyImmediate (throwable.gameObject);
			}

			//set player back to idle
			//
		}

		playerHasControl = true;

		yield return null;
	}

	public void CancelThrowable(){
		if (throwable_active) {
			Destroy (throwable.gameObject);

			throwable_active = false;
			anim.SetBool ("throwableActive", false);

			playerHasControl = true;
		}
		inputFromMousePosition = false;
	}

	public void CancelBellows(){
		if (bellows_active){
			bellows_active = false;
			anim.SetBool ("bellowActive", false);
			bellows.SetActive (false);
			playerHasControl = true;
		}
		inputFromMousePosition = false;
	}

	void OnTriggerEnter2D(Collider2D other){
		if (other.tag == "Keyhole") {
			collidingKeyhole = other.gameObject;
		}
	}

	void OnTriggerExit2D(Collider2D other){
		if (other.tag == "Keyhole") {
			collidingKeyhole = null;
		}
	}

	void OnDestroy(){
		StopAllCoroutines ();
	}
}

