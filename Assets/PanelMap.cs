using UnityEngine;
using System.Collections;

public class PanelMap : MonoBehaviour {

	public UIPanel panel;
	
	void Start () {
		if (panel == null) {
			panel = GetComponent<UIPanel>();
		}
	}
}
