using UnityEngine;
using System.Collections;

public class MobState : State {
	
	protected Mob mob;

	public override void Enter (StateContext context) {
		base.Enter (context);
		mob = ((MobStateManager)context.manager).mob;
	}
}
