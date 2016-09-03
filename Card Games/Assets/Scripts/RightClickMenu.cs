using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class RightClickMenu : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

	public bool m_over;

	void Start () {
		m_over = false;
	}

	void Update () {
		if (!m_over && Input.GetMouseButton (0)) {
			GameObject.Destroy (this.gameObject);
		}
	}

	public void OnPointerEnter (PointerEventData event_data) {
		m_over = true;
	}

	public void OnPointerExit (PointerEventData event_data) {
		m_over = false;
	}
}
