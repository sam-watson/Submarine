using UnityEngine;
using System.Collections;

public class Submarine : MonoBehaviour {

	public Mob mob;
	public Periscope periscope;
		
	private static Submarine _Instance;
	public static Submarine Instance {
		get {
			return _Instance;
		}
	}
	
	void Awake () {
		if (Instance != null) {
			Debug.Log("!!!Created more than one instance of Submarine!!!");
		}
		_Instance = this;
		mob = gameObject.AddComponent<Mob>();
	}
	
	void Start () {
	}
	
	public static EngineRoom EngineRoom {
		get {
			return Instance.mob.engineRoom;
		}
	}
	
	public static FireControl FireControl {
		get {
			return Instance.mob.fireControl;
		}
	}
	
	public static Periscope Periscope {
		get {
			return Instance.periscope;
		}
	}
	
	public static Transform Trans {
		get {
			return Instance.mob.Trans;
		}
	}
}
