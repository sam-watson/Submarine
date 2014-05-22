using UnityEngine;
using System.Collections;

public class Torpedo : MonoBehaviour {
	
	//instance info - path behavior
	public LTDescr tween;
	
	float turnRadius;
	float travelSpeed;
	Vector3 target;
	
	Transform trans;
	
	//static info
	private static Transform _camTrans;
	private static Transform camTrans {
		get {
			if (_camTrans == null) {_camTrans = Camera.main.transform;}
			return _camTrans;
		}
	}
	
	void Awake () {
		trans = gameObject.transform;
	}
	
	// inputs: type, origin and target vectors, path behavior(angle, deviance, distance, speed)
	public static Torpedo Launch (GameObject prefab, Vector3 target) {
		var torpedobj = (GameObject)GameObject.Instantiate(prefab);
		var torpedo = torpedobj.AddComponent<Torpedo>();
		//torpedo behavior below, needs to be parameterized - launch point, starting path, ease, onCompletes, target, speed
		// Pre-launch setup - target (position, speed, turn, onCompletes - type properties set by class or enum)
		torpedo.target = target;
		torpedo.trans.position = camTrans.parent.position;
		torpedo.travelSpeed = 10f;
		torpedo.turnRadius = 5f;
		var launchDest = torpedo.GetLaunchTrajectory(target);
		float time = (launchDest - torpedo.trans.position).magnitude / torpedo.travelSpeed;
		torpedo.tween = LeanTween.move(torpedobj, launchDest, time);
		torpedo.tween.setEase(LeanTweenType.easeInCubic);
		torpedo.tween.setOnComplete(torpedo.OnLaunchComplete);
		torpedo.tween.setOnUpdate(torpedo.OnUpdate);
		return torpedo;
	}
			
	public void OnUpdate(float id) {
		var pos = trans.position;
		Debug.DrawLine(pos, pos, Color.red, 10f);
	}
	
	//startPath = straight launch path method
	private Vector3 GetLaunchTrajectory (Vector3 target) {
		OrientLaunchPath();
		var ray = new Ray(camTrans.parent.position, camTrans.forward);
		return ray.GetPoint(5);
	}
	
	private void OrientLaunchPath () {
		trans.forward = camTrans.forward;
	}
	
	public void OnLaunchComplete () {
		TurnToTarget();
	}
	
	public void OnTurnComplete () {
		var time = (target - trans.position).magnitude / travelSpeed;
		tween = LeanTween.move(gameObject, target, time);
		tween.setDestroyOnComplete(true);
	}
	
	private void TurnToTarget () {
		//determine turn end point
		var startPoint = trans.position;
		var crowFlies = target - startPoint;
		if (crowFlies.magnitude < turnRadius*2) {
			//target is within turning radius - torpedo needs to turn earlier or later to make it
			return;
		}
		var tan = crowFlies;
		var fwd = trans.forward;
		bool obtuse = Vector3.Angle(crowFlies, fwd) > 90f;
		bool leftHand = transform.InverseTransformPoint(target).x < 0f;
		var rot = Vector3.up;
		Vector3.OrthoNormalize(ref fwd, ref tan, ref rot);
		Ray ray = new Ray(startPoint, tan);
		var turnOrigin = ray.GetPoint(turnRadius);
		ray.origin = turnOrigin;
		ray.direction = -tan;
		var originToTarget = target - turnOrigin;
		var tanToTargetAngle = Vector3.Angle(-tan, originToTarget);
		if (obtuse) tanToTargetAngle = 360f - tanToTargetAngle;
		//var postTurnDist = Mathf.Sqrt(originToTarget.magnitude*originToTarget.magnitude - turnRadius*turnRadius);
		var theta = Mathf.Acos( turnRadius/originToTarget.magnitude )* Mathf.Rad2Deg;
		var turnAngle = tanToTargetAngle-theta;
		if (leftHand) turnAngle *= -1;
		TurnAbout(turnOrigin, rot, turnAngle);
	}
	
	public void TurnAbout(Vector3 point, Vector3 rotAxis, float angle) {
		var rotParent = new GameObject();
		var parenTrans = rotParent.transform;
		parenTrans.position = point;
		trans.parent = parenTrans;
		var time = Mathf.Abs(angle/360) * 2*Mathf.PI*turnRadius / travelSpeed;
		tween = LeanTween.rotateAround(rotParent, rotAxis, angle, time);
		tween.setOnComplete(OnTurnComplete);
	}
	
//	private void CustomTurnAround (Vector3 rotAxis, float turnAngle) {
//		Ray ray = new Ray(
//		var endTan = Quaternion.AngleAxis(turnAngle, rot) * ray.direction;
//		ray.direction = endTan;
//		var finalPoint = ray.GetPoint(turnRadius); //wrong?
//		//THEN JUST DO THE CURVE HOOORAY
//		int curveCount = obtuse ? 4 : 2;
//		Vector3[] curves = new Vector3[curveCount * 4];
//		var curveAngle = turnAngle/curveCount;
//		var rotQuat = Quaternion.AngleAxis(curveAngle, rot);
//		ray.direction = -tan;
//		Ray anchorRay;
//		float anchorDist = Mathf.Sqrt(2)/2 * turnRadius;
//		var curveTan = fwd;
//		for (int i=0; i<curveCount; i++) {
//			var pointA = ray.GetPoint(turnRadius);
//			curves[0 + 4*i] = pointA;
//			curveTan *= -1;
//			anchorRay = new Ray(pointA, curveTan);
//			var anchorA = ray.GetPoint(anchorDist);
//			curves[2 + 4*i] = anchorA;
//			ray.direction = rotQuat * ray.direction;
//			var pointB = ray.GetPoint(turnRadius);
//			curves[3 + 4*i] = pointB;
//			var rayDir = ray.direction;
//			Vector3.OrthoNormalize(ref rayDir, ref curveTan);
//			anchorRay = new Ray(pointB, curveTan);
//			var anchorB = anchorRay.GetPoint(anchorDist);
//			curves[1 + 4*i] = anchorB;
//		}
//		var time = curveAngle/360*2*Mathf.PI*turnRadius / travelSpeed;
//		LTBezierPath curvePath = new LTBezierPath(curves);
//		tween = LeanTween.move(gameObject, curvePath.pts, time);
//		tween.setOnComplete(OnTurnComplete);
//		tween.setOnUpdate(OnUpdateDebug);
//		OnStartDebug();
//	}
	
	public void OnStartDebug() {
		Vector3 lpt = Vector3.up;
		foreach (var pt in tween.path.pts) {
			if (lpt != Vector3.up) {
				Debug.DrawLine(lpt, pt, Color.green, 20f);
			}
			lpt = pt;
		}
	}
	
}
