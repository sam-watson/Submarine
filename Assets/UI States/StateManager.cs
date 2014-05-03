using UnityEngine;
using System.Collections;

public class StateManager : MonoBehaviour {
	
	public GuiMap guiMap;
	public Mobs mobs;
	
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
		new ButtonNavState().Enter(new StateContext());
		mobs.InitSharks(20);
	}
}
