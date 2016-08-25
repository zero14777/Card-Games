using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;

	/* <summary>
	 * Spawns Cards based on images in the Card_Sprites directory. All players need
	 * the same images and image names as the host.
	 * </summary>*/
public class SpawnCards : NetworkBehaviour {

	public GameObject card_prefab;
	public GameObject deck_obj;

	[ServerCallback]
	void Start () {
		Deck deck = deck_obj.GetComponent<Deck> ();
		DirectoryInfo card_folder = new DirectoryInfo (Application.dataPath + "/../Cards");
		FileInfo[] png_files = card_folder.GetFiles ("*.png"); // can use jpg files too

		foreach (FileInfo file in png_files) {
			deck.AddCard (file.Name);
			deck.AddCard (file.Name);
			deck.AddCard (file.Name);
			//Card.CreateNewCard (file.Name, transform.position, card_prefab);
		}
	}
}
