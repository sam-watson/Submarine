using UnityEngine;
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
	
	private List<Mob> allMobs = new List<Mob>();
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
		allMobs.Add(mob);
		mob.crNum = allMobs.IndexOf(mob);
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
}