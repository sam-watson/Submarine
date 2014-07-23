using UnityEngine;
using System.Collections;

public class UiStateManager : StateManager {

	public GuiMap guiMap;
	public Mobs mobs;
	
	private static UiStateManager _Instance;
	public static UiStateManager Instance {
		get {
			return _Instance;
		}
	}
	
	void Start () {
		if (Instance != null) {
			Debug.Log("!!!Created more than one instance of UiStateManager!!!");
		}
		_Instance = this;
		new ButtonNavState().Enter(new StateContext(this));
		//mobs.InitSharks(0);
		//mobs.InitAllUrBass(0);
		mobs.InitBlockade(3);
	}
}
