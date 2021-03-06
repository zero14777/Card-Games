﻿using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class RightClickMenu : UIElement, IPointerEnterHandler, IPointerExitHandler {

	public bool m_over;

	void Start () {
		m_over = false;
	}

	void Update () {
		if (!m_over && Input.GetMouseButton (0)) {
			GameObject.Destroy (this.gameObject);
		}
	}

	public override void OnPointerEnter (PointerEventData event_data) {
		m_over = true;
		GameManager.Instance.m_over_UI = true;
	}

	public override void OnPointerExit (PointerEventData event_data) {
		m_over = false;
		GameManager.Instance.m_over_UI = false;
	}
}
