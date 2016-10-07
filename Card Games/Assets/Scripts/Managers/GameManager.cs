using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

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

	// Set Values and Game Setup

	public GameObject card_spawner_prefab;
	public GameObject card_spawner_menu;

	void Start () {
		m_instance = this;
		m_open_menu = null;
		m_over_UI = false;

		DirectoryInfo card_folder = new DirectoryInfo (Application.dataPath + "/../Cards");
		FileInfo[] png_files = card_folder.GetFiles ("*.png"); // can use jpg files too

		foreach (FileInfo file in png_files) {
			GameObject new_card_spawner = Instantiate (card_spawner_prefab);
			new_card_spawner.transform.SetParent(card_spawner_menu.transform);
			byte[] bytes = System.IO.File.ReadAllBytes (Application.dataPath + "/../Cards/" + file.Name);
			new_card_spawner.GetComponent<Image> ().sprite = Card.GenerateSprite (bytes);
			new_card_spawner.GetComponent<CardSpawner> ().m_card = file.Name;
		}

		if (MainMenu.m_load) {
			LoadGame ();
		}
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

	// Disconnect

	public void DisconnectGame () {
		NetworkManager.singleton.StopHost ();
	}

	// Save Game State

	public void SaveGame () {
		Save newsave = new Save ();

		newsave.m_decks = new List<DeckSave> ();
		Deck[] decks = FindObjectsOfType(typeof(Deck)) as Deck[];
		foreach (Deck deck in decks) {
			DeckSave temp = new DeckSave ();
			temp.m_name = deck.m_name;
			temp.m_deck = new List<string> ();
			foreach (string card in deck.m_deck) {
				temp.m_deck.Add (card);
			}
			temp.m_x_pos = deck.transform.position.x;
			temp.m_y_pos = deck.transform.position.y;
			//save rotation
			newsave.m_decks.Add (temp);
		}

		newsave.m_cards = new List<CardSave> ();
		Card[] cards = FindObjectsOfType(typeof(Card)) as Card[];
		foreach (Card card in cards) {
			CardSave temp = new CardSave ();
			temp.m_filename = card.m_filename;
			temp.m_upright = card.m_upright;
			temp.m_x_pos = card.transform.position.x;
			temp.m_y_pos = card.transform.position.y;
			temp.m_rotation = card.m_rotation;
			newsave.m_cards.Add (temp);
		}

		FileStream file = File.Create (Application.dataPath + "/Save");
		BinaryFormatter bf = new BinaryFormatter ();
		bf.Serialize (file, newsave);
		file.Close ();
	}

	// Load Game State

	public void LoadGame () {
		try {
			FileStream file = File.Open (Application.dataPath + "/Save", FileMode.Open);
			BinaryFormatter bf = new BinaryFormatter ();
			Save load = (Save)bf.Deserialize(file);
			file.Close ();

			foreach (DeckSave deck in load.m_decks) {
				GameObject temp = Deck.CreateNewDeck (deck.m_name, deck.m_x_pos, deck.m_y_pos);
				Deck temp_deck = temp.GetComponent<Deck> ();
				foreach (string card in deck.m_deck) {
					temp_deck.m_deck.Add(card);
				}
			}

			foreach (CardSave card in load.m_cards) {
				Card.CreateNewCard (card.m_filename, card.m_upright, card.m_x_pos, card.m_y_pos, card.m_rotation);
			}
		} catch (FileNotFoundException) {
			throw new FileNotFoundException ("File: " + Application.dataPath + "/Save not found.");
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
