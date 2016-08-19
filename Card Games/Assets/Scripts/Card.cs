using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Card : NetworkBehaviour {

	static private Player local_player = null;

	public Sprite front;
	public Vector3 blowup_size;

	private Sprite back;
	[SyncVar]
	private float x;
	[SyncVar]
	private float y;
	[SyncVar]
	private bool held;
	private bool holder;
	[SyncVar]
	private bool upright;
	private bool blownup;
	private SpriteRenderer sprite_component;
	private Vector3 normal_size;

	public delegate void NoArgDelegate ();
	[SyncEvent]
	public event NoArgDelegate EventFlip;

	void Start () {
		EventFlip = new NoArgDelegate(FlipEvent);
		normal_size = new Vector3 (1, 1, 1);
		blowup_size = new Vector3 (3, 3, 1);
		back = Resources.Load<Sprite> ("Card_Back");
		upright = false;
		blownup = false;
		sprite_component = this.GetComponent<SpriteRenderer> ();
		sprite_component.sprite = back;
	}

	void OnConnectedToServer () {
		if (upright) {
			sprite_component.sprite = front;
		} else {
			sprite_component.sprite = back;
		}
	}

	static public void SetLocalPlayer () {
		Player[] players = FindObjectsOfType<Player> ();
		foreach (Player player in players) {
			if (player.isLocalPlayer) {
				local_player = player;
			}
		}
	}

	// Hotkeys

	void OnMouseOver () {
		if (upright && Input.GetKeyDown("s")) {
			ChangeSize ();
		}

		if (Input.GetKeyDown("f")) {
			local_player.CmdFlip (this.gameObject);
		}
	}

	// Card Zoom

	private void Blowup () {
		transform.localScale = blowup_size;
		blownup = true;
	}

	private void Shrink () {
		transform.localScale = normal_size;
		blownup = false;
	}

	private void ChangeSize () {
		if (blownup) {
			Shrink ();
		} else {
			Blowup ();
		}
	}

	// Card Flipping

	public void FlipUp () {
		upright = true;
		sprite_component.sprite = front;
	}

	public void FlipDown () {
		upright = false;
		sprite_component.sprite = back;
		Shrink ();
	}

	public void FlipEvent () {
		if (upright) {
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
		held = val;
	}

	void OnMouseDown () {
		Debug.Log ("down");
		if (!held) {
			local_player.CmdGrab (this.gameObject);
			holder = true;
		}
	}

	void OnMouseUp () {
		Debug.Log ("up");
		if (holder) {
			local_player.CmdRelease (this.gameObject);
			holder = false;
		}
	}

	void OnMouseDrag () {
		if (holder) {
			local_player.CmdDrag (this.gameObject, Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 10)));
		}
	}
}