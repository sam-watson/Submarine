using UnityEngine;
using System.Collections;

public class Controller : MonoBehaviour {
	
	public Transform indicator;
	
	private Compass compass;
	private Gyroscope gyro;
	private Transform trans;
	private Vector3 initialHeading;
	private Vector3 lastHeading;
	
	// Use this for initialization
	void Start () {
		Screen.orientation = ScreenOrientation.Landscape;
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		trans = transform;
		gyro = Input.gyro;
		gyro.enabled = true;
		compass = Input.compass;
		compass.enabled = true;
		initialHeading = HorizontalHeading();
		lastHeading = initialHeading;
	}
	
	// Update is called once per frame
	void Update () {
		var currentHeading = HorizontalHeading();
		indicator.forward = currentHeading;
		var angle = Vector3.Angle(lastHeading, currentHeading);
		angle = gyro.userAcceleration.y < 0 ? -angle : angle;
		trans.Rotate(0, angle, 0);
		lastHeading = currentHeading;
	}
	
	private Vector3 HorizontalHeading () {
		var heading = ReorientNeutral(compass.rawVector);
		var orthoHeading = heading - gyro.gravity;
		return AxisFix(orthoHeading);
	}
	
	private Vector3 ReorientNeutral (Vector3 heading) {
		//landscape left gravity = (-1, 0, 0)
		//face up = (0, 0, 1) ; face down = (0, 0, -1) ; portrait = (0, -1, 0)
		var neutral = new Vector3(-1, 0, 0);
		var reverseTilt = Quaternion.FromToRotation(gyro.gravity, neutral); 
		return reverseTilt * heading;
	}
	
	private Vector3 AxisFix (Vector3 vector) {
		return new Vector3(-vector.y, vector.x, -vector.z);
	}
}
