using UnityEngine;
using System.Collections;

public class Board : MonoBehaviour {

	public static bool s_no_drag = false;

	private Vector3 m_drag_point;
	private Vector3 m_camera_orig;
	private bool m_dragging = false;

	public static void DisableDrag () {
		s_no_drag = true;
	}

	public static void EnableDrag () {
		s_no_drag = false;
	}

	void OnMouseDown () {
		m_drag_point = Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.z * -1));
		m_drag_point.z = 0;
		m_camera_orig = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, Camera.main.transform.position.z);
		m_dragging = true;
	}

	void OnMouseUp () {
		m_dragging = false;
	}

	void Update () {
		if (m_dragging && !s_no_drag) {
			Vector3 offset = m_drag_point - Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.z * -1));
			offset.z = 0; 
			Camera.main.transform.position = m_camera_orig + offset;
			m_drag_point = Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.z * -1));
			m_camera_orig = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, Camera.main.transform.position.z);
		}

		if (Input.GetKeyDown ("q")) {
			Camera.main.transform.Rotate (new Vector3 (0, 0, -90));
		}

		if (Input.GetKeyDown ("e")) {
			Camera.main.transform.Rotate (new Vector3 (0, 0, 90));
		}
		if (!m_dragging) {
			if (Input.GetKeyDown ("r")) {
				Camera.main.transform.position = new Vector3 (0, 0, Camera.main.transform.position.z); //! This seems to autosync sometimes? been unable to replicate
			}

			if (Input.GetAxis ("Mouse ScrollWheel") > 0f && Camera.main.transform.position.z < -10) { // forward
				Camera.main.transform.position = new Vector3 (Camera.main.transform.position.x, Camera.main.transform.position.y, Camera.main.transform.position.z + 1);
			} else if (Input.GetAxis ("Mouse ScrollWheel") < 0f && Camera.main.transform.position.z > -20) { // backwards
				Camera.main.transform.position = new Vector3 (Camera.main.transform.position.x, Camera.main.transform.position.y, Camera.main.transform.position.z - 1);
			}
		}
	}
}
