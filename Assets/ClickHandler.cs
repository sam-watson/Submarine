using UnityEngine;
using System.Collections;

public class ClickHandler : MonoBehaviour {
	//TODO: make singleton class
	
	public Collider water;
	private RaycastHit waterHit;
	
	public LineRenderer navLine;
	
	// Use this for initialization
	void Start () {
		SetNavLine();
		navLine.SetPosition(0, Vector3.zero);
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(0) && GetWaterHit()) {
			SetDest();
			Fire();
		}
	}
	
	private void SetDest () {
		Submarine.EngineRoom.SetDestination(waterHit.point, new EventDelegate(ClearDest));
		navLine.SetPosition(0, waterHit.point);
		navLine.SetPosition(1, waterHit.point+Vector3.up*5);
	}
	
	public void ClearDest () {
		Debug.Log("clearing dest");
		navLine.SetPosition(0, Vector3.zero);
		navLine.SetPosition(1, Vector3.zero);
	}
	
	private void Fire () {
		Submarine.FireControl.Launch<Torpedo>(Submarine.Trans, waterHit.point);
	}
	
	private bool GetWaterHit() {
		// TODO: fix camera setup probs (layers) and reevaluate placement/structure of gui check (all raycasting thru UICamera?)
		// TODO: touch
		// raytrace from mouse (or touch) against water collider
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		waterHit = new RaycastHit();
		if (UICamera.Raycast(Input.mousePosition, out waterHit)) {
			return false;
		}
		if (water.Raycast(ray, out waterHit, water.bounds.size.magnitude)) {
			return true;
		}
		return false;
	}
	
	public void SetNavLine () {
		navLine = Submarine.Instance.gameObject.AddComponent<LineRenderer>();
	}
}
