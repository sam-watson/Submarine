using UnityEngine;
using System.Collections;

public class Mob : MonoBehaviour {
	
	public EngineRoom engineRoom;
	public FireControl fireControl;
	
	protected StateManager command;
	
	protected Transform trans;
	
	void Start () {
		trans = transform;
		engineRoom = new EngineRoom();
		fireControl = new FireControl();
		StartCoroutine(FireAtWill());
	}
	
	IEnumerator FireAtWill () {
		while (true) {
			Debug.Log("Prepare to fire");
			float randTime = Random.Range(5f, 15f);
			yield return new WaitForSeconds(randTime);
			fireControl.Launch<Torpedo>(transform, Submarine.Trans.position);
		}
	}
}
