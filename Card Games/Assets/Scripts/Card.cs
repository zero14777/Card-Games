using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;

public class Card : NetworkBehaviour {

	static private Player s_local_player = null;

	[SyncVar]
	public string m_filename = "";
	public Sprite m_front;
	public Vector3 m_blowup_size;

	private Sprite m_back;
	[SyncVar]
	private bool m_held;
	[SyncVar]
	public bool m_upright = false;
	private bool m_holder;
	private bool m_blownup = false;
	private SpriteRenderer m_sprite_component;
	private Vector3 m_normal_size;

	public delegate void NoArgDelegate ();
	[SyncEvent]
	public event NoArgDelegate EventFlip;

	void Start () {
		m_holder = false;
		m_back = Resources.Load<Sprite> ("Card_Back");
		m_sprite_component = this.GetComponent<SpriteRenderer> ();
		EventFlip = new NoArgDelegate (FlipEvent);
		m_normal_size = new Vector3 (0.33f, 0.33f, 1);
		m_blowup_size = new Vector3 (1, 1, 1);
		if (isClient) {
			if (File.Exists (Application.dataPath + "/../Card_Sprites/" + m_filename)) {
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
		
	// <summary>
	// Loads an image for the front of the card from the
	// Card_Sprites directory based on the set m_filename.
	// 
	// If no image is found a generic missing image is displayed.
	// </summary>
	public void LoadFront () {
		byte[] bytes = System.IO.File.ReadAllBytes (Application.dataPath + "/../Card_Sprites/" + m_filename);
		Texture2D texture = new Texture2D (1, 1);
		texture.LoadImage (bytes);

		// This scales all card images to the standard card size

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

		m_front = Sprite.Create (texture, new Rect (0, 0, texture.width, texture.height), new Vector2 (0.5f, 0.5f), 100);
	}

	static public void SetLocalPlayer () {
		Player[] players = FindObjectsOfType<Player> ();
		foreach (Player player in players) {
			if (player.isLocalPlayer) {
				s_local_player = player;
			}
		}
	}

	// Hotkeys

	void OnMouseOver () {
		if (m_upright && Input.GetKeyDown("s")) {
			ChangeSize ();
		}

		if (Input.GetKeyDown("f")) {
			s_local_player.CmdFlip (this.gameObject);
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

	public void SetHeld (bool val) {
		m_held = val;
	}

	void OnMouseDown () {
		Debug.Log ("down");
		if (!m_held) {
			s_local_player.CmdGrab (this.gameObject);
			m_holder = true;
		}
	}

	void OnMouseUp () {
		Debug.Log ("up");
		if (m_holder) {
			s_local_player.CmdRelease (this.gameObject);
			m_holder = false;
		}
	}

	void OnMouseDrag () {
		if (m_holder) {
			s_local_player.CmdDrag (this.gameObject, Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, (Camera.main.transform.position.z * -1))));
		}
	}
}