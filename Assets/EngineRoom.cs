using UnityEngine;
using System.Collections;

public class EngineRoom : MonoBehaviour {
	
	private float curSpeed;
	private float maxSpeed;
	private GameObject submarine;
	private Transform subTrans;
	private LTDescr tween;
	
	public float Speed { get{ return curSpeed; }}
	
	void Start () {
		submarine = gameObject;
		subTrans = submarine.transform;
		maxSpeed = 30f;
	}
	
	public void ChangeSpeed (float delta) {
		if (delta == 0f) return;
		float newSpeed = curSpeed + delta;
		float oldSpeed = curSpeed;
		curSpeed = Mathf.Clamp(newSpeed, -maxSpeed, maxSpeed);
		float distance = 30f;
		if (oldSpeed == 0f 	||	 Mathf.Sign(oldSpeed) != Mathf.Sign(newSpeed)) {
			MoveAlong(distance);
		} else {
			tween.time = GetTravelTime(distance);
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
		var time = GetTravelTime(distance);
		if (tween != null) {
			tween.cancel();
		}
		tween = LeanTween.move(submarine, to, time);
		tween.setOnComplete(MoveAlong);
	}
	
	private float GetTravelTime (float distance) {
		return distance / curSpeed;
	}
}
