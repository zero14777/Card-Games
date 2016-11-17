using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class Card : Draggable {

	public bool m_hovering = false;

	// References
	private SpriteRenderer m_sprite_component;
	public Sprite m_front;

	// Misc state variables
	[SyncVar]
	public string m_filename = "";
	private Sprite m_back;
	private bool m_blownup = false;
	private Vector3 m_normal_size;
	public Vector3 m_blowup_size;
	[SyncVar]
	public bool m_upright;

	[SyncEvent]
	public event UnityEngine.Events.UnityAction EventFlip;

	public static GameObject CreateNewCard (string file_name, bool upright, float x_pos = 0, float y_pos = 0, float rotation = 0) {
		GameObject new_card = (GameObject)Instantiate (GameManager.Instance.m_card_prefab, new Vector3 (x_pos, y_pos, 0), Quaternion.identity);
		Card card_component = new_card.GetComponent<Card> ();
		card_component.m_filename = file_name;
		card_component.LoadFront ();
		//card_component.m_front = GameObject.Find(file_name + "Spawner").GetComponent<Image> ().sprite;
		card_component.m_rotation = rotation;
		card_component.m_upright = upright;
		NetworkServer.Spawn (new_card);
		return new_card;
	}

	private void Start () {
		m_holder = false;
		m_back = Resources.Load<Sprite> ("Card_Back");
		m_sprite_component = this.GetComponent<SpriteRenderer> ();
		EventFlip = new UnityEngine.Events.UnityAction (FlipEvent);
		m_normal_size = new Vector3 (0.75f, 0.75f, 1);
		m_blowup_size = new Vector3 (1.5f, 1.5f, 1);
		if (isClient) {
			if (File.Exists (Application.dataPath + "/../Cards/" + m_filename)) {
				LoadFront ();
				//m_front = GameObject.Find(m_filename + "Spawner").GetComponent<Image> ().sprite;
			} else {
				m_front = Resources.Load<Sprite> ("Missing_Data");
			}
		}
		if (m_upright) {
			m_sprite_component.sprite = m_front;
		} else {
			m_sprite_component.sprite = m_back;
		}
		if (isClient) {
			transform.rotation = Quaternion.Euler (new Vector3 (0, 0, m_rotation));
		}
		if (isServer) {
			Invoke ("BringToTop", 0.1f);
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

	public static string FormatName (string filename) {
		string name = Path.GetFileNameWithoutExtension (filename);
		return name;
	}

	// Keeps track of cursor and hover text

	private void OnMouseOver () {
		GameManager.Instance.MoveHoverText ();
		if (Input.GetKeyDown(KeyCode.Delete)) {
			Player.s_local_player.CmdDeleteCard (this.gameObject);
		}
		if (m_upright && Input.GetKeyDown("z")) {
			ChangeSize ();
		}

		if (Input.GetKeyDown("f")) {
			DoFlip ();
		}

		if (Input.GetKeyDown("a")) {
			Draw ();
		}
		if (m_hovering && Input.GetMouseButtonDown(1)) {
			List<Tuple<string, UnityEngine.Events.UnityAction>> functions = 
				new List<Tuple<string, UnityEngine.Events.UnityAction>> ();
			if (m_upright) {
				functions.Add (new Tuple<string, UnityEngine.Events.UnityAction>
					("Zoom", new UnityEngine.Events.UnityAction (ChangeSize)));
			}
			functions.Add(new Tuple<string, UnityEngine.Events.UnityAction>
				("Draw", new UnityEngine.Events.UnityAction (Draw)));
			functions.Add(new Tuple<string, UnityEngine.Events.UnityAction>
				("Flip", new UnityEngine.Events.UnityAction (DoFlip)));
			functions.Add(new Tuple<string, UnityEngine.Events.UnityAction>
				("Rotate Left", new UnityEngine.Events.UnityAction (RotateLeft)));
			functions.Add(new Tuple<string, UnityEngine.Events.UnityAction>
				("Rotate Right", new UnityEngine.Events.UnityAction (RotateRight)));
			GameManager.Instance.RightClickMenu (functions);
		}
	}

	private void OnMouseEnter () {
		m_hovering = true;
		string hover_text = "F - Flip Card\nA - Add to Hand\n";
		if (m_upright) {
			hover_text = hover_text + "Z - Zoom\n";
		}
		GameManager.Instance.SetHoverText (hover_text);
	}

	private void OnMouseExit () {
		m_hovering = false;
		GameManager.Instance.SetHoverText ("");
	}

	private void OnDestroy () {
		if (m_hovering) {
			OnMouseExit ();
		}
	}

	// Actions Cards object can perform

	private void ChangeSize () {
		if (m_blownup) {
			Shrink ();
		} else {
			Blowup ();
		}
	}

	private void DoFlip () {
		Player.s_local_player.CmdFlip (this.gameObject);
	}

	private void Draw () {
		Player.s_local_player.CmdAddToHand (this.gameObject);
	}

	private void RotateLeft () {
		Player.s_local_player.CmdRotateCard (this.gameObject, 90.0f);
	}

	private void RotateRight () {
		Player.s_local_player.CmdRotateCard (this.gameObject, -90.0f);
	}

	// Card Zoom (Client side only)

	private void Blowup () {
		transform.localScale = m_blowup_size;
		m_blownup = true;
	}

	private void Shrink () {
		transform.localScale = m_normal_size;
		m_blownup = false;
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
		if (m_hovering) {
			OnMouseEnter ();
		}
	}

	public void Flip () {
		EventFlip ();
	}

	// Dragging & Dropping

	protected override void OnMouseUp () {
		if (m_holder) {
			RaycastHit2D[] hits = Physics2D.RaycastAll (Camera.main.ScreenToWorldPoint (
				new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 
					(Camera.main.transform.position.z * -1))),
				Vector2.zero);
			foreach (RaycastHit2D hit in hits) {
				if (hit.transform.gameObject.GetComponent<Deck> () != null) {
					Player.s_local_player.CmdPlaceOnDeck (m_filename, this.gameObject, hit.transform.gameObject);
					return;
				}
			}
			Player.s_local_player.CmdRelease ();
			m_holder = false;
		}
	}

	protected override void DoOnGrabOrRelease (bool held) {
		if (held) {
			int order_in_layer = GetComponent<SpriteRenderer> ().sortingOrder;
			GameObject[] cards = GameObject.FindGameObjectsWithTag ("Card");
			foreach (GameObject card in cards) {
				if (GetComponent<Collider2D>().IsTouching (card.GetComponent<Collider2D> ())) {
					int card_order = card.GetComponent<SpriteRenderer> ().sortingOrder;
					if (card_order >= order_in_layer) {
						order_in_layer = card_order + 1;
					}
				}
			}
			transform.position = new Vector3 (transform.position.x, transform.position.y, -0.0000000000001f * order_in_layer);
			this.gameObject.GetComponent<SpriteRenderer> ().sortingOrder = order_in_layer;
		}
	}

	// Managing Sorting layer order for cards

	private void OnCollisionEnter2D (Collision2D hit) {
		Card hit_card = hit.gameObject.GetComponent<Card> ();
		if (!(hit_card)) {
			return;
		} else if (m_held && !(hit_card.m_held)) {
			int card_order = hit.gameObject.GetComponent<SpriteRenderer> ().sortingOrder;
			if (card_order >= GetComponent<SpriteRenderer> ().sortingOrder) {
				GetComponent<SpriteRenderer> ().sortingOrder = card_order + 1;
			}
		}
	}

	[ServerCallback]
	public void BringToTop () {
		int order_in_layer = GetComponent<SpriteRenderer> ().sortingOrder;
		GameObject[] cards = GameObject.FindGameObjectsWithTag ("Card");
		foreach (GameObject card in cards) {
			if (GetComponent<Collider2D>().IsTouching (card.GetComponent<Collider2D> ())) {
				int card_order = card.GetComponent<SpriteRenderer> ().sortingOrder;
				if (card_order >= order_in_layer) {
					order_in_layer = card_order + 1;
				}
			}
		}
		transform.position = new Vector3 (transform.position.x, transform.position.y, -0.0000000000001f * order_in_layer);
		RpcMoveToTop(order_in_layer);
	}

	[ClientRpc]
	private void RpcMoveToTop (int order_in_layer) {
		this.gameObject.GetComponent<SpriteRenderer> ().sortingOrder = order_in_layer;
	}
}