using UnityEngine;
using System.Collections;
using Holoville.HOTween;

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
	private Tweener spTweener;
	
	public float Speed { get { return setSpeed; }}
	
	void Awake () {
		trans = transform;
	}
	
	void Start () {
		SetAttributes(null);
		if (tweener == null && startSpeed != 0) {
			SetSpeed(startSpeed);
		}
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
		var diff = newSpeed - setSpeed;
		if (diff == 0) { return; }
		setSpeed = newSpeed;
		Debug.Log(tweener);
		if (tweener == null) {
			MoveAlong();
		}
		if (diff > 0) Accel();
		if (diff < 0) Decel();
	}
	
	public void ChangeSpeed (float delta) {
		if (delta == 0f) return;
		SetSpeed(Speed + delta);
	}
	
	private void MoveAlong() {
		MoveAlong(30f);
	}
	
	private void MoveAlong (float distance) {
		Vector3 to = trans.forward * distance;
		TweenParms straightParms = new TweenParms().Prop("position", trans.forward*distance, true)
			.Loops(-1, LoopType.Incremental)
			.SpeedBased();
		tweener = HOTween.To(trans, maxSpeed, straightParms);
		tweener.timeScale = curSpeed/maxSpeed;
	}
	
	private void TweenSpeed (float to) {
		TweenParms spParms = new TweenParms().Prop("timeScale", to).SpeedBased();
		spParms.OnUpdate(OnSpeedUpdate);
		var pctAccel = maxAccel/maxSpeed;
		spTweener = HOTween.To(tweener, pctAccel, spParms); //maxAccel is expressed as a percentage of maxSpeed
	}
	private void Accel () {
		TweenSpeed(1f);
	}
	private void Decel () {
		TweenSpeed(0f);
	}
	
	public void OnSpeedUpdate () {
		curSpeed = tweener.timeScale * maxSpeed;
		if (curSpeed >= setSpeed) {
			spTweener.Kill();
			tweener.timeScale = setSpeed/maxSpeed;
			curSpeed = setSpeed;
			return;
		}
	}
}
