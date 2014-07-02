using UnityEngine;
using System.Collections;

public class MobStateManager : StateManager {

	public Mob mob;
	
	void Awake () {
		mob = gameObject.GetComponent<Mob>();
	}
}
