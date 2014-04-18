using UnityEngine;
using System.Collections;

public class StateManager : MonoBehaviour {
	
	public GuiMap guiMap;
	
	[HideInInspector] public State currentState;
	
	private static StateManager _Instance;
	public static StateManager Instance {
		get {
			return _Instance;
		}
	}
	
	void Start () {
		if (Instance != null) {
			Debug.Log("!!!Created more than one instance of StateManager!!!");
		}
		_Instance = this;
		currentState = new ButtonNavState();
	}
}
