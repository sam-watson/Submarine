using UnityEngine;
using System.Collections;

public class ButtonNavState : State {
	
	private NavPanelMap navPanelMap;
	
	public ButtonNavState () {
		navPanelMap = StateManager.Instance.guiMap.navButtons as NavPanelMap;
		panelMaps.Add(navPanelMap);
	}

	public override void Enter (StateContext context)
	{
		base.Enter (context);
	}
	
	public void PanLeft () {
		
	}
	
	public void PanRight () {
		
	}
}
