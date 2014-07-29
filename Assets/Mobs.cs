﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Mobs : MonoBehaviour {
	
	/*mobs manager state machine -
	 * - player movement, position, health, weapons
	 * - objectives
	 * - mobs count, health, positions, weapons
	 */
	
	public GameObject mobPrefab;
	public int mobSpeed = 40;
	public int minDist = 40;
	public int pathSize = 5;
	public int pathFat = 10;
	
//	private List<Mob> allMobs = new List<Mob>();
	private Transform trans;
	private Transform camTrans;
	
	void Awake () {
		trans = gameObject.transform;
		camTrans = Camera.main.transform;
	}
	
	protected T CreateMob <T> (GameObject prefab, Vector3 position) where T : Mob {
		var mobj = (GameObject)Object.Instantiate(prefab, position, Quaternion.identity);
		var mob = mobj.AddComponent<T>();
		Debug.Log("Creating mob");
//		allMobs.Add(mob);
		return mob;
	}
	
	public void InitBlockade (int number) {
		var subTrans = Submarine.Trans;
		var subPos = subTrans.position;
		var endPos = subTrans.forward * 1000 + subPos;
		Debug.Log("end pos: "+ endPos);
		var carrier = CreateMob<Carrier>(Carrier.GetPrefab(), endPos);
		for (int i = 0; i < number; i++) {
			Ray ray = new Ray(endPos, subTrans.forward*-1);
			var rot = Quaternion.Euler(0, Random.Range(-20f, 20f), 0);
			ray.direction = rot * ray.direction;
			var mobPos = ray.GetPoint(Random.Range(500f, 1500f));
			var mob = CreateMob<Mob>(Mob.GetPrefab(), mobPos);
			//var mobRot = Vector3.Angle(mob.Trans.forward, endPos-mobPos);
			mob.Trans.Rotate(0f, Random.Range(0f, 360f), 0f);
			var move = new MoveState();
			move.destination = endPos;
			mob.startState = move;
			//mob.Trans.forward = endPos-mobPos;
		}
	}
	
	public void InitAllUrBass (int numberOfBass) {
		for (int i = 0; i < numberOfBass; i++) {
			var bass = (GameObject)GameObject.Instantiate(mobPrefab);
//			allMobs.Add(bass);
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
//			allMobs.Add(shark);
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
