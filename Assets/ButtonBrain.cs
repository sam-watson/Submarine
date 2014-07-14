using UnityEngine;
using System.Collections;

public class ButtonBrain : MonoBehaviour {
	
	protected UIButton button;
	protected UILabel label;
	protected bool pressed;
	
	protected virtual void Start () {
		button = GetComponent<UIButton>();
		label = GetComponentInChildren<UILabel>();
	}
	
	protected virtual void OnPress (bool isPressed) {
		pressed = isPressed;
	}
}
