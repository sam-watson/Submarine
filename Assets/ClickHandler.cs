﻿using UnityEngine;
using System.Collections;

public class ClickHandler : MonoBehaviour {
	//TODO: make singleton class
	
	public Collider water;
	private RaycastHit waterHit;
	
	// Use this for initialization
	void Start () {}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(0) && GetWaterHit()) {
			Submarine.FireControl.Launch<Torpedo>(Submarine.Trans, waterHit.point);
		}
	}
	
	private bool GetWaterHit() {
		// TODO: GUI check
		// TODO: touch
		// raytrace from mouse (or touch) against water collider
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		waterHit = new RaycastHit();
		if (water.Raycast(ray, out waterHit, water.bounds.size.magnitude)) {
			return true;
		}
		return false;
	}
}
