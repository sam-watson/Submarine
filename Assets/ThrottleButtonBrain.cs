using UnityEngine;
using System.Collections;

public class ThrottleButtonBrain : ButtonBrain {
	
	public UILabel speedLabel;
	private TweenAlpha speedLabelTween;
	
	protected override void Start () {
		base.Start ();
		speedLabelTween = speedLabel.GetComponent<TweenAlpha>();
	}
	
	void Update () {
		if (pressed) {
			var delta = Input.GetAxis("Mouse Y");
			if (delta != 0f) {
				Submarine.EngineRoom.ChangeSpeed(delta);
				speedLabel.text = Submarine.EngineRoom.Speed.ToString();
			}
		}
	}
	
	protected override void OnPress (bool isPressed) {
		base.OnPress (isPressed);
		if (isPressed) {
			TweenAlpha.Begin(speedLabel.gameObject, 0f, 1f);
		} else {
			TweenAlpha.Begin(speedLabel.gameObject, 1f, 0f);
		}
	}
}
