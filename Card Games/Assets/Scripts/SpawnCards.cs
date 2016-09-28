using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;

	/* <summary>
	 * Spawns Cards based on images in the Card_Sprites directory. All players need
	 * the same images and image names as the host.
	 * </summary>*/
public class SpawnCards : MonoBehaviour {

	public GameObject card_spawner_prefab;
	public GameObject card_spawner_menu;

	void Start () {
		DirectoryInfo card_folder = new DirectoryInfo (Application.dataPath + "/../Cards");
		FileInfo[] png_files = card_folder.GetFiles ("*.png"); // can use jpg files too

		foreach (FileInfo file in png_files) {
			GameObject new_card_spawner = Instantiate (card_spawner_prefab);
			new_card_spawner.transform.SetParent(card_spawner_menu.transform);
			byte[] bytes = System.IO.File.ReadAllBytes (Application.dataPath + "/../Cards/" + file.Name);
			new_card_spawner.GetComponent<Image> ().sprite = Card.GenerateSprite (bytes);
			new_card_spawner.GetComponent<CardSpawner> ().m_card = file.Name;
		}
	}
}
