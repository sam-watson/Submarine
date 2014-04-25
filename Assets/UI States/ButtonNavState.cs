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
	
//	void Update () {
//		if (headingDelta != 0) {
//			periscope.AdjustHeading(headingDelta);
//		}
//	}

	public override void Enter (StateContext context)
	{
		base.Enter (context);
		navPanelMap.leftButton.GetComponent<NavButtonHelper>().Setup(-2f, periscope);
		navPanelMap.rightButton.GetComponent<NavButtonHelper>().Setup(5f, periscope);
//		EventDelegate.Set( navPanelMap.leftButton.onClick, PanLeft );
//		EventDelegate.Set( navPanelMap.rightButton.onClick, PanRight );
	}
	
	public override void Exit ()
	{
		base.Exit ();
		navPanelMap.leftButton.onClick.Clear();
		navPanelMap.rightButton.onClick.Clear();
	}
	
	public void PanLeft () {
		periscope.AdjustHeading(-5f);
		//headingDelta = headingDelta == 0 ? -2f : 0;
	}
	
	public void PanRight () {
		periscope.AdjustHeading(5f);
		//headingDelta = headingDelta == 0 ? 2f : 0;
	}
}
