using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Player : NetworkBehaviour {

	private Card m_held_card;

	void Start () {
		Card.SetLocalPlayer ();
	}

	[Command]
	public void CmdFlip (GameObject card_obj) {
		Card card = card_obj.GetComponent<Card> ();
		card.Flip ();
	}

	// Dragging & Dropping

	[Command]
	public void CmdGrab (GameObject card_obj) {
		m_held_card = card_obj.GetComponent<Card> ();
		m_held_card.SetHeld (true);
	}

	[Command]
	public void CmdRelease (GameObject card_obj) {
		m_held_card.SetHeld (false);
		m_held_card = null;
	}

	[Command]
	public void CmdDrag (GameObject card_obj, Vector3 mouse_position) {
		m_held_card.transform.position = mouse_position;
	}
}
