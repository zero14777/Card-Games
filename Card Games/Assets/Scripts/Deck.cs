using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Deck : NetworkBehaviour {

	public SyncListString m_deck = new SyncListString ();

	void OnMouseEnter () {
		GameManager.Instance.SetHoverText ("D - Draw\n" +
											"R - Reveal\n" +
											"S - Shuffle\n");
	}

	void OnMouseExit () {
		GameManager.Instance.SetHoverText ("");
	}

	void OnMouseOver () {
		GameManager.Instance.MoveHoverText ();
		if (Input.GetKeyDown("d") && m_deck.Count > 0) {
			Player.s_local_player.CmdDrawToHand (GetTopCard()); // bad way to do this everything should happen on the server same with other functions
		}
		if (Input.GetKeyDown("r") && m_deck.Count > 0) {
			Player.s_local_player.CmdReveal (GetTopCard(), Camera.main.ScreenToWorldPoint 
											(new Vector3 (Input.mousePosition.x, Input.mousePosition.y,
											(Camera.main.transform.position.z * -1))));
		}
		if (Input.GetKeyDown("s") && m_deck.Count > 0) {
			ShuffleDeck ();
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

	public void ShuffleDeck () {
		SyncListString new_deck = new SyncListString ();
		while (m_deck.Count > 0) {
			int pick_a_card = Random.Range(0, m_deck.Count);
			new_deck.Add (m_deck [pick_a_card]); // Producing errors
			m_deck.RemoveAt (pick_a_card);
		}
		m_deck = new_deck;
	}
}
