using UnityEngine;
using System.Collections;

public class NavButtonHelper : MonoBehaviour {

	private UIButton button;
	private float rate;
	private Periscope periscope;
	private bool pressed;
	
	void Start () {
		button = GetComponent<UIButton>();
	}
	
	void Update () {
		if (pressed) {
			periscope.AdjustHeading(rate);
		}
	}
	
	public void Setup (float rate, Periscope periscope) {
		this.rate = rate;
		this.periscope = periscope;
	}
	
	void OnPress (bool isPressed)
	{
		pressed = isPressed;
//		if (isPressed && button != null && button.isEnabled)
//		{
//			UIButton.current = button;
//			EventDelegate.Execute(button.onClick);
//			UIButton.current = null;
//		}
	}
}
