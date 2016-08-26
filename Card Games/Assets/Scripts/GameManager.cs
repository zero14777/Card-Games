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
		m_EMPTY.ID = -1;
	}

	// Keep track of players

	public Text m_players_list;
	[SyncVar]
	public int m_id_counter = 0;
	public struct Player_Data {
		public int ID;
		public string player_name;
		public int hand_count;

		public Player_Data (int id, string name, int count) {
			this.ID = id;
			this.player_name = name;
			this.hand_count = count;
		}
	}
	private Player_Data m_EMPTY = new Player_Data ();
	public class SyncListPlayer_Data : SyncListStruct<Player_Data> {}
	public SyncListPlayer_Data m_players = new SyncListPlayer_Data ();

	[ClientRpc]
	private void RpcUpdatePlayersList () {
		m_players_list.text = "Players:\n";
		foreach (Player_Data data in m_players) {
			m_players_list.text += data.player_name;
			m_players_list.text += " - Hand ";
			m_players_list.text += data.hand_count;
			m_players_list.text += "\n";
		}
	}

	public int AddPlayer (GameObject player_obj) {
		Player player = player_obj.GetComponent<Player> ();
		Player_Data new_player = new Player_Data (++m_id_counter, player.m_player_name,
													player.m_hand.Count);
		//new_player.player_name = player.m_player_name;
		//new_player.ID = ++m_id_counter;
		m_players.Add (new_player);
		RpcUpdatePlayersList ();
		return new_player.ID;
	}

	/*private Player_Data GetPlayer (int player_id) {
		for (int iter = 0; iter < m_players.Count; iter++) {
			if (m_players [iter].ID == player_id) {
				return m_players [iter];
			}
		}
		return m_EMPTY;
	}*/

	public void UpdateHandCount (int player_id, int hand_count) {
		for (int iter = 0; iter < m_players.Count; iter++) {
			if (m_players [iter].ID == player_id) {
				m_players [iter] = new Player_Data (m_players [iter].ID, 
									m_players [iter].player_name, hand_count);
			}
		}
		RpcUpdatePlayersList ();
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
