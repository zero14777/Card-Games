using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Board : MonoBehaviour {

	public static bool s_no_drag = false;
	public static float s_rotation = 0;

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
		if (!GameManager.Instance.m_over_UI) {
			m_dragging = true;
		}
	}

	void OnMouseUp () {
		m_dragging = false;
	}

	void Update () {
		if (m_dragging && !GameManager.Instance.m_over_UI && !s_no_drag) {
			Vector3 offset = m_drag_point - Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.z * -1));
			offset.z = 0; 
			Camera.main.transform.position = m_camera_orig + offset;
			m_drag_point = Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.z * -1));
			m_camera_orig = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, Camera.main.transform.position.z);
		}

		if (Input.GetKeyDown ("q")) {
			Camera.main.transform.Rotate (new Vector3 (0, 0, -90));
			s_rotation -= 90;
		}

		if (Input.GetKeyDown ("e")) {
			Camera.main.transform.Rotate (new Vector3 (0, 0, 90));
			s_rotation += 90;
		}
		if (!m_dragging) {
			if (Input.GetKeyDown ("r")) {
				Camera.main.transform.position = new Vector3 (0, 0, Camera.main.transform.position.z); //! This seems to autosync sometimes? been unable to replicate
			}

			if (Input.GetAxis ("Mouse ScrollWheel") > 0f && Camera.main.transform.position.z < -20) { // forward
				Camera.main.transform.position = new Vector3 (Camera.main.transform.position.x, Camera.main.transform.position.y, Camera.main.transform.position.z + 1);
			} else if (Input.GetAxis ("Mouse ScrollWheel") < 0f && Camera.main.transform.position.z > -40) { // backwards
				Camera.main.transform.position = new Vector3 (Camera.main.transform.position.x, Camera.main.transform.position.y, Camera.main.transform.position.z - 1);
			}
		}
	}

	private void CreateBlankDeck () {
		Player.s_local_player.CmdCreateDeck (Camera.main.ScreenToWorldPoint ( new Vector3
			(Input.mousePosition.x, Input.mousePosition.y, (Camera.main.transform.position.z * -1))));
	}

	private void CreateToken () {
		Player.s_local_player.CmdCreateToken (Camera.main.ScreenToWorldPoint ( new Vector3
			(Input.mousePosition.x, Input.mousePosition.y, (Camera.main.transform.position.z * -1))));
	}

	private void OnMouseOver () {
		if (Input.GetMouseButtonDown(1)) {
			List<Tuple<string, UnityEngine.Events.UnityAction>> functions = 
				new List<Tuple<string, UnityEngine.Events.UnityAction>> ();
			functions.Add(new Tuple<string, UnityEngine.Events.UnityAction>
				("Create New Deck", new UnityEngine.Events.UnityAction (CreateBlankDeck)));
			functions.Add(new Tuple<string, UnityEngine.Events.UnityAction>
				("Create New Token", new UnityEngine.Events.UnityAction (CreateToken)));
			GameManager.Instance.RightClickMenu (functions);
		}
	}
}
