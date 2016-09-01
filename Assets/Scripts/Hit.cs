using UnityEngine;
using System.Collections;

public class Hit{
	public RaycastHit2D raycastHit;
	public float vertical; //-1 down, 0 horizontal only, 1 up
	public float horizontal; //-1 left, 0 vertical only, 1 right
	public Vector2 colliderCrossPoint; //where the ray crossed the player's collider
	public float distance;

	public Hit(RaycastHit2D _rayHit, float _vert, float _hori, Vector2 _ccp){
		raycastHit = _rayHit;
		vertical = _vert;
		horizontal = _hori;
		colliderCrossPoint = _ccp;
		distance = raycastHit.distance;
	}

	public Hit(RaycastHit2D _rayHit, float _vert, float _hori, Vector2 _cpp, float _dist) : this(_rayHit, _vert, _hori, _cpp){
		distance = _dist;
	}
}
