using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class State {
	
	protected StateContext initialContext;
	protected List<PanelMap> panelMaps = new List<PanelMap>();

	public virtual void Enter (StateContext context) {
		initialContext = context;
		var manager = StateManager.Instance;
		if (manager.currentState != null) {
			manager.currentState.Exit();
		}
		manager.currentState = this;
		ActivatePanels();
		Debug.Log("Entering " + this.GetType());
	}
	
	public virtual void Exit( ) {
		DeActivatePanels();
	}
	
	protected virtual void ActivatePanels () {
		foreach (var panelMap in panelMaps) {
			panelMap.gameObject.SetActive(true);
		}
	}
	
	protected virtual void DeActivatePanels () {
		foreach (var panelMap in panelMaps) {
			panelMap.gameObject.SetActive(false);
		}
	}
}
