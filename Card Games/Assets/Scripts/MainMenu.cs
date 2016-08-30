using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MainMenu : MonoBehaviour {

	public static string m_player_name = "Player";

	public void SetPlayerName () {
		m_player_name = GameObject.Find ("Player Name").GetComponent<InputField> ().text;
		Debug.Log (m_player_name);
	}
}
