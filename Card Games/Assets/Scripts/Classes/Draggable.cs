using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Draggable : NetworkBehaviour {

	// Dragging & Dropping

	//[SyncVar(hook="OnGrabOrRelease")]
	[SyncVar]
	public bool m_held;
	protected bool m_holder;
	[SyncVar]
	public Vector3 m_drag_transform;
	private int m_lerp_time = 15;

	protected virtual void OnMouseDown () {
		if (!m_held && !GameManager.Instance.m_over_UI) {
			Player.s_local_player.CmdGrab (this.gameObject);
			m_holder = true;
		}
	}

	protected virtual void OnMouseUp () {
		if (m_holder) {
			Player.s_local_player.CmdRelease ();
			m_holder = false;
		}
	}

	private void OnMouseDrag () {
		if (m_holder) {
			Vector3 drag_pos = Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x,
				Input.mousePosition.y,
				(Camera.main.transform.position.z * -1)));
			drag_pos = new Vector3 (drag_pos.x, drag_pos.y, transform.position.z);
			Player.s_local_player.CmdDrag (drag_pos);
			transform.position = drag_pos;
		}
	}

	private void OnGrabOrRelease (bool held) {
		DoOnGrabOrRelease (held);
	}

	protected virtual void DoOnGrabOrRelease (bool held) {
	}

	// Rotation

	[SyncVar(hook="DoRotation")]
	public float m_rotation;

	private void DoRotation (float rotation) {
		transform.rotation = Quaternion.Euler (new Vector3 (0, 0, rotation));
	}

	// Interpolating movements across the network

	private void FixedUpdate () {
		if (m_held && !m_holder) {
			transform.position = Vector3.Lerp (transform.position, m_drag_transform, Time.deltaTime * m_lerp_time);
		}
	}
}
