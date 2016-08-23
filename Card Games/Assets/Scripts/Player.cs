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

	/// <summary>
	/// Automatically sets up the local client player object.
	/// </summary>
	void Start () {
		if (isLocalPlayer) {
			SetLocalPlayer ();
			m_hand_object = GameObject.Find ("Hand");
		}
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
			Texture2D texture = new Texture2D (1, 1);
			texture.LoadImage (bytes);

			// This scales all card images to the standard card size FUNCTIONALIZE THIS

			int width = 750;
			int height = 1050;
			Rect texture_rect = new Rect (0, 0, width, height);
			texture.filterMode = FilterMode.Trilinear;
			texture.Apply (true);
			RenderTexture render = new RenderTexture(width, height, 32);
			Graphics.SetRenderTarget(render);
			GL.LoadPixelMatrix(0,1,1,0);
			GL.Clear(true,true,new Color(0,0,0,0));
			Graphics.DrawTexture(new Rect(0,0,1,1),texture);
			texture.Resize (width, height);
			texture.ReadPixels (texture_rect, 0, 0, true);
			texture.Apply (true);

			new_hand_card.GetComponent<Image> ().sprite = Sprite.Create (texture, new Rect (0, 0, texture.width, texture.height), new Vector2 (0.5f, 0.5f), 100);
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
		Card.CreateNewCard (card, drop_position, null);
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
	}

	// Dragging & Dropping

	[Command]
	public void CmdGrab (GameObject card_obj) {
		m_held_card = card_obj.GetComponent<Card> ();
		m_held_card.SetHeld (true);
	}

	[Command]
	public void CmdRelease () {
		m_held_card.SetHeld (false);
		m_held_card = null;
	}

	[Command]
	public void CmdDrag (Vector3 mouse_position) {
		m_held_card.m_drag_transform = mouse_position;
	}
}
