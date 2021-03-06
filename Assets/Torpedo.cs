﻿using UnityEngine;
using System.Collections;
using Holoville.HOTween;

public class Torpedo : MonoBehaviour {
	
	public Transform origin; // better as a gameobject anatomy script from which torpedo selects a launch point
	public Vector3 target;
	
	public GameObject explosionPrefab;

	protected float launchDistance = 10f;
	protected float travelSpeed = 80f;
	protected float travelRange = 15f; //in seconds
	protected float turnRadius = 30f;
	
	protected Transform trans;
	protected Tweener tweener;
	protected GameObject rotParent; // TODO: make this part of prefabs
	
	void Awake () {
		trans = gameObject.transform;
	}
	
	public virtual Torpedo Launch () {
		SetAtLaunchPoint();
		float time = GetTravelTime(launchDistance);
		var launchParms = GetLaunchParms();
		tweener = HOTween.To(trans, time, launchParms);
		collider.isTrigger = false;
		StartCoroutine(TimedEvents());
		return this;
	}
	
	IEnumerator TimedEvents () {
		// arm warhead
		yield return new WaitForSeconds(1f);
		collider.isTrigger = true;
		// out of fuel - sink
		yield return new WaitForSeconds(travelRange);
		Expire();
	}
	
	protected virtual void SetAtLaunchPoint () {
		trans.position = origin.position;
		trans.forward = origin.forward;
	}
	
	protected virtual float GetTravelTime (float distance) {
		return distance/travelSpeed;
	}
	
	protected virtual TweenParms GetLaunchParms () {
		return new TweenParms().Prop("position", trans.forward*launchDistance, true)
			.Ease(EaseType.EaseInQuart)
			.OnComplete(OnLaunchComplete)
			.AutoKill(false);
	}
	
	public virtual void OnLaunchComplete () {
		TurnToTarget();
	}
		
	public virtual void OnTriggerEnter(Collider other) {
		if (other.tag != "Player") {
			HOTween.Kill(other.gameObject);
			GameObject.Destroy(other.gameObject);
		}
		Explode();
		Debug.Log("BOOOOM!");
	}
	
	public void Explode () {
		var splosion = (GameObject)Object.Instantiate(explosionPrefab, trans.position, Quaternion.identity);
		var explosion = splosion.AddComponent<Explosion>();
		Expire();
	}
				
	public void Expire () {
		HOTween.Kill(tweener);
		Object.Destroy(this.gameObject);
		if (rotParent != null) Object.Destroy(rotParent);
	}
	
	protected void TurnToTarget () {
		var pos = trans.position;
		var crowFlies = target - pos;
		var tan = crowFlies;
		var fwd = trans.forward;
		var rot = Vector3.up;
		Vector3.OrthoNormalize(ref fwd, ref tan, ref rot);
		Ray ray = new Ray(pos, tan);
		var turnOrigin = ray.GetPoint(turnRadius);
		var originToTarget = target - turnOrigin;
		if (originToTarget.magnitude < turnRadius) {
			//target is within turning radius, torpedo needs to turn later to get to it
			var time = GetTravelTime(5f);
			var delayParms = new TweenParms().Prop("position", trans.forward*5f, true)
				.Ease(EaseType.Linear).OnComplete(OnLaunchComplete).AutoKill(false);
			tweener.ResetAndChangeParms(TweenType.To, time, delayParms);
			return;
			// could also use quadratic equation to find precise distance - vector intersect with circle on plane
		}
		ray = new Ray(turnOrigin, -tan);
		var tanToTargetAngle = Vector3.Angle(-tan, originToTarget);
		bool obtuse = Vector3.Angle(crowFlies, fwd) > 90f;
		if (obtuse) tanToTargetAngle = 360f - tanToTargetAngle;
		var theta = Mathf.Acos( turnRadius/originToTarget.magnitude )* Mathf.Rad2Deg;
		var turnAngle = tanToTargetAngle-theta;
		bool leftHand = trans.InverseTransformPoint(target).x < 0f;
		if (leftHand) turnAngle *= -1;
		TurnAbout(turnOrigin, turnAngle);
	}
	
	protected void TurnAbout(Vector3 point, float angle) {
		rotParent = new GameObject();
		var parenTrans = rotParent.transform;
		parenTrans.position = point;
		trans.parent = parenTrans;
		var dist = Mathf.Abs(angle/360) * 2*Mathf.PI*turnRadius;
		var time = GetTravelTime(dist);
		var turnParms = new TweenParms().Prop("eulerAngles", new Vector3(0, angle, 0))
			.Ease(EaseType.Linear)
			.OnComplete(OnTurnComplete)
			.AutoKill(false);
		tweener = HOTween.To(parenTrans, time, turnParms);
	}
	
	public virtual void OnTurnComplete () {
		var relativeDest = target - trans.position;
		var time = GetTravelTime(relativeDest.magnitude);
		var parms = new TweenParms().Prop("position", relativeDest, true)
			.Loops(-1, LoopType.Incremental)
			.Ease(EaseType.Linear);
		tweener.ResetAndChangeParms(TweenType.To, time, parms);
	}
	
//	public virtual void OnUpdate() {
//		var pos = trans.position;
//		Debug.DrawLine(pos, pos, Color.red, 10f);
//	}
}