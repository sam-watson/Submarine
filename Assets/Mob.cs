using UnityEngine;
using System.Collections;

public class Mob : MonoBehaviour {

	public FireControl fireControl;
	
	void Start () {
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
