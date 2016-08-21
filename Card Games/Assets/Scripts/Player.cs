using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class Player : NetworkBehaviour {

	public static Player s_local_player;

	[SerializeField]
	private GameObject m_hand_card;
	private Card m_held_card;

	public GameObject m_card_prefab; //Kinds messy would like the card prefab to be placed elsewhere.
	public GameObject m_hand_object;
	public SyncListString m_hand = new SyncListString ();

	public static void SetLocalPlayer () {
		Player[] players = FindObjectsOfType<Player> ();
		foreach (Player player in players) {
			if (player.isLocalPlayer) {
				s_local_player = player;
			}
		}
	}

	void Start () {
		SetLocalPlayer ();
		if (isLocalPlayer) {
			m_hand_object = GameObject.Find ("Hand");
		}
	}

	// Hand interactions

	[ClientRpc]
	private void RpcNewHandCard (string card) {
		if (isLocalPlayer) {
			GameObject new_hand_card = Instantiate (m_hand_card);
			new_hand_card.transform.SetParent (m_hand_object.transform);
			new_hand_card.GetComponent<UICard> ().m_name = card; // !

			byte[] bytes = System.IO.File.ReadAllBytes (Application.dataPath + "/../Card_Sprites/" + card);
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

	[Command]
	public void CmdAddToHand (GameObject card_obj, string card) {
		m_hand.Add (card);
		RpcNewHandCard (card);
		Destroy (card_obj);
	}

	[Command]
	public void CmdDropFromHand (string name, Vector3 drop_position) {
		Card.CreateNewCard (name, drop_position, null);
	}

	// Flipping Cards

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
