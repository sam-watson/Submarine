using UnityEngine;
using System.Collections;

public class MoveState : MobState {
	
	public override void Enter (StateContext context) {
		base.Enter (context);
		mob.engineRoom.SetSpeed(1f);
		Debug.Log("Course Laid");
	} 
}
