using UnityEngine;
using System.Collections;

public abstract class State {
	
	protected StateContext context;
	public StateContext Context { get { return context; }}

	public virtual void Enter (StateContext context) {
		this.context = context;
		var manager = context.manager;
		if (manager.state != null) {
			manager.state.Exit();
		}
		manager.state = this;
		//Debug.Log("Entering " + this.GetType());
	}
	
	public virtual void Exit () {}
}
