using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class Deck : NetworkBehaviour {

	public bool m_hovering = false;
	public SyncListString m_deck = new SyncListString ();


	void UpdateHoverText () {
		if (m_hovering) {
			GameManager.Instance.SetHoverText ("Cards: " + m_deck.Count + "\n" +
			"D - Draw\n" +
			"R - Reveal\n" +
			"S - Shuffle\n");
		} else {
			GameManager.Instance.SetHoverText ("");
		}
	}

	/// <summary>
	/// Should be called whenever something happens to the deck that
	/// changes the hover text. Will update the hovertext for any clients
	/// currently hover over this deck.
	/// </summary>
	[ClientRpc]
	void RpcUpdateHoverText () {
		UpdateHoverText ();
	}

	void OnMouseEnter () {
		m_hovering = true;
		UpdateHoverText ();
	}

	void OnMouseExit () {
		m_hovering = false;
		UpdateHoverText ();
	}

	void OnMouseOver () {
		GameManager.Instance.MoveHoverText ();
		if (Input.GetKeyDown("d") && m_deck.Count > 0) {
			Player.s_local_player.CmdDrawToHand (this.gameObject);
		}
		if (Input.GetKeyDown("r") && m_deck.Count > 0) {
			Player.s_local_player.CmdReveal (this.gameObject, Camera.main.ScreenToWorldPoint 
											(new Vector3 (Input.mousePosition.x, Input.mousePosition.y,
											(Camera.main.transform.position.z * -1))));
		}
		if (Input.GetKeyDown("s") && m_deck.Count > 0) {
			Player.s_local_player.CmdShuffleDeck (this.gameObject);
		}
		if (Input.GetKeyDown("l")) {
			GameManager.Instance.RightClickMenu ();//new NoArgDelegate (FlipEvent));
		}
	}

	public void AddCard (string card) {
		m_deck.Add (card);
		RpcUpdateHoverText ();
	}

	public string GetTopCard () {
		string top_card = m_deck[m_deck.Count - 1];
		m_deck.RemoveAt (m_deck.Count - 1);
		RpcUpdateHoverText ();
		return top_card;
	}

	public void ShuffleDeck () {
		List<string> temp_deck = new List<string> ();
		while (m_deck.Count > 0) {
			int pick_a_card = Random.Range(0, m_deck.Count);
			temp_deck.Add (m_deck [pick_a_card]);
			m_deck.RemoveAt (pick_a_card);
		}
		foreach (string card in temp_deck) {
			m_deck.Add (card);
		}
	}
}
