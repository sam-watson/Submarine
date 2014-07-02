using UnityEngine;
using System.Collections;

public class Submarine : MonoBehaviour {

	public EngineRoom engineRoom;
	public FireControl fireControl;
	public Periscope periscope;
	
	private Transform trans;
	
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
		trans = transform;
	}
	
	void Start () {
		fireControl = gameObject.AddComponent<FireControl>();
	}
	
	public static EngineRoom EngineRoom {
		get {
			return Instance.engineRoom;
		}
	}
	
	public static FireControl FireControl {
		get {
			return Instance.fireControl;
		}
	}
	
	public static Periscope Periscope {
		get {
			return Instance.periscope;
		}
	}
	
	public static Transform Trans {
		get {
			return Instance.trans;
		}
	}
}
