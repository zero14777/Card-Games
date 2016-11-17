﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class CardSpawner : UIElement, IPointerClickHandler {

	public string m_card; 

	public void OnPointerClick (PointerEventData event_data) {
		Player.s_local_player.CmdCreateCard (m_card, Board.s_rotation);
	}
}
