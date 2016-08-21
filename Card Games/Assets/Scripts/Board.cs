using UnityEngine;
using System.Collections;

public class Board : MonoBehaviour {

	public static bool s_no_drag = false;

	// Currently only contains code for dragging and moving the camera.

	private Vector3 m_drag_point;
	private Vector3 m_camera_origin;
	private bool m_dragging = false;

	public static void DisableDrag () {
		s_no_drag = true;
	}

	public static void EnableDrag () {
		s_no_drag = false;
	}

	void OnMouseDown () {
		m_drag_point = Camera.main.ScreenToViewportPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0));
		m_camera_origin = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, Camera.main.transform.position.z);
		m_dragging = true;
	}

	void OnMouseUp () {
		m_dragging = false;
	}

	void Update () {
		if (m_dragging && !s_no_drag) {
			Vector3 offset = m_drag_point - Camera.main.ScreenToViewportPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0));
			Camera.main.transform.position = m_camera_origin + (Camera.main.transform.position.z * -1 * offset);
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
