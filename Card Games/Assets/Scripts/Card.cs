using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Card : NetworkBehaviour {

	static private Player s_local_player = null;

	public string m_name;
	public Sprite m_front;
	public Vector3 m_blowup_size;

	private Sprite m_back;
	[SyncVar]
	private bool m_held;
	[SyncVar]
	private bool m_upright;
	private bool m_holder;
	private bool m_blownup;
	private SpriteRenderer m_sprite_component;
	private Vector3 m_normal_size;

	public delegate void NoArgDelegate ();
	[SyncEvent]
	public event NoArgDelegate EventFlip;

	void Start () {
		EventFlip = new NoArgDelegate(FlipEvent);
		m_normal_size = new Vector3 (1, 1, 1);
		m_blowup_size = new Vector3 (3, 3, 1);
		m_back = Resources.Load<Sprite> ("Card_Back");
		m_upright = false;
		m_blownup = false;
		m_sprite_component = this.GetComponent<SpriteRenderer> ();
		m_sprite_component.sprite = m_back;
	}

	void OnConnectedToServer () {
		if (m_upright) {
			m_sprite_component.sprite = m_front;
		} else {
			m_sprite_component.sprite = m_back;
		}
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