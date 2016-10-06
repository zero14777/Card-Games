using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class DeckSave {
	public string m_name;
	public List<string> m_deck;
	public float m_x_pos;
	public float m_y_pos;
	public float m_rotation;
}

[System.Serializable]
public class CardSave {
	public string m_filename;
	public bool m_upright;
	public float m_x_pos;
	public float m_y_pos;
	public float m_rotation;
}

[System.Serializable]
public class Save {
	public List<DeckSave> m_decks;
	public List<CardSave> m_cards;
}
