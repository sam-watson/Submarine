using UnityEngine;
using System.Collections;

public class FireControl {
	
	private GameObject torpedoPrefab;
	
	public FireControl () {
		Init();
	}
	
	void Init() {
		torpedoPrefab = (GameObject)Resources.Load("torpedo", typeof(GameObject));
	}
	
	public T Launch<T>(Vector3 target) where T : Torpedo {
		var torpedobj = (GameObject)GameObject.Instantiate(torpedoPrefab);
		var torpedo = torpedobj.AddComponent<T>();
		torpedo.target = target;
		return (T)torpedo.Launch();
	}
}
