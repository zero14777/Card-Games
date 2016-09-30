using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class Deck : Draggable {
	
	// References
	private static GameObject m_deck_prefab;

	// Deck Values
	public string m_name = "deck";
	public bool m_hovering = false;
	public SyncListString m_deck = new SyncListString ();

	public static GameObject CreateNewDeck (string name, Vector3 position) {
		GameObject new_deck = (GameObject)Instantiate (GameManager.Instance.m_deck_prefab, new Vector3(position.x, position.y, 0.05f), Quaternion.identity);
		Deck deck_component = new_deck.GetComponent<Deck> ();
		deck_component.name = name;
		NetworkServer.Spawn (new_deck);
		return new_deck;
	}

	// Keeps track of cursor and hover text

	private void OnMouseOver () {
		GameManager.Instance.MoveHoverText ();
		if (Input.GetKeyDown("d") && m_deck.Count > 0) {
			Draw ();
		}
		if (Input.GetKeyDown("r") && m_deck.Count > 0) {
			Reveal ();
		}
		if (Input.GetKeyDown("s") && m_deck.Count > 0) {
			Player.s_local_player.CmdShuffleDeck (this.gameObject);
		}
		if (m_hovering && Input.GetMouseButtonDown(1)) {
			List<Tuple<string, UnityEngine.Events.UnityAction>> functions = new List<Tuple<string, UnityEngine.Events.UnityAction>> ();
			functions.Add(new Tuple<string, UnityEngine.Events.UnityAction>
				("Draw", new UnityEngine.Events.UnityAction (Draw)));
			functions.Add(new Tuple<string, UnityEngine.Events.UnityAction>
				("Reveal", new UnityEngine.Events.UnityAction (Reveal)));
			functions.Add(new Tuple<string, UnityEngine.Events.UnityAction>
				("Shuffle", new UnityEngine.Events.UnityAction (Shuffle)));
			GameManager.Instance.RightClickMenu (functions);
		}
	}

	private void OnMouseEnter () {
		m_hovering = true;
		UpdateHoverText ();
	}

	private void OnMouseExit () {
		m_hovering = false;
		UpdateHoverText ();
	}

	private void OnDestroy () {
		OnMouseExit ();
	}

	private void UpdateHoverText () {
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
	private void RpcUpdateHoverText () {
		UpdateHoverText ();
	}

	// Actions Decks object can perform

	private void Draw () {
		Player.s_local_player.CmdDrawToHand (this.gameObject);
	}

	private void Reveal () {
		Player.s_local_player.CmdReveal (this.gameObject, Camera.main.ScreenToWorldPoint 
			(new Vector3 (Input.mousePosition.x, Input.mousePosition.y,
			(Camera.main.transform.position.z * -1))));
	}

	private void Shuffle () {
		Player.s_local_player.CmdShuffleDeck (this.gameObject);
	}

	// Core Deck functionality

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
			int pick_a_card = UnityEngine.Random.Range(0, m_deck.Count);
			temp_deck.Add (m_deck [pick_a_card]);
			m_deck.RemoveAt (pick_a_card);
		}
		foreach (string card in temp_deck) {
			m_deck.Add (card);
		}
	}

	// Dragging & Dropping

	protected override void OnMouseDown () {
		if (!m_held && !GameManager.Instance.m_over_UI && Input.GetKey(KeyCode.LeftShift)) {
			Player.s_local_player.CmdGrab (this.gameObject);
			m_holder = true;
		}
	}
}
