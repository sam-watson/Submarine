using UnityEngine;
using System.Collections;
using Holoville.HOTween;
using Holoville.HOTween.Plugins;

public class EngineRoom : MonoBehaviour {
	
	private bool init;
	private Mob mob;
	
	private float maxAccel;
	private float maxSpeed;
	private float turnRadius;
	
	public float curSpeed;
	private float setSpeed;
	private float startSpeed;
	private float normSpeed { get { return curSpeed/maxSpeed; }}
	private Vector3 nullDest = new Vector3(0,1000,0);
	private Vector3 dest;
	
	private Transform moveTrans;
	private Transform trans;
	//public Transform MoveTrans { get { return moveTrans; }}
	//public Transform ShipTrans { get { return trans ?? transform ; }}
	
	private Tweener tweener;
	private Tweener depthTweener;
	private Tweener spTweener;
	
	public float Speed { get { return setSpeed; }}
	public float RealSpeed { get { return curSpeed; }}
	
	void Awake () {
		trans = transform;
		moveTrans = new GameObject().transform;
		trans.parent = moveTrans;
		mob = GetComponent<Mob>();
		moveTrans.name = mob.Id + " mvmt";
		dest = nullDest;
	}
	
	void Start () {
		InitAttributes();
		SetDestination(dest);
		SetSpeedTween();
		SetSpeed(startSpeed);
	}
	
	public void InitAttributes () {
		if (mob == null) {
			mob = GetComponent<Mob>();
		}
		mob.AssignAttributes(ref maxSpeed, ref maxAccel, ref turnRadius); //turnradius
		init = true;
	}
	
	public void SetDestination (Vector3 position) {
		Debug.Log(mob.Id + " dest set to: " + position);
		dest = position;
		if (!init) { return; }
		SetCourse();
	}
	
	public void SetSpeed (float newSpeed) {
		if (!init) {
			startSpeed = newSpeed;
			return;
		}
		newSpeed = Mathf.Clamp(newSpeed, 0, maxSpeed);
		if (newSpeed == setSpeed) { return; }
		setSpeed = newSpeed;
		MatchSpeed();
	}
	
	private void MatchSpeed () {
		var diff = setSpeed - curSpeed;
		Accel(diff < 0);
	}
	
	public void ChangeSpeed (float delta) {
		if (delta == 0f) return;
		SetSpeed(Speed + delta);
	}
	
	public void Dive () {
		if (depthTweener == null) {
			var depthParms = new TweenParms().Prop("position", new PlugVector3Y(-10f))
				.AutoKill(false);
			var pos = moveTrans.position;
			moveTrans.position = new Vector3(pos.x, 0f, pos.z);
			depthTweener = HOTween.To(moveTrans, 10f, depthParms);
		}
		depthTweener.PlayForward();
	}
	
	public void Surf () {
		depthTweener.PlayBackwards();
	}
	
	private void SetCourse () {
		if (spTweener != null) spTweener.Pause(); //TODO refactor shit
		if (dest != nullDest) {
			var relDest = dest-trans.position;
			if (Mathf.Abs( Vector3.Angle(trans.forward, relDest) ) >= 1) {
				TurnToDestination();
			} else {
				//Debug.Log("moving str8 to dest: " + dest.ToString());
				MoveStraight(relDest.magnitude);
				tweener.ApplyCallback(CallbackType.OnStepComplete, OnDestReached);
			}
		} else {
			MoveStraight(20f);
		}
		if (spTweener != null) spTweener.Play();
	}
	
	private void MoveStraight (float distance) {
		if (tweener != null) tweener.Kill(true);
		TweenParms straightParms = new TweenParms().Prop("position", trans.forward*distance, true)
			.Loops(-1, LoopType.Incremental)
			.SpeedBased()
			.TimeScale(normSpeed);
		tweener = HOTween.To(moveTrans, maxSpeed, straightParms);
	}
	
