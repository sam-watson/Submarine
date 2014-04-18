using UnityEngine;
using System.Collections;

public class Periscope : MonoBehaviour {
	
	public Transform orientationIndicator;
	
	private bool buttonControl;  //gui states - gyro, buttons, pause/menu ; later maybe - dive, target

	private Compass compass;
	private Gyroscope gyro;
	private Transform camTrans;
	
	private float angularHeadingAdjustment;
	
	// Use this for initialization
	void Start () {
#if UNITY_ANDROID
		Screen.orientation = ScreenOrientation.Landscape;
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		gyro = Input.gyro;
		gyro.enabled = true;
		compass = Input.compass;
		compass.enabled = true;
#endif
		camTrans = transform;
		buttonControl = true;
	}
	
	// Update is called once per frame
	void Update () {
		if (angularHeadingAdjustment != 0) {
			Debug.Log("Turning " + angularHeadingAdjustment);
			camTrans.Rotate(0, angularHeadingAdjustment, 0);
			angularHeadingAdjustment = 0;
		}
	}
	
	public void AdjustHeading (float deltaAngle) {
		angularHeadingAdjustment = deltaAngle;
		Debug.Log("Turn " + deltaAngle);
	}
	
	//stuff that doesn't work
	private Vector3 HorizontalCompassHeading () {
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
