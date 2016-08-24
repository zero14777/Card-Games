using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Deck : NetworkBehaviour {

	public SyncListString m_deck = new SyncListString ();

	void Start () {
		AddCard ("m15_frame.png");
	}

	void OnMouseOver () {
		if (Input.GetKeyDown("d") && m_deck.Count > 0) {
			Player.s_local_player.CmdDrawToHand (GetTopCard()); // bad way to do this everything should happen on the server same with other functions
		}
		if (Input.GetKeyDown("r") && m_deck.Count > 0) {
			Player.s_local_player.CmdReveal (GetTopCard(), Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, (Camera.main.transform.position.z * -1))));
		}
	}

	public void AddCard (string card) {
		m_deck.Add (card);
	}

	public string GetTopCard () {
		string top_card = m_deck[m_deck.Count - 1];
		m_deck.RemoveAt (m_deck.Count - 1);
		return top_card;
	}

	/*public string DrawCard () {
		return GetTopCard ();
	}
	???
	public string RevealCard () {
		return GetTopCard ();
	}*/

	public void ShuffleDeck () {
		SyncListString new_deck = new SyncListString ();
		while (m_deck.Count > 0) {
			int pick_a_card = Random.Range(0, m_deck.Count);
			new_deck.Add (m_deck [pick_a_card]);
			m_deck.RemoveAt (pick_a_card);
		}
		m_deck = new_deck;
	}
}
