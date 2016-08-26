﻿using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;

public class Card : NetworkBehaviour {

	// References
	private static GameObject m_card_prefab;
	private SpriteRenderer m_sprite_component;
	public Sprite m_front;

	// Misc state variables
	[SyncVar]
	public string m_filename = "";
	private Sprite m_back;
	private bool m_blownup = false;
	private Vector3 m_normal_size;
	public Vector3 m_blowup_size;
	private int m_lerp_time = 15;

	// Dragging & Dropping
	[SyncVar]
	public bool m_held;
	[SyncVar]
	public bool m_upright = false;
	private bool m_holder;
	[SyncVar]
	public Vector3 m_drag_transform; // !!!!! RECT TRANSFORM OR REGULAR TRANSFORM?

	// Flipping
	public delegate void NoArgDelegate ();
	[SyncEvent]
	public event NoArgDelegate EventFlip;

	public static GameObject CreateNewCard (string file_name, Vector3 position, GameObject card_prefab) {
		if (card_prefab != null) {
			m_card_prefab = card_prefab;
		}
		GameObject new_card = (GameObject)Instantiate (m_card_prefab, position, Quaternion.identity);
		Card card_component = new_card.GetComponent<Card> ();
		card_component.m_filename = file_name;
		card_component.LoadFront ();
		NetworkServer.Spawn (new_card);
		return new_card;
	}

	void Start () {
		m_holder = false;
		m_back = Resources.Load<Sprite> ("Card_Back");
		m_sprite_component = this.GetComponent<SpriteRenderer> ();
		EventFlip = new NoArgDelegate (FlipEvent);
		m_normal_size = new Vector3 (0.33f, 0.33f, 1);
		m_blowup_size = new Vector3 (1, 1, 1);
		if (isClient) {
			if (File.Exists (Application.dataPath + "/../Cards/" + m_filename)) {
				LoadFront ();
			} else {
				m_front = Resources.Load<Sprite> ("Missing_Data");
			}
		}
		if (m_upright) {
			m_sprite_component.sprite = m_front;
		} else {
			m_sprite_component.sprite = m_back;
		}
	}

	/// <summary>
	/// Generates a card sized sprite for an image.
	/// </summary>
	/// <param name="card">The image to make a sprite of in a byte array.</param>
	public static Sprite GenerateSprite (byte[] bytes) {
		Texture2D texture = new Texture2D (1, 1);
		texture.LoadImage (bytes);
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
		return Sprite.Create (texture, new Rect (0, 0, texture.width, texture.height), 
								new Vector2 (0.5f, 0.5f), 100);
	}

	/// <summary>
	/// Loads an image for the front of the card from the
	/// Card_Sprites directory based on the set m_filename.
	/// If no image is found a generic missing image is displayed.
	/// </summary>
	public void LoadFront () {
		byte[] bytes = System.IO.File.ReadAllBytes (Application.dataPath + "/../Cards/" + m_filename);
		m_front = GenerateSprite (bytes);
	}

	// Hotkeys

	void OnMouseEnter () {
		string hover_text = "F - Flip Card\nA - Add to Hand\n";
		if (m_upright) {
			hover_text = hover_text + "Z - Zoom\n";
		}
				
		GameManager.Instance.SetHoverText (hover_text);
	}

	void OnMouseExit () {
		GameManager.Instance.SetHoverText ("");
	}

	void OnMouseOver () {
		GameManager.Instance.MoveHoverText ();
		if (m_upright && Input.GetKeyDown("z")) {
			ChangeSize ();
		}

		if (Input.GetKeyDown("f")) {
			Player.s_local_player.CmdFlip (this.gameObject);
		}

		if (Input.GetKeyDown("a")) {
			Player.s_local_player.CmdAddToHand (this.gameObject, m_filename);
		}
	}

	// Card Zoom

	private void Blowup () {
		transform.localScale = m_blowup_size;
		m_blownup = true;
	}

	private void Shrink () {
		transform.localScale = m_normal_size;
		m_blownup = false;
	}

	private void ChangeSize () {
		if (m_blownup) {
			Shrink ();
		} else {
			Blowup ();
		}
	}

	// Card Flipping

	public void FlipUp () {
		m_upright = true;
		m_sprite_component.sprite = m_front;
	}

	public void FlipDown () {
		m_upright = false;
		m_sprite_component.sprite = m_back;
		Shrink ();
	}

	public void FlipEvent () {
		if (m_upright) {
			FlipDown ();
		} else {
			FlipUp ();
		}
	}

	public void Flip () {
		EventFlip ();
	}

	// Dragging & Dropping

	void OnMouseDown () {
		if (!m_held) {
			Player.s_local_player.CmdGrab (this.gameObject);
			m_holder = true;
		}
	}

	void OnMouseUp () {
		if (m_holder) {
			RaycastHit2D[] temp = Physics2D.RaycastAll (Camera.main.ScreenToWorldPoint (
														new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 
														(Camera.main.transform.position.z * -1))),
														Vector2.zero);
			/*foreach (RaycastHit2D hit in temp) {
				Debug.Log (hit.transform.gameObject.name);
				if (hit.transform.gameObject == GameObject.Find ("Hand")) {
					Player.s_local_player.CmdAddToHand (this.gameObject, m_filename); Dropping to UI layer?
					return;
				}
			}*/
			foreach (RaycastHit2D hit in temp) {
				Debug.Log (hit.transform.gameObject.name);
				if (hit.transform.gameObject == GameObject.Find ("Deck")) {
					Player.s_local_player.CmdPlaceOnDeck (m_filename, this.gameObject, hit.transform.gameObject);
					return;
				}
			}
			Player.s_local_player.CmdRelease ();
			m_holder = false;
		}
	}

	void OnMouseDrag () {
		if (m_holder) {
			Vector3 drag_pos = Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x,
																Input.mousePosition.y,
																(Camera.main.transform.position.z * -1)));
			Player.s_local_player.CmdDrag (drag_pos);
			transform.position = drag_pos;
		}
	}

	void FixedUpdate () {
		if (m_held && !m_holder) {
			transform.position = Vector3.Lerp (transform.position, m_drag_transform, Time.deltaTime * m_lerp_time);
		}
	}
}