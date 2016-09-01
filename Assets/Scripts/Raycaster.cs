using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Raycaster {

	private GameObject raycastObject;
	private BoxCollider2D side;
	private CircleCollider2D bottom;
	private CircleCollider2D top;

	private float collisionErrorFactor;

	public LayerMask RayMask { get; set; }

	/*Constants*/
	float adjustedRadius;
	float sideCircleRayLength;
	float bottomCircleRayLength;

	float adjustedWidth;
	float sideRayLength;
	float sideRaySpacing;

	public Raycaster(GameObject _raycastObject, LayerMask mask){
		raycastObject = _raycastObject;
		collisionErrorFactor = 0.01f;

		side = raycastObject.GetComponent<BoxCollider2D> ();
		CircleCollider2D[] circles = raycastObject.GetComponents<CircleCollider2D> ();
		if (circles [0].offset.y > 0) {
			top = circles [0];
			bottom = circles [1];
		}
		else {
			top = circles [1];
			bottom = circles [0];
		}

		//height = Mathf.Abs (bottom.bounds.min.y - side.bounds.max.y);
		//width = Mathf.Abs (side.bounds.min.x - side.bounds.max.x);

		adjustedRadius = bottom.radius*raycastObject.transform.localScale.y;
		bottomCircleRayLength = adjustedRadius * 1.41f;
		sideCircleRayLength = bottomCircleRayLength * 0.9f;

		adjustedWidth = side.bounds.extents.x * raycastObject.transform.localScale.y;
		sideRayLength = adjustedWidth * 0.8f;
		sideRaySpacing = side.bounds.size.y / 3f;

		RayMask = mask;
	}

	public void RaycastVertical(List<Hit> upRays, List<Hit> downRays, Vector2 velocity){
		//if velocity vector is specified, we are doing predictions
		//returns 1 if 0 or positive
		int directionY = (int)Mathf.Sign (velocity.y);

		Vector2 bottomCenter = new Vector2(raycastObject.transform.position.x, raycastObject.transform.position.y + (bottom.offset.y * raycastObject.transform.localScale.y));
		Vector2 topCenter = new Vector2(raycastObject.transform.position.x, raycastObject.transform.position.y + (top.offset.y * raycastObject.transform.localScale.y));

		downRays.Clear ();
		upRays.Clear ();

		Vector2 start = bottomCenter;
		for (float i = -1; i <= 1; i++) {
			//for bottom, we need to be concerned with walkable surfaces so we use raycastAll instead of raycast
			Vector2 drawNormal = Vector3.Normalize (new Vector2 (i, -1));

			if (i == 0) {
				RaycastHit2D[] allray = Physics2D.RaycastAll (start, drawNormal, bottomCircleRayLength + Mathf.Abs(velocity.y), RayMask);
				Debug.DrawLine (start, start + drawNormal * (bottomCircleRayLength + Mathf.Abs(velocity.y)), Color.cyan);
				foreach(RaycastHit2D ray in allray){
					downRays.Add (new Hit (ray, -1, i, start + drawNormal * adjustedRadius));
				}
			}
			else {
				RaycastHit2D[] allray = Physics2D.RaycastAll (start, drawNormal, sideCircleRayLength, RayMask);
				Debug.DrawLine (start, start + drawNormal * sideCircleRayLength, Color.cyan);
				if (allray.Length > 0) {
					foreach (RaycastHit2D ray in allray) {
						downRays.Add (new Hit (ray, -1, i, start + drawNormal * adjustedRadius));
					}
				}
				else if (velocity.y < 0) {
					Vector2 rayStart = start + drawNormal * sideCircleRayLength;
					allray = Physics2D.RaycastAll (rayStart, Vector2.up, velocity.y, RayMask);
					Debug.DrawLine (rayStart, rayStart + Vector2.up * velocity.y, Color.cyan);
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
				RaycastHit2D ray = Physics2D.Raycast (start, drawNormal, bottomCircleRayLength + Mathf.Abs(velocity.y), RayMask);
				Debug.DrawLine (start, start + drawNormal * (bottomCircleRayLength + Mathf.Abs(velocity.y)), Color.cyan);
				if (ray.collider != null) {
					upRays.Add (new Hit (ray, 1, i, start + drawNormal * adjustedRadius));
				}
			}
			else {
				RaycastHit2D ray = Physics2D.Raycast (start, drawNormal, sideCircleRayLength, RayMask);
				Debug.DrawLine (start, start + drawNormal * sideCircleRayLength, Color.cyan);
				if (ray.collider != null) {
					upRays.Add (new Hit (ray, 1, i, start + drawNormal * adjustedRadius));
				}
				else if (velocity.y > 0) {
					Vector2 rayStart = start + drawNormal * sideCircleRayLength;
					ray = Physics2D.Raycast (rayStart, Vector2.up, Mathf.Abs(velocity.y), RayMask);
					Debug.DrawLine (rayStart, rayStart + Vector2.up * Mathf.Abs(velocity.y), Color.cyan);
					if (ray.collider != null) {
						upRays.Add (new Hit (ray, 1, i, start + drawNormal * adjustedRadius, Vector2.Distance(start, ray.point)));
					}
				}
			}
		}
	}

	public void RaycastHorizontal(List<Hit> leftRays, List<Hit> rightRays, Vector2 velocity){
		//returns 1 if 0 or positive
		int directionX = (int)Mathf.Sign (velocity.x);

		Vector2 bottomCenter = new Vector2(raycastObject.transform.position.x, raycastObject.transform.position.y + (bottom.offset.y * raycastObject.transform.localScale.y));
		Vector2 topCenter = new Vector2(raycastObject.transform.position.x, raycastObject.transform.position.y + (top.offset.y * raycastObject.transform.localScale.y));

		rightRays.Clear();
		leftRays.Clear ();

		//bottom
		//only want sides
		Vector2 start = bottomCenter;
		for (float i = -1; i <= 1; i+=2) {
			//for bottom, we need to be concerned with walkable surfaces so we use raycastAll instead of raycast
			Vector2 drawNormal = Vector3.Normalize (new Vector2 (i, -1));

			RaycastHit2D[] allray = Physics2D.RaycastAll (start, drawNormal, sideCircleRayLength, RayMask);
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
			else if (Mathf.Sign(velocity.x) == i) {
				Vector2 rayStart = start + drawNormal * sideCircleRayLength;
				allray = Physics2D.RaycastAll (rayStart, Vector2.right, velocity.x, RayMask);
				Debug.DrawLine (rayStart, rayStart + Vector2.right * velocity.x, Color.magenta);
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

			RaycastHit2D[] allray = Physics2D.RaycastAll (start, drawNormal, sideCircleRayLength, RayMask);
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
			else if (Mathf.Sign(velocity.x) == i) {
				Vector2 rayStart = start + drawNormal * sideCircleRayLength;
				allray = Physics2D.RaycastAll (rayStart, Vector2.right, velocity.x, RayMask);
				Debug.DrawLine (rayStart, rayStart + Vector2.right * velocity.x, Color.magenta);
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
			start = new Vector2 (raycastObject.transform.position.x, clamped_y);

			//left
			float velocity_addition = velocity.x <= 0 ? velocity.x : 0;
			RaycastHit2D ray = Physics2D.Raycast (start, Vector2.left, sideRayLength - velocity_addition, RayMask);
			Debug.DrawLine (start, start + Vector2.left * (sideRayLength - velocity_addition), Color.cyan);
			if (ray.collider != null) {
				leftRays.Add (new Hit (ray, 0, -1, start + Vector2.left * side.bounds.extents.x));
			}

			//right
			velocity_addition = velocity.x >= 0 ? velocity.x : 0;
			ray = Physics2D.Raycast (start, Vector2.right, sideRayLength + velocity_addition, RayMask);
			Debug.DrawLine (start, start + Vector2.right * (sideRayLength + velocity_addition), Color.cyan);
			if (ray.collider != null) {
				rightRays.Add (new Hit (ray, 0, 1, start + Vector2.right * side.bounds.extents.x));
			}
		}
	}
}
