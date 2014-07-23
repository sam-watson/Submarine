using UnityEngine;
using System.Collections;

public class DiveButtonBrain : ButtonBrain {
	
	private bool dived;
	
	protected override void Start ()
	{
		base.Start ();
		label.text = "Dive!";
	}
	
	public void OnClick () {
		if (dived) {
			Submarine.EngineRoom.Surf();
			label.text = "Dive!";
			dived = !dived;

		} else {
			Submarine.EngineRoom.Dive ();
			label.text = "Surface";
			dived = !dived;
		}
	}
}
