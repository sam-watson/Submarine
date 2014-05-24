using UnityEngine;
using System.Collections;

public class NavButtonBrain : ButtonBrain {

	private float rate;
	private Periscope periscope;
	
	void Update () {
		if (pressed) {
			periscope.AdjustHeading(rate);
		}
	}
	
	public void Setup (float rate, Periscope periscope) {
		this.rate = rate;
		this.periscope = periscope;
	}
	
}
