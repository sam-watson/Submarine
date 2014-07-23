using UnityEngine;
using System.Collections;
using Holoville.HOTween;
using Holoville.HOTween.Plugins;

public class EngineRoom : MonoBehaviour {
	
	private bool init;
	
	private float maxAccel;
	private float maxSpeed;
	private float turnRadius;
	
	private float curSpeed;
	private float setSpeed;
	private float startSpeed;
	private Vector3 nullDest = new Vector3(0,1000,0);
	private Vector3 dest;
	
	private Transform rotTrans;
	private Transform trans;
	public Transform Trans { get { return trans ?? transform ; }}
	
	private Tweener tweener;
	private Tweener depthTweener;
	private Tweener spTweener;
	
	public float Speed { get { return setSpeed; }}
	public float RealSpeed { get { return curSpeed; }}
	
	void Awake () {
		trans = transform;
		dest = nullDest;
	}
	
	void Start () {
		SetAttributes(null);
		SetCourse();
		SetSpeed(startSpeed);
	}
	
	public void SetAttributes (Mob mob) {
		if (mob == null) {
			mob = GetComponent<Mob>();
		}
		mob.AssignAttributes(ref maxSpeed, ref maxAccel, ref turnRadius); //turnradius
		init = true;
	}
	
	public void SetDestination (Vector3 position) {
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
		var diff = setSpeed - curSpeed;
		if (diff != 0) Accel(diff < 0);
	}
	
	public void ChangeSpeed (float delta) {
		if (delta == 0f) return;
		SetSpeed(Speed + delta);
	}
	
	public void Dive () {
		if (depthTweener == null) {
			var depthParms = new TweenParms().Prop("position", new PlugVector3Y(-10f))
				.AutoKill(false);
			var pos = Trans.position;
			Trans.position = new Vector3(pos.x, 0f, pos.z);
			depthTweener = HOTween.To(Trans, 10f, depthParms);
		}
		depthTweener.PlayForward();
	}
	
	public void Surf () {
		depthTweener.PlayBackwards();
	}
	
	private void SetCourse () {
		if (dest != nullDest) {
			var relDest = dest-Trans.position;
			if (Vector3.Angle(Trans.forward, relDest) != 0) {
				TurnToDestination();
			} else {
				MoveStraight(relDest.magnitude);
				tweener.ApplyCallback(CallbackType.OnStepComplete, OnDestReached);
			}
		} else {
			MoveStraight(20f);
		}
	}
	
	private void MoveStraight (float distance) {
		TweenParms straightParms = new TweenParms().Prop("position", trans.forward*distance, true)
			.Loops(-1, LoopType.Incremental)
			.SpeedBased()
			.TimeScale(0f);
		tweener = HOTween.To(trans, maxSpeed, straightParms);
	}
	
	protected void TurnToDestination () {
		Debug.Log("turn start");
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
			tweener.ApplyCallback (CallbackType.OnComplete, TurnToDestination);
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
		Debug.Log("turn tween start");
		var rotParent = new GameObject();
		rotTrans = rotParent.transform;
		rotTrans.position = point;
		trans.parent = rotTrans;
		var maxTurnSpeed = GetAngularSpeed(maxSpeed, turnRadius);
		var turnParms = new TweenParms().Prop("eulerAngles", new Vector3(0, angle, 0))
			.SpeedBased()
			.TimeScale(0f)
			.OnComplete(SetCourse);
		tweener = HOTween.To(rotTrans, maxTurnSpeed, turnParms);
	}
	
	private float GetAngularSpeed (float speed, float radius) {
		return (speed/(2*Mathf.PI*radius))*360;
	}
	
	private void SpeedTween () {
		tweener.timeScale = 0f;
		TweenParms spParms = new TweenParms().Prop("timeScale", 1f).SpeedBased();
		spParms.OnUpdate(OnSpeedUpdate);
		var pctAccel = maxAccel/maxSpeed;
		spTweener = HOTween.To(tweener, pctAccel, spParms);
		spTweener.GoToAndPlay(curSpeed/maxSpeed);
	}
	
	private void Accel (bool decel) {
		if (spTweener == null) { SpeedTween(); }
		if (spTweener.isReversed != decel) {
			spTweener.Reverse();
		}
		spTweener.Play();
	}
	
	public void OnSpeedUpdate () {
		curSpeed = tweener.timeScale * maxSpeed;
		if ((curSpeed < setSpeed) == spTweener.isReversed) {
			Debug.Log("speed match, cur: "+ curSpeed+ " ,set: "+ setSpeed+ " Accel: "+ !spTweener.isReversed);
			spTweener.Pause();
			tweener.timeScale = setSpeed/maxSpeed;
			curSpeed = setSpeed;
			return;
		}
	}
	
	public void OnDestReached () {
		dest = nullDest;
	}
}
