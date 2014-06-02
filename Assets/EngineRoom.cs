using UnityEngine;
using System.Collections;
using Holoville.HOTween;

public class EngineRoom : MonoBehaviour {
	
	private float curSpeed;
	private float maxSpeed;
	private float minTurnRadius;
	private int curTurn;
	private GameObject submarine;
	private Transform subTrans;
	private LTDescr tween;
	private Tweener tweener;
	
	public float Speed { get{ return curSpeed; }}
	
	void Start () {
		submarine = gameObject;
		subTrans = submarine.transform;
		maxSpeed = 100f;
	}
	
	public void ChangeSpeed (float delta) {
		if (delta == 0f) return;
		float newSpeed = curSpeed + delta;
		float oldSpeed = curSpeed;
		curSpeed = Mathf.Clamp(newSpeed, 0, maxSpeed);
		float distance = 30f;
		if (oldSpeed == 0f 	||	 Mathf.Sign(oldSpeed) != Mathf.Sign(newSpeed)) {
			MoveAlong(distance);
		} else {
			tweener.timeScale = Speed;
//			tween.time = GetTravelTime(distance);
		}
	}
	
	private void MoveAlong() {
		MoveAlong(30f);
	}
	
	private void MoveAlong (float distance) {
		Vector3 to = subTrans.position;
		if (curSpeed != 0) {
			to = new Ray(subTrans.position, subTrans.forward*Mathf.Sign(curSpeed)).GetPoint(distance);
		}
		if (tweener != null) {
			tweener.Kill();
		}
		TweenParms straightParms = new TweenParms().Prop("position", subTrans.forward*distance, true)
			.Loops(-1, LoopType.Incremental)
			.SpeedBased();
		tweener = HOTween.To(subTrans, Speed, straightParms);
//		tween = LeanTween.move(submarine, to, time);
//		tween.setOnComplete(MoveAlong);
	}
	
//	private void Turn(int delta) {
//		var newTurn = curTurn + delta;
//		var sn = Mathf.Sign(delta);
//		var fwd = subTrans.forward;
//		var tan = new Vector3(fwd.z*
//	}
	
	private float GetTravelTime (float distance) {
		return distance / curSpeed;
	}
}
