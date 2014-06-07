using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Mobs : MonoBehaviour {
	
	public GameObject mobPrefab;
	public int mobSpeed = 10;
	public int minDist = 40;
	public int pathSize = 5;
	public int pathFat = 10;
	
	private List<GameObject> allMobs = new List<GameObject>();
	private Transform trans;
	private Transform camTrans;
	
	// Use this for initialization
	void Awake () {
		trans = gameObject.transform;
		camTrans = Camera.main.transform;
	}
	
	// Update is called once per frame
	void Update () {}
	
	public void InitAllUrBass (int numberOfBass) {
		for (int i = 0; i < numberOfBass; i++) {
			var bass = (GameObject)GameObject.Instantiate(mobPrefab);
			allMobs.Add(bass);
			var bassTrans = bass.transform;
			bassTrans.parent = trans;
			Vector2 randXY = Random.insideUnitCircle*500;
			Vector3 relativePos = new Vector3( randXY.x, 0, randXY.y);
			Vector3 bassPos = camTrans.parent.TransformPoint(relativePos);
			bassTrans.position = bassPos;
			bassTrans.eulerAngles = new Vector3(0f, Random.Range(0f, 360f), 0f);
			bass.AddComponent<Mob>();
		}
	}
	
	public void InitSharks (int numberOfSharks) {
		for (int i = 0; i < numberOfSharks; i++) {
			var shark = (GameObject)GameObject.Instantiate(mobPrefab);
			allMobs.Add(shark);
			var sharkTrans = shark.transform;
			sharkTrans.parent = trans;
			Vector2 randXY = Random.insideUnitCircle*100;
			Vector3 relativePos = 
				new Vector3( randXY.x, 0, randXY.y);
			Vector3 sharkPos = camTrans.parent.TransformPoint(relativePos);
			sharkTrans.position = sharkPos;
			//tween path
			float diameter;
			var path = SharkPath(sharkPos, out diameter);
			float speed = diameter * 1/mobSpeed*100;
			LeanTween.move(shark, path.pts, speed).setLoopClamp();
		}
	}
	
	private LTBezierPath SharkPath (Vector3 startingPoint, out float diameter) {
		Vector3[] curves = new Vector3[8];
		var cameraPos = camTrans.parent.position;
		var ray = new Ray(startingPoint, cameraPos - startingPoint);
		var distance = (startingPoint - cameraPos).magnitude;
		var minRad = distance + minDist;
		diameter = Random.Range(minRad, minRad*pathSize);
		var farPoint = ray.GetPoint(diameter);
		var perp = new Vector3(-ray.direction.z, 0, ray.direction.x);
		ray.direction = perp;
		curves[0] = startingPoint;
		curves[2] = ray.GetPoint(Random.Range(minRad, minRad*pathFat));
		ray.origin = farPoint;
		curves[1] = ray.GetPoint(Random.Range(minRad, minRad*pathFat));
		curves[3] = farPoint;
		ray.direction*= -1;
		ray.origin = startingPoint;
		curves[4] = farPoint;
		curves[5] = ray.GetPoint(Random.Range(minRad, minRad*pathFat));
		ray.origin = farPoint;
		curves[6] = ray.GetPoint(Random.Range(minRad, minRad*pathFat));
		curves[7] = startingPoint;
		return new LTBezierPath(curves);
	}
}
