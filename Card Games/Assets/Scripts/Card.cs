using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class Card : NetworkBehaviour {

	public bool m_hovering = false;

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
	[SyncVar(hook="DoRotation")]
	public float m_rotation;

	// Dragging & Dropping
	[SyncVar]
	public bool m_held;
	[SyncVar]
	public bool m_upright = false;
	private bool m_holder;
	[SyncVar]
	public Vector3 m_drag_transform;

	[SyncEvent]
	public event UnityEngine.Events.UnityAction EventFlip;

	public static GameObject CreateNewCard (string file_name, Vector3 position, GameObject card_prefab, float rotation = 0) {
		if (card_prefab != null) {
			m_card_prefab = card_prefab;
		}
		GameObject new_card = (GameObject)Instantiate (m_card_prefab, position, Quaternion.identity);
		Card card_component = new_card.GetComponent<Card> ();
		card_component.m_filename = file_name;
		card_component.LoadFront ();
		card_component.m_rotation = rotation;
		NetworkServer.Spawn (new_card);
		return new_card;
	}

	private void Start () {
		m_holder = false;
		m_back = Resources.Load<Sprite> ("Card_Back");
		m_sprite_component = this.GetComponent<SpriteRenderer> ();
		EventFlip = new UnityEngine.Events.UnityAction (FlipEvent);
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
		if (isClient) {
			transform.rotation = Quaternion.Euler (new Vector3 (0, 0, m_rotation));
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
		name.Replace ("_", " ");
		return name;
	}

	// Keeps track of cursor and hover text

	private void OnMouseOver () {
		GameManager.Instance.MoveHoverText ();
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
		Player.s_local_player.CmdRotate (this.gameObject, 90.0f);
	}

	private void RotateRight () {
		Player.s_local_player.CmdRotate (this.gameObject, -90.0f);
	}

	private void DoRotation (float rotation) {
		transform.rotation = Quaternion.Euler (new Vector3 (0, 0, rotation));
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

	private void OnMouseDown () {
		if (!m_held && !GameManager.Instance.m_over_UI) {
			Player.s_local_player.CmdGrab (this.gameObject);
			m_holder = true;
		}
	}

	private void OnMouseUp () {
		if (m_holder) {
			RaycastHit2D[] temp = Physics2D.RaycastAll (Camera.main.ScreenToWorldPoint (
														new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 
														(Camera.main.transform.position.z * -1))),
														Vector2.zero);
			foreach (RaycastHit2D hit in temp) {
				if (hit.transform.gameObject == GameObject.Find ("Deck")) {
					Player.s_local_player.CmdPlaceOnDeck (m_filename, this.gameObject, hit.transform.gameObject);
					return;
				}
			}
			Player.s_local_player.CmdRelease ();
			m_holder = false;
		}
	}

	private void OnMouseDrag () {
		if (m_holder) {
			Vector3 drag_pos = Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x,
																Input.mousePosition.y,
																(Camera.main.transform.position.z * -1)));
			Player.s_local_player.CmdDrag (drag_pos);
			transform.position = drag_pos;
		}
	}

	private void FixedUpdate () {
		if (m_held && !m_holder) {
			transform.position = Vector3.Lerp (transform.position, m_drag_transform, Time.deltaTime * m_lerp_time);
		}
	}
}