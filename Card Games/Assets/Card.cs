using UnityEngine;
using System.Collections;

public class Card : MonoBehaviour {

	public Sprite front;
	public Vector3 blowup_size;

	private Sprite back;
	private bool upright;
	private SpriteRenderer sprite_component;
	private Vector3 normal_size;
	private Vector3 oldPoint;

	void Start() {
		normal_size = new Vector3 (1, 1, 1);
		blowup_size = new Vector3 (3, 3, 1);
		back = Resources.Load<Sprite> ("Card_Back");
		upright = true;
		sprite_component = this.GetComponent<SpriteRenderer> ();
		sprite_component.sprite = back;
	}

	void OnMouseOver () {
		if (upright) {
			transform.localScale = blowup_size;
		}
	}

	void OnMouseExit () {
		transform.localScale = normal_size;
	}

	void OnMouseDown () {
		Flip ();
		oldPoint = Camera.main.ScreenToViewportPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
	}

	void OnMouseDrag () {
		Vector3 currentPoint = Camera.main.ScreenToViewportPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
		Vector3 positionChange = currentPoint - oldPoint;
		oldPoint = currentPoint;
		transform.position = transform.position + positionChange;
		Debug.Log (currentPoint);
	}

	void Flip () {
		if (upright) {
			upright = false;
			sprite_component.sprite = back;
		} else {
			upright = true;
			sprite_component.sprite = front;
		}
	}
}