	protected void TurnToDestination () {
		//Debug.Log("turn start");
		var pos = trans.position;
		var crowFlies = dest - pos;
		var tan = crowFlies;
		var fwd = trans.forward;
		var rot = Vector3.up;
		Vector3.OrthoNormalize(ref fwd, ref tan, ref rot);
		Ray ray = new Ray(pos, tan);
		var turnOrigin = ray.GetPoint(turnRadius);
		var originToTarget = dest - turnOrigin;
		if (originToTarget.magnitude < turnRadius) {
			//target is within turning radius, torpedo needs to turn later to get to it
			MoveStraight(10f);
			tweener.ApplyCallback (CallbackType.OnStepComplete, SetCourse);
			return;
			// could also use quadratic equation to find precise distance - vector intersect with circle on plane
		}
		ray = new Ray(turnOrigin, -tan);
		var tanToTargetAngle = Vector3.Angle(-tan, originToTarget);
		bool obtuse = Vector3.Angle(crowFlies, fwd) > 90f;
		if (obtuse) tanToTargetAngle = 360f - tanToTargetAngle;
		var theta = Mathf.Acos( turnRadius/originToTarget.magnitude )* Mathf.Rad2Deg;
		var turnAngle = tanToTargetAngle-theta;
		bool leftHand = trans.InverseTransformPoint(dest).x < 0f;
		if (leftHand) turnAngle *= -1;
		TurnAbout(turnOrigin, turnAngle);
	}
	
	protected void TurnAbout (Vector3 point, float angle) {
		if (tweener != null) tweener.Kill(true);
		Debug.Log(mob.Id + " turn start, pre position: "+ trans.position + ", turn pivot: " + point + ", angle: " + angle);
		moveTrans.DetachChildren();
		moveTrans.position = point;
		moveTrans.forward = trans.forward;
		trans.parent = moveTrans;
		Debug.Log(mob.Id + " now at " + trans.position);
		var maxTurnSpeed = GetAngularSpeed(maxSpeed, turnRadius);
		var turnParms = new TweenParms().Prop("eulerAngles", new Vector3(0, angle, 0), true)
			.SpeedBased()
			.TimeScale(normSpeed)
			.OnComplete(SetCourse);
		tweener = HOTween.To(moveTrans, maxTurnSpeed, turnParms);
	}
	
	private float GetAngularSpeed (float speed, float radius) {
		return (speed/(2*Mathf.PI*radius))*360;
	}
	
	private void SetSpeedTween () {
		//Debug.Log(mob.Id + " setting sp tween");
		var cacheSpeed = curSpeed;
		curSpeed = 0;
		TweenParms spParms = new TweenParms()
			.Prop("curSpeed", maxSpeed)
			.SpeedBased()
			.OnUpdate(OnSpeedUpdate)
			.AutoKill(false);
		spTweener = HOTween.To(this, maxAccel, spParms);
		spTweener.GoTo(cacheSpeed/maxAccel);
		spTweener.Pause();
		//Debug.Log(mob.Id + " sp now at " + spTweener.elapsed + ", should be " + normSpeed);
	}
	
	private void Accel (bool decel) {
		if (spTweener == null) { SetSpeedTween(); }
		if (spTweener.isReversed != decel) {
			spTweener.Reverse();
		}
		spTweener.Play();
	}
	
	public void OnSpeedUpdate () {
		tweener.timeScale = normSpeed;
		if ((curSpeed < setSpeed) == spTweener.isReversed) {
			Debug.Log(mob.Id + " speed match, cur: "+ curSpeed+ ", set: "+ setSpeed+ ", accel: "+ !spTweener.isReversed + ", dest: " + dest);
			spTweener.Pause();
			tweener.timeScale = setSpeed/maxSpeed;
			spTweener.GoTo(setSpeed/maxAccel);
			return;
		}
	}
	
	public void OnDestReached () {
		Debug.Log(mob.Id + " dest reached");
		dest = nullDest;
	}
}
