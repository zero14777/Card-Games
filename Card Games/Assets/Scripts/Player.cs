using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Contains data for each player. A new player object is created for each client
/// connected.
/// </summary>
public class Player : NetworkBehaviour {

	public static Player s_local_player;
	[SyncVar]
	public int m_player_ID;
	[SyncVar]
	public string m_player_name = "player";

	[SerializeField]
	private GameObject m_hand_card;
	private Card m_held_card;

	public GameObject m_card_prefab; //Kinds messy would like the card prefab to be placed elsewhere.
	public GameObject m_hand_object;
	public SyncListString m_hand = new SyncListString ();

	/// <summary>
	/// Sets the Player.s_local_player to be the Player component
	/// that represents local client.
	/// </summary>
	public static void SetLocalPlayer () {
		Player[] players = FindObjectsOfType<Player> ();
		foreach (Player player in players) {
			if (player.isLocalPlayer) {
				s_local_player = player;
			}
		}
	}

	[Command]
	private void CmdSetName (string name) {
		m_player_name = name;
		PlayLog.Instance.LogEvent (m_player_name + " Joined");
	}

	/// <summary>
	/// Automatically sets up the local client player object.
	/// </summary>
	void Start () {
		if (isLocalPlayer) {
			SetLocalPlayer ();
			m_hand_object = GameObject.Find ("Hand");
			CmdSetName (MainMenu.m_player_name);
		}
		if (isServer) {
			GameManager.Instance.RpcUpdatePlayersList ();
		}
	}

	public override void OnNetworkDestroy () {
		if (isServer) {
			foreach (string card in m_hand) {
				Card.CreateNewCard (card, new Vector3 (0, 0, 0), m_card_prefab);  // Make cards drop to a more convenient spot !
			}
		}
	}

	// Send a message to the chat/play log

	[Command]
	public void CmdSendMessage (string message) {
		PlayLog.Instance.LogEvent (m_player_name + ": " + message);
	}

	// Hand interactions

	/// <summary>
	/// Sends a message out creates a UI card, but only on
	/// the client whose player picked up a card.
	/// </summary>
	/// <param name="card">A string that designates which
	/// image in the Cards folder is associated with this card.</param>
	[ClientRpc]
	private void RpcNewHandCard (string card) {
		if (isLocalPlayer) {
			GameObject new_hand_card = Instantiate (m_hand_card);
			new_hand_card.transform.SetParent (m_hand_object.transform);
			new_hand_card.GetComponent<UICard> ().m_name = card; // !

			byte[] bytes = System.IO.File.ReadAllBytes (Application.dataPath + "/../Cards/" + card);
			new_hand_card.GetComponent<Image> ().sprite = Card.GenerateSprite (bytes);
		}
	}

	/// <summary>
	/// Adds the given card to the hand of the player who picked it up
	/// then removes that card object from the server.
	/// </summary>
	/// <param name="card_obj">The card object on the server to be
	/// destroyed.</param>
	/// <param name="card">A string that designates which
	/// image in the Cards folder is associated with this card.</param>
	[Command]
	public void CmdAddToHand (GameObject card_obj, string card) {
		m_hand.Add (card);
		RpcNewHandCard (card);
		Destroy (card_obj);
		GameManager.Instance.RpcUpdatePlayersList ();
		PlayLog.Instance.LogEvent (m_player_name + " took a card from the table.");
	}

	[Command]
	public void CmdDrawToHand (GameObject deck_obj) {
		Deck draw_deck = deck_obj.GetComponent<Deck> ();
		string card = draw_deck.GetTopCard();
		m_hand.Add (card);
		RpcNewHandCard (card);
		GameManager.Instance.RpcUpdatePlayersList ();
		PlayLog.Instance.LogEvent (m_player_name + " drew a card from " + draw_deck.m_name + "."); // ! specify deck
	}

	/// <summary>
	/// Removes the designated card from the hand of the palyer and creates
	/// a server game object to represent that card on the board.
	/// </summary>
	/// <param name="card">A string that designates which
	/// image in the Cards folder is associated with this card.</param>
	/// <param name="drop_position">The position at which to generate
	/// a the new card object.</param>
	[Command]
	public void CmdDropFromHand (string card, Vector3 drop_position) {
		m_hand.Remove (card);
		Card.CreateNewCard (card, drop_position, m_card_prefab);
		GameManager.Instance.RpcUpdatePlayersList ();
		PlayLog.Instance.LogEvent (m_player_name + " dropped " + card + " from their hand.");
	}

	[Command]
	public void CmdReveal (GameObject deck_obj, Vector3 drop_position) {
		string card = deck_obj.GetComponent<Deck> ().GetTopCard();
		GameObject new_card_obj = Card.CreateNewCard (card, drop_position, m_card_prefab);
		new_card_obj.GetComponent<Card> ().Flip ();
		PlayLog.Instance.LogEvent (m_player_name + " revealed " + card + ".");
	}

	[Command]
	public void CmdPlaceOnDeck (string card, GameObject card_obj, GameObject deck_obj) {
		Deck place_deck = deck_obj.GetComponent<Deck> ();
		place_deck.AddCard (card);
		Destroy (card_obj);
		PlayLog.Instance.LogEvent (m_player_name + " placed a card on " + place_deck.m_name + ".");
	}

	[Command]
	public void CmdShuffleDeck (GameObject deck_obj) {
		Deck suffle_deck = deck_obj.GetComponent<Deck> ();
		suffle_deck.GetComponent<Deck> ().ShuffleDeck ();
		PlayLog.Instance.LogEvent (m_player_name + " shuffled " + suffle_deck.m_name + ".");
	}

	// Flipping Cards

	/// <summary>
	/// Flips the given card_object on the server and all connected
	/// clients.
	/// </summary>
	/// <param name="card_obj">The card object on the server to be
	/// flipped.</param>
	[Command]
	public void CmdFlip (GameObject card_obj) {
		Card card = card_obj.GetComponent<Card> ();
		card.Flip ();
		PlayLog.Instance.LogEvent (m_player_name + " flipped " + card.m_filename + ".");
	}

	// Dragging & Dropping

	[Command]
	public void CmdGrab (GameObject card_obj) {
		m_held_card = card_obj.GetComponent<Card> ();
		m_held_card.m_held = true;
	}

	[Command]
	public void CmdRelease () {
		m_held_card.m_held = false;
		m_held_card = null;
	}

	[Command]
	public void CmdDrag (Vector3 mouse_position) {
		m_held_card.m_drag_transform = mouse_position;
	}
}
