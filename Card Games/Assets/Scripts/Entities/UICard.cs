using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class UICard : UIElement, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {

	private RectTransform m_rectTransform;

	public string m_name;

	void Start () {
		m_rectTransform = (RectTransform)transform;
	}

	// Mouse Over

	public override void OnPointerEnter (PointerEventData event_data) {
		m_rectTransform.position = new Vector3 (m_rectTransform.position.x, 0, 0);
		GameManager.Instance.m_over_UI = true;
	}

	public override void OnPointerExit (PointerEventData event_data) {
		m_rectTransform.position = new Vector3 (m_rectTransform.position.x, -(m_rectTransform.rect.height/2), 0);
		GameManager.Instance.m_over_UI = false;
	}

	// Drag and Drop

	public void OnBeginDrag (PointerEventData event_data) {
		Board.DisableDrag ();
	}

	public void OnDrag (PointerEventData event_data) {
		this.transform.position = event_data.position;
	}

	public void OnEndDrag (PointerEventData event_data) {
		Board.EnableDrag ();

		bool deck_found = false;

		RaycastHit2D[] hits = Physics2D.RaycastAll (Camera.main.ScreenToWorldPoint (
													new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 
													(Camera.main.transform.position.z * -1))),
													Vector2.zero);
		foreach (RaycastHit2D hit in hits) {
			if (hit.transform.gameObject.GetComponent<Deck> () != null) {
				Player.s_local_player.CmdPlaceOnDeck (m_name, this.gameObject, hit.transform.gameObject);
				deck_found = true;
				return;
			}
		}
		if (!deck_found) {
			Player.s_local_player.CmdDropFromHand (m_name, Camera.main.ScreenToWorldPoint (
													new Vector3 (Input.mousePosition.x, Input.mousePosition.y,
													(Camera.main.transform.position.z * -1))),
													Board.s_rotation);
		}

		GameObject.Destroy (this.gameObject);
	}

	void OnDestroy () {
		GameManager.Instance.m_over_UI = false;
	}
}
