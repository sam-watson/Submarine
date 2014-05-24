using UnityEngine;
using System.Collections;

public class ThrottleButtonBrain : ButtonBrain {

	protected override void OnPress (bool isPressed) {
		base.OnPress (isPressed);
		if (isPressed) {
			// get an engine room script with a speed input method to call with Mouse Y Delta
			// get a gui element to act as indicator - label is easiest
		}
	}
}
