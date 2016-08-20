using UnityEngine;
using System.Collections;

public class Board : MonoBehaviour {

	// Currently only contains code for dragging and moving the camera.

	private Vector3 drag_point;
	private Vector3 camera_origin;
	private bool dragging = false;

	void OnMouseDown () {
		drag_point = Camera.main.ScreenToViewportPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0));
		camera_origin = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, Camera.main.transform.position.z);
		dragging = true;
	}

	void OnMouseUp () {
		dragging = false;
	}

	void Update () {
		if (dragging) {
			Vector3 offset = drag_point - Camera.main.ScreenToViewportPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0));
			Camera.main.transform.position = camera_origin + (Camera.main.transform.position.z * -1 * offset);
			Debug.Log(offset);
		}

		if (Input.GetKeyDown ("r")) {
			Camera.main.transform.position = new Vector3 (0, 0, Camera.main.transform.position.z);
		}

		if (Input.GetAxis ("Mouse ScrollWheel") > 0f && Camera.main.transform.position.z < -10) { // forward
			Camera.main.transform.position = new Vector3 (Camera.main.transform.position.x, Camera.main.transform.position.y, Camera.main.transform.position.z + 1);
		} else if (Input.GetAxis ("Mouse ScrollWheel") < 0f && Camera.main.transform.position.z > -20) { // backwards
			Camera.main.transform.position = new Vector3 (Camera.main.transform.position.x, Camera.main.transform.position.y, Camera.main.transform.position.z - 1);
		}
	}
}
