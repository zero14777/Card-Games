﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class MainMenu : MonoBehaviour {

	public static string m_player_name = "Player";
	public static bool m_load = false;

	public void SetPlayerName () {
		m_player_name = GameObject.Find ("Player Name").GetComponent<InputField> ().text;
	}

	public void ConnectToHost () {
		NetworkManager.singleton.networkAddress = GameObject.Find ("Client IP").GetComponent<InputField> ().text;
		NetworkManager.singleton.StartClient ();
	}

	public void HostGame () {
		NetworkManager.singleton.StartHost ();
	}

	public void LoadGame () {
		m_load = true;
		NetworkManager.singleton.StartHost ();
	}
}
