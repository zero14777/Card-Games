using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class UICard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {

	private RectTransform m_rectTransform;

	public string m_name;

	void Start () {
		m_rectTransform = (RectTransform)transform;
	}

	// Mouse Over

	public void OnPointerEnter (PointerEventData event_data) {
		m_rectTransform.position = new Vector3 (m_rectTransform.position.x, 0, 0);
		GameManager.Instance.m_over_UI = true;
	}

	public void OnPointerExit (PointerEventData event_data) {
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
		Player.s_local_player.CmdDropFromHand (m_name, Camera.main.ScreenToWorldPoint (
												new Vector3 (Input.mousePosition.x, Input.mousePosition.y,
												(Camera.main.transform.position.z * -1))),
												Board.s_rotation);
		GameObject.Destroy (this.gameObject);
	}

	void OnDestroy () {
		GameManager.Instance.m_over_UI = false;
	}
}
