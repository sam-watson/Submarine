using UnityEngine;
using System.Collections;

public class ButtonBrain : MonoBehaviour {
	
	protected UIButton button;
	protected bool pressed;
	
	protected virtual void Start () {
		button = GetComponent<UIButton>();
	}
	
	protected virtual void OnPress (bool isPressed) {
		pressed = isPressed;
	}
}
