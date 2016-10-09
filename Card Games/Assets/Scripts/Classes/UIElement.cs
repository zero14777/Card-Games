using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class UIElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

	public virtual void OnPointerEnter (PointerEventData event_data) {
		GameManager.Instance.m_over_UI = true;
	}

	public virtual void OnPointerExit (PointerEventData event_data) {
		GameManager.Instance.m_over_UI = false;
	}

	void OnDestroy () {
		GameManager.Instance.m_over_UI = false;
	}
}
