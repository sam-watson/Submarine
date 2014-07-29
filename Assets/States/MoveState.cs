using UnityEngine;
using System.Collections;

public class MoveState : MobState {
	
	public Vector3 destination;
	
	public override void Enter (StateContext context) {
		base.Enter (context);
		if (destination != null) {
			mob.engineRoom.SetDestination(destination);
			Debug.Log(mob.Id + " Course Laid: " + context.destination);
		}
		mob.engineRoom.SetSpeed(10f);
	} 
}
