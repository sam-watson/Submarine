using UnityEngine;
using System.Collections;

public class FireControl {
	
	private GameObject torpedoPrefab;
	private GameObject explosionPrefab;
	
	public FireControl () {
		Init();
	}
	
	void Init() {
		torpedoPrefab = (GameObject)Resources.Load("torpedo", typeof(GameObject));
		explosionPrefab = (GameObject)Resources.Load("explosion", typeof(GameObject));
	}
	
	public T Launch<T>(Transform origin, Vector3 target) where T : Torpedo {
		var torpedobj = (GameObject)GameObject.Instantiate(torpedoPrefab);
		var torpedo = torpedobj.AddComponent<T>();
		torpedo.origin = origin;
		torpedo.target = target;
		torpedo.explosion = explosionPrefab;
		return (T)torpedo.Launch();
	}
}
