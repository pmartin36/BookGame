using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class PlayerPhysics : MonoBehaviour {

	private Animator anim;

	private Rigidbody2D rigid;
	private BoxCollider2D side;
	private CircleCollider2D bottom;

	private PhysicsMaterial2D slippery;
	private PhysicsMaterial2D friction;

	public float movespeed { get; set; }
	public float jumpSpeed { get; set; }

	public bool InSlowMotion {get; set;}
	private float timeScaleSetting = 1f;
	private bool transitioningBacktoNormalTime = false;

	private Vector3 lastVelocity;
	private float velocityXSmooth;

	public List<Hit> hits;

	private float height, width;

	public bool collidingWithLetter = false;
	public Letter collisionLetter;
	private bool exitEvent = false;
	private bool letterEvent = false;

	public event EventHandler<NewLetterEvent> NewLetter;

	public bool isSliding = false;
	public float eulerAngle = 0;

	//the raycasts that are sent out each fixedupdate
	private Raycaster raycaster;
	private List<Hit> upRays;
	private List<Hit> downRays;
	private List<Hit> leftRays;
	private List<Hit> rightRays;
	private bool raycasted = false;

	public Vector2 previousPosition;
	Vector2 lastGroundedPosition;
	Vector2 previousFrameVelocity;

	public bool OnStableLocation { get; private set; }

	[SerializeField]
	private GameObject boundingObject;
	private Bounds boundingbox;

	private bool canMoveLeft = true;
	private bool canMoveRight = true;
	private bool canJump = true;

	public bool IceMove { get; set; }

	private bool isJumping = false;
	private bool jumpUsed = false;
	private bool adjusting_position = false;

	private float baseGravity = 1f;
	private float multiplierGravity = 1f;
		
	private Rigidbody2D stringRigid;
	private bool canDropFromBalloon = false;
	private float stringGrabOffset;

	public bool Grappling { get; private set; }
	Vector3 grappleVector;
	Vector3 grappleStartPoint;
	Vector3 grappleEndPoint;
	List<RopeSegment> throwableRope = new List<RopeSegment>();

	private bool wallStuck = false;

	private PlayerController playerController;

	public float sAdjusttemp = 0.292f;

	Vector2 collisionNormal;
	bool horizontal_set;

	void Awake(){
		rigid = GetComponent<Rigidbody2D> ();

		side = GetComponent<BoxCollider2D> ();
		bottom = GetComponent<CircleCollider2D> ();

		anim = GetComponent<Animator> ();

		slippery = Resources.Load ("Materials/Physics/SlipperyPlayer") as PhysicsMaterial2D;
		friction = Resources.Load ("Materials/Physics/FrictionPlayer") as PhysicsMaterial2D;

		movespeed = 3f;
		jumpSpeed = 4f;

		InSlowMotion = false;

		hits = new List<Hit> ();

		height = Mathf.Abs (bottom.bounds.min.y - side.bounds.max.y);
		width = Mathf.Abs (side.bounds.min.x - side.bounds.max.x);

		IceMove = false;

		//get player bounds
		boundingbox = boundingObject.GetComponent<SpriteRenderer>().bounds;

		InitializeRaycaster ();

		horizontal_set = false;
	}

	private void InitializeRaycaster(){
		var layermask = ~(1 << LayerMask.NameToLayer ("Player") | 1 << LayerMask.NameToLayer ("StringItem") | 1 << LayerMask.NameToLayer("Throwable"));
		raycaster = new Raycaster (this.gameObject, layermask);
		upRays = new List<Hit> ();
		downRays = new List<Hit> ();
		rightRays = new List<Hit> ();
		leftRays = new List<Hit> ();
	}

	void Start () {
		
	}
		
	void Update(){
		
	}

	void FixedUpdate(){
		PerformRaycast();
		raycasted = false;
		horizontal_set = false;

		List<Hit> walkablehits = downRays.Where (s => s.raycastHit.collider != null && !s.raycastHit.collider.isTrigger && (s.raycastHit.collider.tag.Equals ("Walkable") || s.raycastHit.collider.tag.Equals("TempWalkable"))).ToList();
		List<Hit> letterhits = downRays.Where (s => s.raycastHit.collider != null && !s.raycastHit.collider.isTrigger && s.raycastHit.collider.tag.Equals ("Letter")).ToList ();

		if (exitEvent) {
			if (walkablehits.Count <= 0) {
				LetterEventOccur (null);

				collidingWithLetter = false;
				exitEvent = false;
				canDropFromBalloon = true;
			}
		} 
			
		//check for a stuck condition that is not caught by normal physics
		CheckIfStuck(walkablehits,letterhits);

		//ledge stuff
		SetMovementOptions(walkablehits,letterhits);

		//if our uprays ran in to anything we want to kill velocity - now handled in oncollisionenter
		StopIfUpRayCollision ();

		updateStringVelocity ();

		//adjust for movement on slopes
		SlopeAdjust ();

		//counteracting liftoff when changing slope angles
		GroundPlayer ();

		if (collidingWithLetter && collisionLetter.MoveAmount != Vector3.zero){
			transform.Translate (collisionLetter.MoveAmount);
			//if the player is on moving letter, out of bounds checking is done below.  If not, it is handled in OnCollisionEnter or Stay
		}

		if (InSlowMotion) {
			GameManager.SetTimeScale (timeScaleSetting);
		}
		else {
			GameManager.SetTimeScale (1f);
		}

		Hit centerhit = walkablehits.Where (h => h.horizontal == 0).FirstOrDefault ();
		if (centerhit != null){// && (walkablehits.Count > 1 || centerhit.raycastHit.normal.y >= 0.99f)) {
			//store last position in case player gets stuck in an illegal position
			lastGroundedPosition = transform.position;
			OnStableLocation = true;
		}
		else {
			OnStableLocation = false;
		}

		if (Grappling) {
			float playerMoveMag = (this.transform.position - grappleStartPoint).sqrMagnitude;
			float distanceToMoveMag = (grappleEndPoint - grappleStartPoint).sqrMagnitude;
			if (playerMoveMag >= distanceToMoveMag) {
				StopGrapple ();
			}
			else {
				//move
				rigid.velocity = grappleVector * 8f;
				transform.localScale = new Vector3(Mathf.Sign(rigid.velocity.x) * Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
			}
				
			while(throwableRope[0].SqrDistanceFromStart < playerMoveMag){
				Destroy (throwableRope [0].gameObject);
				throwableRope.RemoveAt (0);
			}
		}

		//check if out of bounds
		if (transform.position.y + height / 2f > boundingbox.max.y) {
			if (collisionLetter == null || collisionLetter.MoveAmount.y <= 0) {
				transform.position = new Vector3 (transform.position.x, boundingbox.max.y - height / 2f, transform.position.z);
				rigid.velocity = new Vector2 (rigid.velocity.x, 0);
			}
			else {
				//pushed out by letter
				GameManager.ResetLevel ();
			}
		}
		if (transform.position.y - height / 2f < boundingbox.min.y) {
			GameManager.ResetLevel ();
		}
		if (transform.position.x + width / 2f > boundingbox.max.x) {
			if (collisionLetter == null || collisionLetter.MoveAmount.x <= 0) {
				transform.position = new Vector3 (boundingbox.max.x - width / 2f, transform.position.y, transform.position.z);
				rigid.velocity = new Vector2 (0, rigid.velocity.y);
			}
			else {
				//pushed out by letter
				if (leftRays.Any (s => s.vertical == 0)) {
					GameManager.ResetLevel ();
				}
			}
		}
		if (transform.position.x - width / 2f < boundingbox.min.x) {
			if (collisionLetter == null || collisionLetter.MoveAmount.x <= 0) {
				transform.position = new Vector3 (boundingbox.min.x + width / 2f, transform.position.y, transform.position.z);
				rigid.velocity = new Vector2 (0, rigid.velocity.y);
			}
			else {
				//pushed out by letter
				if (rightRays.Any (s => s.vertical == 0)) {
					GameManager.ResetLevel ();
				}
			}
		}

		previousFrameVelocity = rigid.velocity;
		previousPosition = transform.position;
	}

	public bool CheckLetterCollision(){
		PerformRaycast ();
		return downRays.Any (s => s.raycastHit.collider != null && !s.raycastHit.collider.isTrigger && s.raycastHit.collider.tag.Equals ("Walkable"));
	}

	private bool CheckIfPositionInBounds(float xOffset = 0, float yOffset = 0){
		if (yOffset != 0) {
			if (transform.position.y + yOffset > boundingbox.max.y) {
				return false;
			}
			else if(transform.position.y + yOffset < boundingbox.min.y) {
				return false;
			}
		}
		else if (xOffset != 0) {
			if (transform.position.x + xOffset > boundingbox.max.x) {
				return false;
			}
			else if(transform.position.x + xOffset < boundingbox.min.x) {
				return false;
			}
		}
		return true;
	}

	void StopIfUpRayCollision(){
		if (rigid.velocity.y > 0) {
			Hit uphit = upRays.Where (h => h.horizontal != 0).FirstOrDefault ();
			if (upRays.Count >= 1) {
				//rigid.velocity = new Vector2 (rigid.velocity.x, 0);
				playerController.jumpDown = false;
			}
		}
	}

	void GroundPlayer(){
		if (collisionNormal == Vector2.zero && collidingWithLetter && stringRigid == null && !rigid.isKinematic) {
			//if we're supposed to be colliding with letter but we aren't
			if(playerController.getJumpCount() <= 0){
				//check if center raycast has a hit
				Hit walkablehit = downRays.Where (s => s.raycastHit.collider.tag.Equals ("Walkable") && s.horizontal == 0).FirstOrDefault();
				if (walkablehit != null) {
					//send down to keep grounded
					rigid.velocity += new Vector2(0, -Mathf.Abs((walkablehit.colliderCrossPoint.y+rigid.velocity.y) - walkablehit.raycastHit.point.y));
				}
			}
		}
	}

	void CheckIfStuck(List<Hit> walkable, List<Hit> letter){
		if (isJumping && Vector2.SqrMagnitude (rigid.velocity) < 0.001f) {
			if (walkable.Count <= 0 && letter.Count <= 0 && rigid.velocity == Vector2.zero && !wallStuck) {
				ApplyForce ( rightRays.Count >= 1 ? Vector3.left : Vector3.right * 5);
			}
		}
	}

	void SetMovementOptions(List<Hit> walkable, List<Hit> letter){
		if (walkable.Count > 0) {
			if (walkable.Any (h => h.horizontal == 0)) {
				//set whether player is jumping
				anim.SetBool ("isJumping", false);
				isJumping = false;

				enableAllMovement ();
			}
			else if (walkable.Count >= 2) {
				//both sides are hit
				anim.SetBool ("isJumping", false);
				isJumping = false;

				enableAllMovement ();
			}
			else if (walkable.Any (h => h.horizontal < 0)) {
				//left hit only
				canMoveLeft = false;
				canMoveRight = true;
				canJump = false;
			}
			else {
				//right hit only
				canMoveLeft = true;
				canMoveRight = false;
				canJump = false;
			}
				
			//finding letter hits that don't have corresponding walkable hit
			foreach (Hit lhit in letter) {
				if (!walkable.Any (s => s.colliderCrossPoint == lhit.colliderCrossPoint)) {
					if (lhit.horizontal > 0) {
						canMoveRight = false;
					}
					else if (lhit.horizontal < 0) {
						canMoveLeft = false;
					}
				}
			}
		}
		else {
			side.sharedMaterial = slippery;
			bottom.sharedMaterial = slippery;
			if (letter.Count > 2) {
				//don't know how this could happen
				resetPosition();
			}
			else if (letter.Count == 2 && !letter.Any (x => x.horizontal == 0)) {
				//if hitting both sides but not in the middle (such as in between b)
				Debug.Log ("Both sides");
				resetPosition ();
			}
			else if (letter.Count >= 1) {
				//hitting on just 1 side
				if (letter.Any(x=>x.horizontal < 0)) {
					//if ray is from left raycast
					canMoveLeft = false;
					canMoveRight = true;
					canJump = false;
					return;
				}
				else if (letter.Any(x=>x.horizontal > 0)) {
					//if ray if from right raycast
					canMoveLeft = true;
					canMoveRight = false;
					canJump = false;
					return;
				}
				else {
					//center raycast
					//Debug.Log("Center");
					enableAllMovement ();
				}
			}
			else {
				isJumping = true;
				anim.SetBool ("isJumping", true);

				enableAllMovement ();
				canJump = false;
			}
		}
	}

	void enableAllMovement(){
		canMoveLeft = true;
		canMoveRight = true;
		canJump = true;
	}

	void disableAllMovement(){
		canMoveLeft = false;
		canMoveRight = false;
		canJump = false;
	}

	void resetPosition(){
		rigid.velocity = Vector2.zero;
		this.transform.position = new Vector3 (lastGroundedPosition.x, lastGroundedPosition.y, transform.position.z);

		getPlayerController ().CancelBellows ();
		playerController.CancelThrowable ();
		InSlowMotion = false;
		exitedStringItemState ();
	}

	public void jump(float horizontal = 0){
		exitedStringItemState ();

		float velx = rigid.velocity.x;
		if (getPlayerController ().Climbing) {
			ExitClimb ();
			//float sign = Mathf.Sign (transform.localScale.x);
			//velx = -sign * movespeed;
		}
		else if (stringRigid != null) {
			velx = horizontal * movespeed;
		}
		rigid.velocity = new Vector2 (velx, jumpSpeed);

		jumpUsed = true;
	}

	public Vector2 horizontal_move(float horizontal, bool overrideDamp, bool checkOverride, bool alreadyIncludesMoveSpeed = false){
		if ((horizontal > 0 && !canMoveRight || horizontal < 0 && !canMoveLeft || !canMoveLeft && !canMoveRight) && !checkOverride) {
			return rigid.velocity;
			//horizontal = 0;
			//overrideDamp = true;
		}

		float ms = alreadyIncludesMoveSpeed ? 1 : movespeed;

		if (!horizontal_set) {
			float inc = horizontal * ms;
			if (overrideDamp) {
				float velx = horizontal * ms;
				velocityXSmooth = velx;
				rigid.velocity = new Vector2 (velx, rigid.velocity.y);
			}
			else if (isJumping) {
				//float smoothAmt = Mathf.Abs(horizontal) < 0.1f ? 1.0f : 0.2f; //slow down slower midair
				float smoothAmt = 0.2f;
				float velx = Mathf.SmoothDamp (rigid.velocity.x, horizontal * ms, ref velocityXSmooth, smoothAmt);
				rigid.velocity = new Vector2 (velx, rigid.velocity.y);
			}
			else if (IceMove) {
				float velx = Mathf.SmoothDamp (rigid.velocity.x, horizontal * ms, ref velocityXSmooth, 0.6f);
				rigid.velocity = new Vector2 (velx, rigid.velocity.y);
			}
			else {
				float velx;
				if (Math.Abs (inc) > Mathf.Abs (rigid.velocity.x)) {
					//if we're accelerating
					velx = Mathf.SmoothDamp (rigid.velocity.x, inc, ref velocityXSmooth, 0.05f);
				}
				else {
					velx = Mathf.SmoothDamp (rigid.velocity.x, inc, ref velocityXSmooth, 0.1f);
				}
				rigid.velocity = new Vector2 (velx, rigid.velocity.y);
			}
			anim.SetFloat ("moveSpeed", Mathf.Abs (inc));
			horizontal_set = true;

		}
		return rigid.velocity;
	}

	private void SlopeAdjust(){
		List<Hit> walkablehits = downRays.Where (c => c.raycastHit.collider != null && c.raycastHit.collider.tag == "Walkable").ToList();
		if (!isSliding && !rigid.isKinematic) {
			Vector2 normal;
			if (walkablehits.Any(h => h.horizontal == 0)) {
				if (Mathf.Abs (collisionNormal.y) > 0.01f && collisionNormal.x != 0) {
					Vector2 n = collisionNormal.normalized;
					Vector2 direction = Quaternion.Euler (0, 0, 90) * n * Math.Sign (collisionNormal.x);
					float theta = Mathf.Atan2 (direction.y, direction.x);
					float gslope = (Physics2D.gravity.y * rigid.gravityScale) * Mathf.Sin (theta);
					Vector2 counterVelocity = direction * -gslope * Time.fixedDeltaTime;
					rigid.velocity = rigid.velocity + counterVelocity;
				}
			}
		}
	}
		
	private RaycastHit2D [] raycastDown(){
		Vector2 end = new Vector2 (transform.position.x, transform.position.y - height);
		Debug.DrawLine (transform.position, end,Color.red,2);
		return Physics2D.LinecastAll (transform.position, end, 1 << LayerMask.NameToLayer ("Walkable"));
	}

	private void PerformRaycast(){
		if (!raycasted) {
			if (raycaster == null) {
				InitializeRaycaster ();
			}
			raycaster.RaycastVertical (upRays, downRays, Vector2.zero);
			raycaster.RaycastHorizontal (leftRays, rightRays, Vector2.zero);
		}
		raycasted = true;
	}

	private List<RaycastHit2D> getRayResults(){
		List<RaycastHit2D> rayhits = new List<RaycastHit2D> ();
		eulerAngle = transform.rotation.eulerAngles.z;

		//sidecasts
		if(isSliding){
			if (eulerAngle > 0) {
				rayhits.AddRange(Physics2D.RaycastAll (transform.position, angleToVector (-eulerAngle + 90), width,  1 << LayerMask.NameToLayer ("Walkable"))); //if standing normally, this ray goes right
			} else if (eulerAngle < 0) {
				rayhits.AddRange(Physics2D.RaycastAll (transform.position, angleToVector (-eulerAngle + 270), width, 1 << LayerMask.NameToLayer ("Walkable"))); //if standing normally, this ray goes left
			}
		}

		//downcasts
		//we want rays being 0.1 away from the edge, so (width /2 * .99)
		Vector2 offset = (angleToVector(-eulerAngle+270) * (width/2f) * 0.9f);
		Vector2 downray = angleToVector (-eulerAngle + 180);
		float rayLength = height * 0.55f;
		rayhits.AddRange(Physics2D.RaycastAll((Vector2)transform.position + offset, downray, rayLength));
		rayhits.AddRange(Physics2D.RaycastAll((Vector2)transform.position - offset, downray, rayLength));
		rayhits.AddRange(Physics2D.RaycastAll((Vector2)transform.position, downray, rayLength));
		Debug.DrawLine ((Vector2)transform.position + offset, (Vector2)transform.position + offset - Vector2.up * rayLength, Color.red);
		Debug.DrawLine ((Vector2)transform.position - offset, (Vector2)transform.position - offset - Vector2.up * rayLength, Color.red);
		Debug.DrawLine ((Vector2)transform.position, (Vector2)transform.position - Vector2.up * rayLength, Color.red);

		return rayhits.Where (c => c.collider.tag != "Player").ToList();
	}

	public bool isAirborn(){
		return downRays.Where (s => s.raycastHit.collider.tag.Equals ("Walkable")).ToList ().Count == 0;
	}

	private void LetterEventOccur(Letter letter_event){
		EventHandler<NewLetterEvent> handler = NewLetter;
		if (handler != null) {
			handler (this, new NewLetterEvent (letter_event));
		}
	}

	public void setKinematic(bool isKine){
		//if it's kinematic, it ignores physics
		if (isKine) {
			lastVelocity = rigid.velocity;
			rigid.isKinematic = true;
		} else {
			rigid.isKinematic = false;
		}
	}

	public bool getKinematic(){
		return rigid.isKinematic;
	}

	public void ApplyForce(Vector3 force){
		rigid.AddForce (force * rigid.mass);
	}

	private Vector2 angleToVector(float angle){
		return new Vector2(Mathf.Sin(Mathf.Deg2Rad * angle), Mathf.Cos(Mathf.Deg2Rad * angle));
	}

	private float vectorToAngle(Vector2 vector){
		return Mathf.Atan(vector.y/vector.x) * Mathf.Rad2Deg;
	}

	private PlayerController getPlayerController(){
		if (playerController == null)
			playerController = gameObject.GetComponent<PlayerController> ();
		return playerController;
	}

	public void attachToStringItem(GameObject stringItem){
		if (stringRigid == null) {
			stringRigid = stringItem.GetComponent<Rigidbody2D> ();
			if (stringItem.GetComponent<Umbrella> () != null) {
				rigid.velocity = new Vector2 (rigid.velocity.x, stringRigid.velocity.y);
				stringRigid.velocity = rigid.velocity;
			}
			else if (stringItem.GetComponent<Balloon> () != null) {
				stringRigid.velocity = rigid.velocity;
				getPlayerController().resetJumpCount ();
				canJump = true;

				PerformRaycast ();
				if (downRays.Where (h => h.raycastHit.transform.tag == "Walkable").ToList().Count > 0) {
					canDropFromBalloon = false;
				}
				else {
					canDropFromBalloon = true;
				}
			}

			foreach(Collider2D c in stringItem.GetComponents<Collider2D> ()) {
				if (!c.isTrigger) {
					foreach (Collider2D p in this.GetComponents<Collider2D>()) {
						Physics2D.IgnoreCollision (c, p);
					}
				}
			}

			float xpos = stringItem.transform.position.x;// - Mathf.Sign (transform.localScale.x) * .15f;
			this.transform.position = new Vector3 (xpos, transform.position.y, transform.position.z);

			StringItem stringitem = stringItem.gameObject.GetComponent<StringItem> ();
			stringitem.setAttachedRigidbody (rigid);
			stringitem.setStringGrabOffset (transform.position);

			rigid.gravityScale = 0;

			getPlayerController ().onStringItem = true;

			anim.SetBool ("stringItem", true);
		}
	}

	public void exitedStringItemState(){
		if (stringRigid != null) {
			Balloon b = stringRigid.GetComponent<Balloon> ();
			bool isBalloon = b != null;
			if ( (isBalloon && (canDropFromBalloon || b.isAtTop)) || !isBalloon) {
				if (!isBalloon) {
					horizontal_move (stringRigid.velocity.x, true, false, true);
					stringRigid.gameObject.SetActive (false);
				}

				stringRigid.gameObject.GetComponent<StringItem> ().setAttachedRigidbody (null);
				rigid.gravityScale = 1;
				anim.SetBool ("stringItem", false);

				getPlayerController ().onStringItem = false;

				stringRigid = null;
				//rigid.velocity = Vector2.zero;
			}
		}
	}

	public void updateStringVelocity(){
		if (stringRigid != null) {
			//velocity gets updated by the stringitem
			//resetStringPosition ();
		}
	}

	public void resetStringPosition(){
		if(stringRigid != null){
			stringRigid.gameObject.GetComponent<StringItem> ().resetAttachedPosition ();
			//this.transform.position = new Vector3(stringRigid.transform.position.x, stringRigid.transform.position.y-stringGrabOffset, transform.position.z);
		}
	}

	public void zeroVelocity(){
		rigid.velocity = Vector3.zero;
	}

	public void multiplyGravityScale(float mult){
		rigid.gravityScale *= mult;
	}

	public void setGravityScale(float grav){
		rigid.gravityScale = grav;
	}

	public void setBaseGravity(float grav){
		baseGravity = grav;
		rigid.gravityScale = baseGravity * multiplierGravity;
	}

	public void setMultiplierGravity(float grav){
		multiplierGravity = grav;
		rigid.gravityScale = baseGravity * multiplierGravity;
	}

	public Vector2 getVelocity(){ return rigid.velocity; }

	public void StartSlowMotion(){
		StopCoroutine ("SlowMotion");
		StopCoroutine("ResumeNormalTime");
		StartCoroutine ("SlowMotion");
	}

	public IEnumerator SlowMotion(){
		InSlowMotion = true;
		timeScaleSetting = 0.2f;
		yield return new WaitForSeconds (3f*timeScaleSetting);
		yield return StartCoroutine("ResumeNormalTime");
	}

	public IEnumerator ResumeNormalTime(){
		if (InSlowMotion && !transitioningBacktoNormalTime) {
			float transitionTime = 0.2f;
			transitioningBacktoNormalTime = true;
			//speed back up for 0.5f
			float timeChangeStartTime = Time.time;
			while (Time.time - timeChangeStartTime < transitionTime) {
				float interpTime = (Time.time - timeChangeStartTime) / transitionTime;
				timeScaleSetting = Mathf.Lerp (0.2f, 1f, interpTime);
				yield return new WaitForFixedUpdate ();
			}

			InSlowMotion = false;
			transitioningBacktoNormalTime = false;
		}
	}

	public void CancelSlowMotion(){
		StopCoroutine ("SlowMotion");
		StartCoroutine ("ResumeNormalTime");
	}

	public bool CheckIfSquished(Collision2D col){
		if (col.collider.tag == "Letter" || col.collider.tag == "Walkable" || col.collider.tag == "Book") {
			if (col.contacts.Any (c => Mathf.Abs (c.normal.y) >= .99f)) {
				Hit upRay = upRays.Where (r => r.horizontal == 0).OrderBy (r => r.distance).FirstOrDefault (); 
				Hit downRay = downRays.Where (r => r.horizontal == 0).OrderBy (r => r.distance).FirstOrDefault (); 

				//1000 is an arbitrary large number
				float max = upRay != null ? upRay.raycastHit.point.y : 1000;
				float min = downRay != null ? downRay.raycastHit.point.y : -1000;
				float playerHeight = (2 * bottom.radius + side.size.y) * transform.lossyScale.y;

				if (max - min < playerHeight) {
					return true;
				}

				//there's a small bug here where the platform doesn't have enough room to fit the player and the collision won't squish them (pushes them off to the side).  They bounce around a bit before finding the final position.
			}
			else if (col.contacts.Any (c => Mathf.Abs (c.normal.x) >= .99f)) {
				Hit leftRay = leftRays.Where (r => r.vertical == 0).OrderBy (r => r.distance).FirstOrDefault (); 
				Hit rightRay = rightRays.Where (r => r.vertical == 0).OrderBy (r => r.distance).FirstOrDefault (); 

				//1000 is an arbitrary large number
				float max = rightRay != null ? rightRay.raycastHit.point.x : 1000;
				float min = leftRay != null ? leftRay.raycastHit.point.x : -1000;
				float playerWidth = side.size.x * transform.lossyScale.x;

				if (max - min < playerWidth) {
					return true;
				}
			}
		}
		return false;
	}

	public void SetGrappleDirection(Vector3 grapple_location, List<RopeSegment> ropes){
		if (!rigid.isKinematic) {
			
			StopGrapple (); //destroy existing ropes
			getPlayerController ().CancelBellows ();
			getPlayerController ().CancelThrowable ();

			Vector3 localScale = transform.localScale;
			localScale.x = Mathf.Abs (localScale.x) * Mathf.Sign (grapple_location.x - transform.position.x);
			transform.localScale = localScale;

			Grappling = true;
			grappleStartPoint = this.transform.position;
			grappleEndPoint = grapple_location;
			grappleVector = (grappleEndPoint - grappleStartPoint).normalized;
			throwableRope = ropes;
			setGravityScale (0);
		}
	}

	public void StopGrapple(){
		Grappling = false;
		foreach (RopeSegment r in throwableRope) {
			Destroy (r.gameObject);
		}
		throwableRope.Clear ();
		setGravityScale (1);
	}

	public void SetYank(Letter effector, GameObject tobject, List<RopeSegment> ropes){
		if (this.collisionLetter != effector) {
			getPlayerController ().CancelBellows ();
			getPlayerController ().CancelThrowable ();
			StartCoroutine (Yank (effector, tobject, ropes));
		}
		else {
			for (int i = 0; i < ropes.Count; i++) {
				Destroy (ropes [i].gameObject);
			}
			getPlayerController ().playerHasControl = true;
		}
	}

	public IEnumerator Yank(Letter effector, GameObject tobject, List<RopeSegment> ropes){
		Vector2 position = tobject.transform.position;

		Vector3 localScale = transform.localScale;
		localScale.x = Mathf.Abs (localScale.x) * Mathf.Sign (tobject.transform.position.x - transform.position.x);
		transform.localScale = localScale;

		rigid.velocity = Vector2.zero;
		getPlayerController ().setPlayerActive (false);

		//pull animation
		anim.SetBool("yanking",true);

		yield return new WaitForSeconds (1f);

		anim.SetBool ("yanking", false);

		int d = Math.Sign (this.transform.position.x - position.x);
		effector.SetMoveAmount (Quaternion.Euler (0, 0, -effector.transform.localRotation.eulerAngles.z) * new Vector3 (d * 0.02f, 0, 0));
		for (int i = 0; i < ropes.Count; i++) {
			Destroy (ropes [i].gameObject);
		}

		getPlayerController ().setPlayerActive (true);
	}

	public void SetClimbing(){
		PerformRaycast ();
		List<Hit> leftFiltered = leftRays.Where (r => r.raycastHit.normal.y <= 0.1 && r.vertical == 0 && Mathf.Abs(r.colliderCrossPoint.x - r.raycastHit.point.x) < 0.1f).ToList();
		List<Hit> rightFiltered = rightRays.Where (r => r.raycastHit.normal.y <= 0.1 && r.vertical == 0 && Mathf.Abs(r.colliderCrossPoint.x - r.raycastHit.point.x) < 0.1f).ToList();

		Vector2 translateDistance;
		if (leftFiltered.Count >= 3 && transform.localScale.x < 0) {
			Hit closest = leftFiltered.OrderBy (r => r.distance).FirstOrDefault();
			translateDistance = closest.raycastHit.point - closest.colliderCrossPoint;
			EnterClimb (translateDistance);
		}
		else if (rightFiltered.Count >= 3 && transform.localScale.x > 0) {
			Hit closest = rightFiltered.OrderBy (r => r.distance).FirstOrDefault();
			translateDistance = (closest.raycastHit.point - closest.colliderCrossPoint) * 0.9f;
			EnterClimb (translateDistance);
		}
	}

	private void EnterClimb(Vector2 translateDistance){
		rigid.isKinematic = true;
		getPlayerController ().Climbing = true;
		getPlayerController ().resetJumpCount ();
		anim.SetBool ("climbing", true);
	}

	public void ExitClimb(){
		if (getPlayerController ().Climbing) {
			rigid.isKinematic = false;
			getPlayerController ().Climbing = false;
			anim.SetBool ("climbing", false);
		}
	}

	public void CheckForCollisions(){
		//stopping item usage if landing while in use
		if (downRays.Count > 0) {
			if(!collidingWithLetter){
				//stop slow motion
				//CancelSlowMotion ();
				InSlowMotion = false;

				//stop using throwable
				getPlayerController().CancelThrowable();
				//stop using bellows
				playerController.CancelBellows();
			}
		}

		List<Hit> walkablehits = downRays.Where (s => s.raycastHit.collider.tag.Equals ("Walkable")).ToList();
		if (walkablehits.Count > 0) {
			Hit belowhit = walkablehits.Where (s => s.horizontal == 0).FirstOrDefault();
			Letter newLetter = walkablehits[0].raycastHit.collider.GetComponent<Letter> () ??  walkablehits[0].raycastHit.collider.transform.parent.gameObject.GetComponent<Letter> ();
			if (newLetter != null) {
				LetterEventOccur (newLetter);
				if (!collidingWithLetter) {
					if(walkablehits.Count == 1 && walkablehits.Any(s => s.horizontal != 0) && isJumping){
						adjusting_position = true;
					}

					getPlayerController ().playAudio (0);

					anim.SetBool ("isJumping", false);
					isJumping = false;

					ExitClimb ();

					float velx = previousFrameVelocity.x / movespeed;
					rigid.velocity = Vector2.zero;
					horizontal_move (velx, true, true);

					exitedStringItemState ();
				}
				collisionLetter = newLetter;
				collidingWithLetter = true;
				letterEvent = false;
			}
		}
		//clouds or other temporary walkables
		else {
			walkablehits = downRays.Where (s => s.raycastHit.collider.tag.Equals ("TempWalkable")).ToList();
			if (walkablehits.Count > 0) {
				Hit belowhit = walkablehits.Where (s => s.horizontal == 0).FirstOrDefault();
				playerController = getPlayerController ();
				playerController.playAudio (0);
				anim.SetBool ("isJumping", false);

				playerController.resetJumpCount ();
				playerController.resetItemAvailability (3);

				exitedStringItemState ();
			}
		}
	}

	public bool PointInCollider(Vector3 point){
		return side.bounds.Contains (point);
	}

	/***************************************/
	/********* COLLISION HANDLING **********/
	/***************************************/
	public void OnCollisionEnter2D(Collision2D col){		
		PerformRaycast ();

		if (CheckIfSquished (col)) {
			GameManager.ResetLevel ();
		}

		if (Grappling) {
			StopGrapple ();
		}

		CheckForCollisions ();

		exitEvent = false;
		collisionNormal = col.contacts [0].normal;

		List<Hit> leftrighthits = new List<Hit>(leftRays);
		leftrighthits.AddRange (rightRays);
		leftrighthits = leftrighthits.Where (s => (s.raycastHit.collider.tag == "Walkable" || s.raycastHit.collider.tag == "TempWalkable" || s.raycastHit.collider.tag == "Letter" || s.raycastHit.collider.tag == "Book")).ToList ();
		if (leftrighthits.Count > 0) {
			Letter newLetter = leftrighthits[0].raycastHit.collider.GetComponent<Letter> ();
			Vector3 moveAmount = newLetter != null ? newLetter.MoveAmount : Vector3.zero;
			if (stringRigid != null) {
				bool isBalloon = stringRigid.GetComponent<Balloon> ();

				stringRigid.velocity = new Vector2 (moveAmount.x, stringRigid.velocity.y);
			}
		}

		//if we hit the top of something on our jump
		if (rigid.velocity.y > 0 && col.contacts.Any (c => c.normal.y < 0)) {
			//rigid.velocity = new Vector2 (previousFrameVelocity.x, 0);
			//getPlayerController().jumpDown = false;
		}
	}

	public void OnCollisionExit2D(Collision2D col){
		//need to add a check to see if the player walked off a walkable on to a slidetime
		if (col.collider.tag == "Walkable" || col.collider.tag == "TempWalkable") {
			exitEvent = true;
			collisionNormal = Vector2.zero;
		}
	}

	public void OnCollisionStay2D(Collision2D col){
		if (col.collider.tag == "Walkable" || col.collider.tag == "TempWalkable") {
			collisionNormal = col.contacts [0].normal;

			PerformRaycast ();
			List<Hit> walkablehits = downRays.Where (s => s.raycastHit.collider.tag.Equals ("Walkable")).ToList();
			if (walkablehits.Count == 1 && walkablehits.Any (s => s.horizontal != 0) && adjusting_position) {
				float walkabledirection = walkablehits.FirstOrDefault ().horizontal;
				Vector3 forcedirection = IceMove ? Vector3.down : Vector3.right;
				ApplyForce (forcedirection * 20 * walkabledirection);
			}
			else {
				adjusting_position = false;
				if (walkablehits.Count >= 1) {
					if (!collidingWithLetter) {
						Letter newLetter = walkablehits [0].raycastHit.collider.GetComponent<Letter> () ?? walkablehits [0].raycastHit.collider.transform.parent.gameObject.GetComponent<Letter> ();
						if (newLetter != null) {
							LetterEventOccur (newLetter);
						}

						playerController = getPlayerController ();
						playerController.resetJumpCount ();
						playerController.resetItemAvailability (3);
					}
				}
			}
		}
	}

	//
	void OnTriggerEnter2D(Collider2D other) {
		//balloon or umbrella
		if (other.tag == "StringItem") {
			attachToStringItem (other.gameObject);
		}
	}

	void OnTriggerExit2D(Collider2D other) {
		if (other.tag == "StringItem") {
			exitedStringItemState ();
			StartCoroutine(disableCollisionsForTime(1f, other.gameObject));
			//other.GetComponent<StringItem> ().EnableCollisionAfterTime (0.1f, this.gameObject);
		}
	}

	IEnumerator disableCollisionsForTime(float time, GameObject other){
		if (other == null)
			yield break;
		
		foreach(Collider2D c in other.GetComponents<Collider2D> ()) {
			foreach (Collider2D p in this.GetComponents<Collider2D>()) {
				Physics2D.IgnoreCollision (c, p, true);
			}
		}

		yield return new WaitForSeconds (time);

		if (other == null)
			yield break;

		foreach(Collider2D c in other.GetComponents<Collider2D> ()) {
			foreach (Collider2D p in this.GetComponents<Collider2D>()) {
				Physics2D.IgnoreCollision (c, p, false);
			}
		}

		yield return null;
	}

	void OnDestroy(){
		StopAllCoroutines ();
	}
}
