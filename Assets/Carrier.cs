using UnityEngine;
using System.Collections;

public class Carrier : Mob {

	public static GameObject GetPrefab () {
		string prefabName = "aircraft carrier";
		var prefab = (GameObject)Resources.Load("Models/"+prefabName);
		return prefab;
	}
}
