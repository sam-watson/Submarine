using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UiState : State {

	protected List<PanelMap> panelMaps = new List<PanelMap>();
	
	public override void Enter (StateContext context) {
		base.Enter (context);
		ActivatePanels();
	}
	
	public override void Exit () {
		base.Exit ();
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
		panelMaps.Clear();
	}
}
