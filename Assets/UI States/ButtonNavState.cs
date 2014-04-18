using UnityEngine;
using System.Collections;

public class ButtonNavState : State {
	
	private NavPanelMap navPanelMap;
	private Periscope periscope;
	
	private bool leftPressed;
	private bool rightPressed;
	private float headingDelta;
	
	public ButtonNavState () {
		var guiMap = StateManager.Instance.guiMap;
		navPanelMap = guiMap.navButtons;
		periscope = guiMap.periscope;
		panelMaps.Add(navPanelMap);
		
		Enter(new StateContext());
	}
	
	void Update () {
		if (headingDelta != 0) {
			periscope.AdjustHeading(headingDelta);
		}
	}

	public override void Enter (StateContext context)
	{
		base.Enter (context);
		EventDelegate.Set( navPanelMap.leftButton.onClick, PanLeft );
		EventDelegate.Set( navPanelMap.rightButton.onClick, PanRight );
	}
	
	public override void Exit ()
	{
		base.Exit ();
		navPanelMap.leftButton.onClick.Clear();
		navPanelMap.rightButton.onClick.Clear();
	}
	
	public void PanLeft () {
		headingDelta = headingDelta == 0 ? -2f : 0;
	}
	
	public void PanRight () {
		headingDelta = headingDelta == 0 ? 2f : 0;
	}
}
