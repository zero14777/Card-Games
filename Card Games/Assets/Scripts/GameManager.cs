using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;

public class GameManager : NetworkBehaviour {

	// Singleton

	private static GameManager m_instance;
	public static GameManager Instance {
		get { return m_instance; }
	}

	// Set Values

	void Start () {
		m_instance = this;
	}

	// Keep track of players

	public Text m_players_list;

	[ClientRpc]
	public void RpcUpdatePlayersList () {
		Player[] players = FindObjectsOfType<Player> ();
		m_players_list.text = "Players:\n";
		foreach (Player player in players) {
			m_players_list.text += player.m_player_name;
			m_players_list.text += " - Hand ";
			m_players_list.text += player.m_hand.Count;
			m_players_list.text += "\n";
		}
	}

	// Hover Text for mousing over objects

	public Text m_hover_text;

	public void SetHoverText (string text) {
		m_hover_text.text = text;
	}

	public void MoveHoverText () {
		m_hover_text.transform.position = Input.mousePosition;
	}
}
