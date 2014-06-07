using UnityEngine;
using System.Collections;
using Holoville.HOTween;

public class Torpedo : MonoBehaviour {
	
	public Vector3 target;

	protected float launchDistance = 10f;
	protected float travelSpeed = 80f;
	protected float turnRadius = 30f;
	
	protected Transform trans;
	protected Tweener tweener;
	
	protected Transform _camTrans;
	protected Transform camTrans {
		get {
			if (_camTrans == null) {_camTrans = Camera.main.transform;}
			return _camTrans;
		}
	}
	
	void Awake () {
		trans = gameObject.transform;
	}
	
	// necessary prior inputs: (TODO:origin) and target vectors
	public virtual Torpedo Launch () {
		SetAtLaunchPoint();
		float time = GetTravelTime(launchDistance);
		var launchParms = GetLaunchParms();
		tweener = HOTween.To(trans, time, launchParms);
		return this;
	}
	
	protected virtual void SetAtLaunchPoint () {
		var camParent = camTrans.parent;
		trans.position = camParent.position;
		trans.forward = camParent.forward;
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
		HOTween.Kill(other.gameObject);
		GameObject.Destroy(other.gameObject);
		HOTween.Kill(tweener);
		GameObject.Destroy(this.gameObject);
		Debug.Log("BOOOOM!");
	}
				
	public void SelfDestruct () {
		GameObject.Destroy(this.gameObject);
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
		var rotParent = new GameObject();
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
//			.Loops(3, LoopType.Incremental)
			.OnComplete(SelfDestruct);
		tweener.ResetAndChangeParms(TweenType.To, time, parms);
	}
	
//	public virtual void OnUpdate() {
//		var pos = trans.position;
//		Debug.DrawLine(pos, pos, Color.red, 10f);
//	}
}