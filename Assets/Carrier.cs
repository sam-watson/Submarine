using UnityEngine;
using System.Collections;

public class Carrier : Mob {

	public static GameObject GetPrefab () {
		string prefabName = "aircraft carrier01";
		var prefab = (GameObject)Resources.Load(prefabName);
		return prefab;
	}
}
