using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;

public class SpawnCards : NetworkBehaviour {

	public GameObject card;

	void Start () {
		DirectoryInfo card_folder = new DirectoryInfo (Application.dataPath + "/../Card_Sprites");
		FileInfo[] png_files = card_folder.GetFiles ("*.png");
		//FileInfo[] jpg_files = card_folder.GetFiles ("*.jpg");

		foreach (FileInfo file in png_files) {
			GameObject new_card = (GameObject)Instantiate (card, transform.position, Quaternion.identity);
			var bytes = System.IO.File.ReadAllBytes (Application.dataPath + "/../Card_Sprites/" + file.Name);
			var tex = new Texture2D (1, 1);
			tex.LoadImage (bytes);
			Card card_component = new_card.GetComponent<Card> ();
			card_component.m_front = Sprite.Create (tex, new Rect (0, 0, tex.width, tex.height), new Vector2 (0.5f, 0.5f), 100);
			card_component.m_name = file.Name;
			NetworkServer.Spawn (new_card);
		}
	}
}
