using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class OverUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

	public virtual void OnPointerEnter (PointerEventData event_data) {
		GameManager.Instance.m_over_UI = true;
		Board.DisableDrag ();
	}

	public virtual void OnPointerExit (PointerEventData event_data) {
		GameManager.Instance.m_over_UI = false;
		Board.EnableDrag ();
	}
}
