using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class Tuple<Type1, Type2> {
	public Type1 Name { get; set;}
	public Type2 Function { get; set;}
	internal Tuple(Type1 t1, Type2 t2) {
		Name = t1;
		Function = t2;
	}
}

public class GameManager : NetworkBehaviour {

	// Singleton

	private static GameManager m_instance;
	public static GameManager Instance {
		get { return m_instance; }
	}

	// Used to check if the curser is currently over UI
	public bool m_over_UI;

	// Prefabs
	public GameObject m_card_prefab;
	public GameObject m_deck_prefab;

	// Set Values

	void Start () {
		m_instance = this;
		m_open_menu = null;
		m_over_UI = false;
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

	// Right Click Menus

	public GameObject m_RCMenu_prefab;
	public GameObject m_button_obj;
	private GameObject m_open_menu;

	public void RightClickMenu (List<Tuple<string, UnityEngine.Events.UnityAction>> functions) {
		if (m_open_menu) {
			GameObject.Destroy (m_open_menu);
		}
		GameObject tempcanvas = GameObject.Find ("Canvas");
		m_open_menu = Instantiate (m_RCMenu_prefab);
		m_open_menu.transform.SetParent (tempcanvas.transform);
		m_open_menu.transform.position = Input.mousePosition;
		((RectTransform)(m_open_menu.transform)).sizeDelta = new Vector2 (200, functions.Count * 40);
		foreach (Tuple<string, UnityEngine.Events.UnityAction> function in functions) {
			GameObject button = Instantiate (m_button_obj);
			button.transform.SetParent (m_open_menu.transform);
			button.GetComponent<Button> ().onClick.AddListener (function.Function);
			button.transform.FindChild ("Text").gameObject.GetComponent<Text> ().text = function.Name;
		}
	}

	// Search Addable Cards

	public InputField m_search_input_field;
	public Transform m_Card_List;

	public void CardSearch () {
		string asdf = "";
		foreach (Transform child in m_Card_List)
		{
			if (child.gameObject.GetComponent<CardSpawner> ().m_card.Contains (m_search_input_field.text)) {
				child.gameObject.SetActive (true);
				asdf = asdf + child.gameObject.GetComponent<CardSpawner> ().m_card;
			} else {
				child.gameObject.SetActive (false);
			}
		}
		Debug.Log (asdf);
	}
}
