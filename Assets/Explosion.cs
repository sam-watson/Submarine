using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Explosion : MonoBehaviour {
	
	protected float duration = 5f;
	protected float time;
	
	void Update () {
		time += Time.deltaTime;
		if (time >= duration) Object.Destroy(this.gameObject);
	}
}
