using UnityEngine;
using System.Collections;

public class Mob : MonoBehaviour {
	
	public EngineRoom engineRoom;
	public FireControl fireControl;
	
	protected StateManager command;
	
	public State startState;
	
	protected Transform trans;
	public Transform Trans { get { return trans ?? transform ; }}
	
	void Awake () {
		trans = transform;
	}
	
	public virtual void AssignAttributes (ref float accel, ref float speed) {
		accel = 1f;
		speed = 30f;
	}
	
	void Start () {
		engineRoom = gameObject.AddComponent<EngineRoom>();
		fireControl = gameObject.AddComponent<FireControl>();
		//StartCoroutine(FireAtWill());
		command = gameObject.AddComponent<MobStateManager>();
		if (startState != null) {
			startState.Enter(new StateContext(command));
		}
	}
	
	public static GameObject GetPrefab () {
		string prefabName = Mathf.RoundToInt(Random.value) == 0 ? "battleship01" : "patrol boat01";
		var prefab = (GameObject)Resources.Load(prefabName);
		return prefab;
	}
	
	IEnumerator FireAtWill () {
		while (true) {
			//Debug.Log("Prepare to fire");
			float randTime = Random.Range(5f, 15f);
			yield return new WaitForSeconds(randTime);
			fireControl.Launch<Torpedo>(transform, Submarine.Trans.position);
		}
	}
}
