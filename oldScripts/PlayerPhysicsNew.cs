using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class PlayerPhysicsNew: MonoBehaviour {

	private BoxCollider2D side;
	private CircleCollider2D bottom;
	private CircleCollider2D top;

	private PhysicsMaterial2D slippery;
	private PhysicsMaterial2D friction;

	private float height, width;

	public event EventHandler<NewLetterEvent> NewLetter;

	private float gravity;
	private float jumpTime;
	[SerializeField]
	public float maxMovespeed;
	[SerializeField]
	private float jumpSpeed;

	private float collisionErrorFactor = 0.01f;

	public Vector2 Inputs { get; set;}
	private Vector2 velocity;
	private float velocityXSmooth;
	private Vector2 lastFrameVelocity;
	public CollisionDirections Collisions { get; private set; }

	private bool deflected = false;

	[SerializeField]
	private LayerMask rayMask;

	[SerializeField]
	private GameObject boundingObject;
	private Bounds boundingbox;

	/*Constants*/
	float adjustedRadius;
	float sideCircleRayLength;
	float bottomCircleRayLength;

	float adjustedWidth;
	float sideRayLength;
	float sideRaySpacing;

	private PlayerControllerNew playerController;

	public class CollisionDirections
	{
		public bool above;
		public bool below;
		public bool right;
		public bool left;

		public CollisionDirections(){
			above = below = right = left = false;
		}
	}

	void Awake(){
		side = GetComponent<BoxCollider2D> ();
		CircleCollider2D[] circles = GetComponents<CircleCollider2D> ();
		if (circles [0].offset.y > 0) {
			top = circles [0];
			bottom = circles [1];
		}
		else {
			top = circles [1];
			bottom = circles [0];
		}

		slippery = Resources.Load ("Materials/Physics/SlipperyPlayer") as PhysicsMaterial2D;
		friction = Resources.Load ("Materials/Physics/FrictionPlayer") as PhysicsMaterial2D;

		height = Mathf.Abs (bottom.bounds.min.y - side.bounds.max.y);
		width = Mathf.Abs (side.bounds.min.x - side.bounds.max.x);

		//get player bounds
		boundingbox = boundingObject.GetComponent<SpriteRenderer>().bounds;

		//new stuff
		velocity = Vector2.zero;
		gravity = -1;
		maxMovespeed = 4f;
		jumpSpeed = 2;

		adjustedRadius = bottom.radius*transform.localScale.y;
		sideCircleRayLength = adjustedRadius * 1.41f; //sqrt(2) * x
		bottomCircleRayLength = adjustedRadius * 1.1f;

		adjustedWidth = side.bounds.extents.x * transform.localScale.y;
		sideRayLength = adjustedWidth * 0.8f;
		sideRaySpacing = side.bounds.size.y / 3f;

		Collisions = new CollisionDirections ();
	}

	void Start () {


	}

	public void Move(Vector2 inputs){
		//set initial velocity, will be modified with collions
		float targetVelocity = inputs.x * maxMovespeed * Time.deltaTime;
		float xvelocity = Mathf.SmoothDamp (velocity.x, targetVelocity, ref velocityXSmooth, Collisions.below ? 0.1f : 0.2f);
		//xvelocity = 0.02f;
		velocity.x = 0;

		Vector2 hvelocity = new Vector2 (xvelocity, 0);
		HorizontalRayCast (ref hvelocity);
		transform.Translate (hvelocity);

		velocity.y += gravity * Time.deltaTime;
		VerticalRaycast (ref velocity);
		transform.Translate (velocity);

		velocity = hvelocity + velocity;
	}

	private void VerticalRaycast(ref Vector2 velocity){

		//returns 1 if 0 or positive
		int directionY = (int)Mathf.Sign (velocity.y);

		Vector2 bottomCenter = new Vector2(transform.position.x, transform.position.y + (bottom.offset.y * transform.localScale.y));
		Vector2 topCenter = new Vector2(transform.position.x, transform.position.y + (top.offset.y * transform.localScale.y));

		List<Hit> downRays = new List<Hit>();
		List<Hit> upRays = new List<Hit>();

		Vector2 start = bottomCenter;
		for (float i = -1; i <= 1; i++) {
			//for bottom, we need to be concerned with walkable surfaces so we use raycastAll instead of raycast
			Vector2 drawNormal = Vector3.Normalize (new Vector2 (i, -1));

			if (i == 0) {
				RaycastHit2D[] allray = Physics2D.RaycastAll (start, drawNormal, bottomCircleRayLength + Mathf.Abs(velocity.y), rayMask);
				Debug.DrawLine (start, start + drawNormal * (bottomCircleRayLength + Mathf.Abs(velocity.y)), Color.blue);
				foreach(RaycastHit2D ray in allray){
					downRays.Add (new Hit (ray, -1, i, start + drawNormal * adjustedRadius));
				}
			}
			else {
				RaycastHit2D[] allray = Physics2D.RaycastAll (start, drawNormal, sideCircleRayLength, rayMask);
				Debug.DrawLine (start, start + drawNormal * sideCircleRayLength, Color.blue);
				if (allray.Length > 0) {
					foreach (RaycastHit2D ray in allray) {
						downRays.Add (new Hit (ray, -1, i, start + drawNormal * adjustedRadius));
					}
				}
				else if (velocity.y < 0) {
					Vector2 rayStart = start + drawNormal * sideCircleRayLength;
					allray = Physics2D.RaycastAll (rayStart, Vector2.up, velocity.y, rayMask);
					Debug.DrawLine (rayStart, rayStart + Vector2.up * velocity.y, Color.blue);
					foreach (RaycastHit2D ray in allray) {
						downRays.Add (new Hit (ray, -1, i, start + drawNormal * adjustedRadius, Vector2.Distance(start, ray.point)));
					}
				}
			}
		}

		//top
		start = topCenter;
		for (int i = -1; i <= 1; i++) {
			Vector2 drawNormal = Vector3.Normalize (new Vector2 (i, 1));

			if (i == 0) {
				RaycastHit2D ray = Physics2D.Raycast (start, drawNormal, bottomCircleRayLength + Mathf.Abs(velocity.y), rayMask);
				Debug.DrawLine (start, start + drawNormal * (bottomCircleRayLength + Mathf.Abs(velocity.y)), Color.blue);
				if (ray.collider != null) {
					upRays.Add (new Hit (ray, 1, i, start + drawNormal * adjustedRadius));
				}
			}
			else {
				RaycastHit2D ray = Physics2D.Raycast (start, drawNormal, sideCircleRayLength, rayMask);
				Debug.DrawLine (start, start + drawNormal * sideCircleRayLength, Color.blue);
				if (ray.collider != null) {
					upRays.Add (new Hit (ray, 1, i, start + drawNormal * adjustedRadius));
				}
				else if (velocity.y > 0) {
					Vector2 rayStart = start + drawNormal * sideCircleRayLength;
					ray = Physics2D.Raycast (rayStart, Vector2.up, Mathf.Abs(velocity.y), rayMask);
					Debug.DrawLine (rayStart, rayStart + Vector2.up * Mathf.Abs(velocity.y), Color.blue);
					if (ray.collider != null) {
						upRays.Add (new Hit (ray, 1, i, start + drawNormal * adjustedRadius, Vector2.Distance(start, ray.point)));
					}
				}
			}
		}

		foreach (Hit hit in downRays) {
			Debug.DrawLine (hit.colliderCrossPoint, hit.raycastHit.point, Color.cyan);
		}

		//do stuff with the rays
		if (downRays.Count > 0) {
			Collisions.below = true;
			downRays = downRays.OrderBy (s => s.distance).ToList ();

			if (deflected) {
				float smax = Mathf.Sign (downRays.Max (s => s.raycastHit.normal.x));
				float smin = Mathf.Sign (downRays.Min (s => s.raycastHit.normal.x));

				if (smax > 0) {
					Vector2 newNormal = downRays.Find (s => s.raycastHit.point.x == downRays.Max (ss => ss.raycastHit.point.x)).raycastHit.normal;
					Vector2 velocity_angle = ((Quaternion.Euler (0, 0, -90) * newNormal).normalized);
					velocity = Mathf.Sqrt (Vector2.SqrMagnitude (lastFrameVelocity)) * velocity_angle;
				}
				else {
					Vector2 newNormal = downRays.Find (s => s.raycastHit.point.x == downRays.Min (ss => ss.raycastHit.point.x)).raycastHit.normal;
					Vector2 velocity_angle = ((Quaternion.Euler (0, 0, 90) * newNormal).normalized);
					velocity = Mathf.Sqrt (Vector2.SqrMagnitude (lastFrameVelocity)) * velocity_angle;
				}

				//we want to capture the movement amount during the slide to preserve speed
				lastFrameVelocity = velocity;
			}
			else {
				List<Hit> walkableRays = downRays.Where (h => h.raycastHit.collider.tag == "Walkable").ToList ();

				//we do this here because if we have a transfer falling velocity to a potential slide. The y velocity value changes to a smaller number in the downRays > 0 section.
				lastFrameVelocity = velocity;

				velocity.y = directionY * Mathf.Abs (downRays.First ().colliderCrossPoint.y - downRays.First ().raycastHit.point.y);
				velocity.y = Mathf.Abs (velocity.y) > collisionErrorFactor ? velocity.y + collisionErrorFactor : 0; //this is to prevent the letters getting pushed down if the player is on it

				if (walkableRays.Count > 0) {
					//send event

					deflected = false;
				}
				else{ //!deflected
					float smax = Mathf.Sign (downRays.Max (s => s.raycastHit.normal.x));
					float smin = Mathf.Sign (downRays.Min (s => s.raycastHit.normal.x));
					//on the same slope
					if (smax == smin && smax > 0) {
						deflected = true;
						velocity.x = 0;
					}
					else if (smax == smin && smax < 0) {
						deflected = true;
						velocity.x = 0;
					}
					//in between two slopes
					else {

					}
				}
			}
		}
		else {
			deflected = false;
			Collisions.below = false;
			lastFrameVelocity = velocity;
		}

		if (upRays.Count > 0) {
			Collisions.above = true;

			upRays = upRays.OrderBy (s => s.distance).ToList ();

			velocity.y = directionY * Mathf.Abs (upRays.First ().colliderCrossPoint.y - upRays.First ().raycastHit.point.y);
			velocity.y = Mathf.Abs (velocity.y) > collisionErrorFactor ? velocity.y - collisionErrorFactor : 0; //this is to prevent the letters getting pushed up if the player is on it
		}
		else {
			Collisions.above = false;
		}
	}

	private void HorizontalRayCast(ref Vector2 hvelocity){

		//returns 1 if 0 or positive
		int directionX = (int)Mathf.Sign (hvelocity.x);

		Vector2 bottomCenter = new Vector2(transform.position.x, transform.position.y + (bottom.offset.y * transform.localScale.y));
		Vector2 topCenter = new Vector2(transform.position.x, transform.position.y + (top.offset.y * transform.localScale.y));

		List<Hit> rightRays = new List<Hit>();
		List<Hit> leftRays = new List<Hit>();

		//bottom
		//only want sides
		Vector2 start = bottomCenter;
		for (float i = -1; i <= 1; i+=2) {
			//for bottom, we need to be concerned with walkable surfaces so we use raycastAll instead of raycast
			Vector2 drawNormal = Vector3.Normalize (new Vector2 (i, -1));

			RaycastHit2D[] allray = Physics2D.RaycastAll (start, drawNormal, sideCircleRayLength, rayMask);
			Debug.DrawLine (start, start + drawNormal * sideCircleRayLength, Color.magenta);
			if (allray.Length > 0) {
				foreach (RaycastHit2D ray in allray) {
					if (i < 0) {
						leftRays.Add (new Hit (ray, -1, i, start + drawNormal * adjustedRadius));
					}
					else {
						rightRays.Add (new Hit (ray, -1, i, start + drawNormal * adjustedRadius));
					}
				}
			}
			else if (Mathf.Sign(hvelocity.x) == i) {
				Vector2 rayStart = start + drawNormal * sideCircleRayLength;
				allray = Physics2D.RaycastAll (rayStart, Vector2.right, hvelocity.x, rayMask);
				Debug.DrawLine (rayStart, rayStart + Vector2.right * hvelocity.x, Color.magenta);
				foreach (RaycastHit2D ray in allray) {
					if (i < 0) {
						leftRays.Add (new Hit (ray, -1, i, start + drawNormal * adjustedRadius, Vector2.Distance (start, ray.point)));
					}
					else {
						rightRays.Add (new Hit (ray, -1, i, start + drawNormal * adjustedRadius, Vector2.Distance (start, ray.point)));
					}
				}
			}

		}

		//top
		start = topCenter;
		for (int i = -1; i <= 1; i+=2) {
			Vector2 drawNormal = Vector3.Normalize (new Vector2 (i, 1));

			RaycastHit2D[] allray = Physics2D.RaycastAll (start, drawNormal, sideCircleRayLength, rayMask);
			Debug.DrawLine (start, start + drawNormal * sideCircleRayLength, Color.magenta);
			if (allray.Length > 0) {
				foreach (RaycastHit2D ray in allray) {
					if (i < 0) {
						leftRays.Add (new Hit (ray, 1, i, start + drawNormal * adjustedRadius));
					}
					else {
						rightRays.Add (new Hit (ray, 1, i, start + drawNormal * adjustedRadius));
					}
				}
			}
			else if (Mathf.Sign(hvelocity.x) == i) {
				Vector2 rayStart = start + drawNormal * sideCircleRayLength;
				allray = Physics2D.RaycastAll (rayStart, Vector2.right, hvelocity.x, rayMask);
				Debug.DrawLine (rayStart, rayStart + Vector2.right * hvelocity.x, Color.magenta);
				foreach (RaycastHit2D ray in allray) {
					if (i < 0) {
						leftRays.Add (new Hit (ray, 1, i, start + drawNormal * adjustedRadius, Vector2.Distance (start, ray.point)));
					}
					else {
						rightRays.Add (new Hit (ray, 1, i, start + drawNormal * adjustedRadius, Vector2.Distance (start, ray.point)));
					}
				}
			}

		}

		//middle
		for (float y = bottomCenter.y; y < topCenter.y+sideRaySpacing; y += sideRaySpacing) {
			float clamped_y = Mathf.Clamp (y, bottomCenter.y, topCenter.y);
			start = new Vector2 (transform.position.x, clamped_y);

			//left
			float velocity_addition = hvelocity.x <= 0 ? hvelocity.x : 0;
			RaycastHit2D ray = Physics2D.Raycast (start, Vector2.left, sideRayLength - velocity_addition, rayMask);
			Debug.DrawLine (start, start + Vector2.left * (sideRayLength - velocity_addition), Color.blue);
			if (ray.collider != null) {
				leftRays.Add (new Hit (ray, 0, -1, start + Vector2.left * side.bounds.extents.x));
			}

			//right
			velocity_addition = hvelocity.x >= 0 ? hvelocity.x : 0;
			ray = Physics2D.Raycast (start, Vector2.right, sideRayLength + velocity_addition, rayMask);
			Debug.DrawLine (start, start + Vector2.right * (sideRayLength + velocity_addition), Color.blue);
			if (ray.collider != null) {
				rightRays.Add (new Hit (ray, 0, 1, start + Vector2.right * side.bounds.extents.x));
			}
		}
			

		//do stuff with rays
		if (hvelocity.x != 0 && !deflected) {
			if (leftRays.Count > 0) {
				leftRays = leftRays.OrderBy (s => s.distance).ToList ();
				Collisions.left = true;

				if (!HandleSlope (leftRays, directionX, ref hvelocity, transform.position.x - sideRayLength)) {
					hvelocity.x = Mathf.Max (directionX * (leftRays.First ().colliderCrossPoint.x - leftRays.First ().raycastHit.point.x), hvelocity.x); //max because negative numbers
				}
			}
			else {
				Collisions.left = false;
			}

			if (rightRays.Count > 0) {
				rightRays = rightRays.OrderBy (s => s.distance).ToList ();
				Collisions.right = true;

				if (!HandleSlope (rightRays, directionX, ref hvelocity, transform.position.x + sideRayLength)) {
					hvelocity.x = Mathf.Min (directionX * (rightRays.First ().raycastHit.point.x - rightRays.First ().colliderCrossPoint.x), hvelocity.x);
				}
			}
			else {
				Collisions.right = false;
			}
		}
		else {
			hvelocity = Vector2.zero;
		}
	}

	private bool HandleSlope(List<Hit> rays, int direction, ref Vector2 slopevelocity, float boundaryX){
		//return if we are walking on a slope
		float incomingVelocity = Mathf.Abs(slopevelocity.x);

		//find if the bottom hit collided with a slope
		Hit walkablehit = rays.Where (h => h.raycastHit.collider.tag == "Walkable" &&  h.vertical < 0).FirstOrDefault();
		Hit closestNonWalkable = rays.Where (h => h.raycastHit.collider.tag != "Walkable" && h.vertical >= 0).OrderBy (h => h.distance).FirstOrDefault();
		if (walkablehit != null) {
			float walkableHitDistance;
			if (direction < 0) {
				walkableHitDistance = boundaryX <= walkablehit.raycastHit.point.x ? 0 : Mathf.Abs (walkablehit.raycastHit.point.x - boundaryX); 
			}
			else {
				walkableHitDistance = boundaryX >= walkablehit.raycastHit.point.x ? 0 : Mathf.Abs (walkablehit.raycastHit.point.x - boundaryX);  
			}
			float nonWalkableHitDistance = closestNonWalkable != null ? Mathf.Abs (closestNonWalkable.colliderCrossPoint.x - closestNonWalkable.raycastHit.point.x) : incomingVelocity;

			slopevelocity.x = nonWalkableHitDistance * direction;

			float angle = Vector2.Angle (Vector2.up, walkablehit.raycastHit.normal);
			float directionY = Mathf.Sign (slopevelocity.x) != Mathf.Sign(walkablehit.raycastHit.normal.x) ? 1 : -1;  // are we climbing or descending
			if (incomingVelocity > walkableHitDistance) {
				slopevelocity.y = (Mathf.Abs(slopevelocity.x) - walkableHitDistance) * Mathf.Tan (angle * Mathf.Deg2Rad) * directionY;
			}
			return true;
		}
		return false;
	}

	private void Raycast(List<Hit> rays){
		Vector2 bottomCenter = new Vector2(transform.position.x, transform.position.y + (bottom.offset.y * transform.localScale.y));
		Vector2 topCenter = new Vector2(transform.position.x, transform.position.y + (top.offset.y * transform.localScale.y));

		//bottom
		Vector2 start = bottomCenter;
		for (float i = -1; i <= 1; i++) {
			//for bottom, we need to be concerned with walkable surfaces so we use raycastAll instead of raycast
			Vector2 drawNormal = Vector3.Normalize (new Vector2 (i, -1));

			if (i == 0) {
				RaycastHit2D[] allray = Physics2D.RaycastAll (start, drawNormal, bottomCircleRayLength + Mathf.Abs(velocity.y), rayMask);
				Debug.DrawLine (start, start + drawNormal * (bottomCircleRayLength + Mathf.Abs(velocity.y)), Color.blue);
				foreach(RaycastHit2D ray in allray){
					rays.Add (new Hit (ray, -1, i, start + drawNormal * adjustedRadius));
				}
			}
			else {
				RaycastHit2D[] allray = Physics2D.RaycastAll (start, drawNormal, sideCircleRayLength, rayMask);
				Debug.DrawLine (start, start + drawNormal * sideCircleRayLength, Color.blue);
				if (allray.Length > 0) {
					foreach (RaycastHit2D ray in allray) {
						rays.Add (new Hit (ray, -1, i, start + drawNormal * adjustedRadius));
					}
				}
				else if (velocity.y < 0) {
					Vector2 rayStart = start + drawNormal * sideCircleRayLength;
					allray = Physics2D.RaycastAll (rayStart, Vector2.up, velocity.y, rayMask);
					Debug.DrawLine (rayStart, rayStart + Vector2.up * velocity.y, Color.blue);
					foreach (RaycastHit2D ray in allray) {
						rays.Add (new Hit (ray, -1, i, start + drawNormal * adjustedRadius, Vector2.Distance(start, ray.point)));
					}
				}
			}
		}
			
		//top
		start = topCenter;
		for (int i = -1; i <= 1; i++) {
			Vector2 drawNormal = Vector3.Normalize (new Vector2 (i, 1));

			if (i == 0) {
				RaycastHit2D ray = Physics2D.Raycast (start, drawNormal, bottomCircleRayLength + Mathf.Abs(velocity.y), rayMask);
				Debug.DrawLine (start, start + drawNormal * (bottomCircleRayLength + Mathf.Abs(velocity.y)), Color.blue);
				if (ray.collider != null) {
					rays.Add (new Hit (ray, 1, i, start + drawNormal * adjustedRadius));
				}
			}
			else {
				RaycastHit2D ray = Physics2D.Raycast (start, drawNormal, sideCircleRayLength, rayMask);
				Debug.DrawLine (start, start + drawNormal * sideCircleRayLength, Color.blue);
				if (ray.collider != null) {
					rays.Add (new Hit (ray, 1, i, start + drawNormal * adjustedRadius));
				}
				else if (velocity.y > 0) {
					Vector2 rayStart = start + drawNormal * sideCircleRayLength;
					ray = Physics2D.Raycast (rayStart, Vector2.up, Mathf.Abs(velocity.y), rayMask);
					Debug.DrawLine (rayStart, rayStart + Vector2.up * Mathf.Abs(velocity.y), Color.blue);
					if (ray.collider != null) {
						rays.Add (new Hit (ray, 1, i, start + drawNormal * adjustedRadius, Vector2.Distance(start, ray.point)));
					}
				}
			}
		}

		//middle
		for (float y = bottomCenter.y; y < topCenter.y+sideRaySpacing; y += sideRaySpacing) {
			float clamped_y = Mathf.Clamp (y, bottomCenter.y, topCenter.y);
			start = new Vector2 (transform.position.x, clamped_y);

			//left
			float velocity_addition = velocity.x <= 0 ? velocity.x : 0;
			RaycastHit2D ray = Physics2D.Raycast (start, Vector2.left, sideRayLength - velocity_addition, rayMask);
			Debug.DrawLine (start, start + Vector2.left * (sideRayLength - velocity_addition), Color.blue);
			if (ray.collider != null) {
				rays.Add (new Hit (ray, 0, -1, start + Vector2.left * side.bounds.extents.x));
			}

			//right
			velocity_addition = velocity.x >= 0 ? velocity.x : 0;
			ray = Physics2D.Raycast (start, Vector2.right, sideRayLength + velocity_addition, rayMask);
			Debug.DrawLine (start, start + Vector2.right * (sideRayLength + velocity_addition), Color.blue);
			if (ray.collider != null) {
				rays.Add (new Hit (ray, 0, 1, start + Vector2.right * side.bounds.extents.x));
			}
		}
	}

	private void LetterEventOccur(Letter letter_event){
		EventHandler<NewLetterEvent> handler = NewLetter;
		if (handler != null) {
			handler (this, new NewLetterEvent (letter_event));
		}
	}

	/***************************************/
	/********* COLLISION HANDLING **********/
	/***************************************/
	public void OnCollisionEnter2D(Collision2D col){		

	}

	public void OnCollisionExit2D(Collision2D col){
		//need to add a check to see if the player walked off a walkable on to a slidetime

	}

	//
	void OnTriggerEnter2D(Collider2D other) {
		//balloon or umbrella
		if (other.tag == "StringItem") {

		}
	}

	void OnTriggerExit2D(Collider2D other) {
		if (other.tag == "StringItem") {

		}
	}

}
