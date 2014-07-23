using UnityEngine;
using System.Collections;
using Holoville.HOTween;
using Holoville.HOTween.Plugins;

public class EngineRoom : MonoBehaviour {
	
	private bool init;
	private float maxAccel;
	private float maxSpeed;
	
	private float curSpeed;
	private float setSpeed;
	private float startSpeed;

	private Transform trans;
	public Transform Trans { get { return trans ?? transform ; }}
	
	private Tweener tweener;
	private Tweener depthTweener;
	private Tweener spTweener;
	
	public float Speed { get { return setSpeed; }}
	public float RealSpeed { get { return curSpeed; }}
	
	void Awake () {
		trans = transform;
	}
	
	void Start () {
		SetAttributes(null);
		SetSpeed(startSpeed);
	}
	
	public void SetAttributes (Mob mob) {
		if (mob == null) {
			mob = GetComponent<Mob>();
		}
		mob.AssignAttributes(ref maxAccel, ref maxSpeed);
		init = true;
	}
	
	public void SetSpeed (float newSpeed) {
		if (!init) {
			startSpeed = newSpeed;
			return;
		}		
		newSpeed = Mathf.Clamp(newSpeed, 0, maxSpeed);
		if (newSpeed == setSpeed) { return; }
		setSpeed = newSpeed;
		if (tweener == null) { //or direction change (reverse)
			Debug.Log("priming path");
			MoveAlong();
		}
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
			var pos = Trans.position;
			Trans.position = new Vector3(pos.x, 0f, pos.z);
			depthTweener = HOTween.To(Trans, 10f, depthParms);
		}
		depthTweener.PlayForward();
	}
	
	public void Surf () {
		depthTweener.PlayBackwards(); //Bug: will not work if sub started moving after submerge
	}
	
	private void MoveAlong() {
		MoveAlong(30f);
	}
	
	private void MoveAlong (float distance) {
		Vector3 to = trans.forward * distance;
		TweenParms straightParms = new TweenParms().Prop("position", trans.forward*distance, true)
			.Loops(-1, LoopType.Incremental)
			.SpeedBased()
			.TimeScale(0f);
		tweener = HOTween.To(trans, maxSpeed, straightParms);
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
}
