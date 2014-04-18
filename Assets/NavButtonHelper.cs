using UnityEngine;
using System.Collections;

public class NavButtonHelper : MonoBehaviour {

	private UIButton button;
	
	void Start () {
		button = GetComponent<UIButton>();
	}
	
	void OnPress (bool isPressed)
	{
		if (isPressed && button != null && button.isEnabled)
		{
			UIButton.current = button;
			EventDelegate.Execute(button.onClick);
			UIButton.current = null;
		}
	}
}
