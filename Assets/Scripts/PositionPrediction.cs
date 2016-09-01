using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Thanks thetop_04

public static class PositionPrediction {

	public static List<RigidbodyStatusData> predict(Vector3 startPosition, Vector3 startVelocity, float startAngularVelocity, 
													int cycles, float dragValue, bool _2DGravity, float gravityModifier, float angularDragValue, 
													float radiusObstacleDetection, int layerMaskOfObstacles){
		float t = Time.fixedDeltaTime;
		//drag how it is used by the unity physics
		float d = Mathf.Clamp01(1.0f - (dragValue * t));
		//angular drag
		float ad = Mathf.Clamp01(1-0f - (angularDragValue * t));
		//gravity with modifier
		Vector3 g;
		if (_2DGravity)
			g = Physics2D.gravity * gravityModifier;
		else
			g = Physics.gravity * gravityModifier;

		Vector3 currentPosition = startPosition;
		Vector3 currentVelocity = startVelocity;
		float currentAngularVelocity = startAngularVelocity;

		List<RigidbodyStatusData> list = new List<RigidbodyStatusData> ();

		for (int currentCycle = 0; currentCycle < cycles; currentCycle++) {
			currentPosition = currentPosition + t * d * currentVelocity + t * d * t * g;
			//update velocity AFTER the position to use the velocity from the last cycle
			currentVelocity = d * (currentVelocity + t * g);
			currentAngularVelocity = currentAngularVelocity * ad;

			if (_2DGravity) {
				if (Physics2D.OverlapCircle (currentPosition, radiusObstacleDetection, layerMaskOfObstacles) != null) {
					return list;
				}
			}
			else {
				if(Physics.OverlapSphere(currentPosition, radiusObstacleDetection, layerMaskOfObstacles) != null){
					return list;
				}
			}

			list.Add(new RigidbodyStatusData(currentPosition, currentVelocity, currentAngularVelocity));
		}
		return list;
	}

	public class RigidbodyStatusData{
		public Vector3 position;
		public Vector3 velocity;
		public float angularVelocity;

		public RigidbodyStatusData(Vector3 position, Vector3 velocity, float angularVelocity){
			this.position = position;
			this.velocity = velocity;
			this.angularVelocity = angularVelocity;
		}
	}
}
