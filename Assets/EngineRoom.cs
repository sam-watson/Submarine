using UnityEngine;
using System.Collections;
using Holoville.HOTween;

public class EngineRoom : MonoBehaviour {
	
	private float startSpeed;
	private float curSpeed;
	private float maxSpeed = 100f;
	private float minTurnRadius;
	private int curTurn;

	private Transform trans;
	public Transform Trans { get { return trans ?? transform ; }}
	
	private LTDescr tween;
	private Tweener tweener;
	
	public float Speed { get { return curSpeed; }}
	
	void Awake () {
		trans = transform;
	}
	
	void Start () {
		if (curSpeed != 0 && tweener == null) {
			SetSpeed(startSpeed);
		}
	}
	
	public void SetSpeed (float newSpeed) {
		if (trans == null) {
			startSpeed = newSpeed;
		} else {
			float oldSpeed = curSpeed;
			curSpeed = Mathf.Clamp(newSpeed, 0, maxSpeed);
			float distance = 30f;
			if ( oldSpeed == 0f || Mathf.Sign(oldSpeed) != Mathf.Sign(newSpeed) ) {
				MoveAlong(distance);
			} else {
				tweener.timeScale = Speed;
			}
		}
	}
	
	public void ChangeSpeed (float delta) {
		if (delta == 0f) return;
		SetSpeed(curSpeed + delta);
	}
	
	private void MoveAlong() {
		MoveAlong(30f);
	}
	
	private void MoveAlong (float distance) {
		Vector3 to = trans.position;
		if (curSpeed != 0) {
			to = new Ray(trans.position, trans.forward*Mathf.Sign(curSpeed)).GetPoint(distance);
		}
		if (tweener != null) {
			tweener.Kill();
		}
		TweenParms straightParms = new TweenParms().Prop("position", trans.forward*distance, true)
			.Loops(-1, LoopType.Incremental)
			.SpeedBased();
		tweener = HOTween.To(trans, Speed, straightParms);
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
