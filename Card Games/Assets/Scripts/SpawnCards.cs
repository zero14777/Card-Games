﻿using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;

	/* <summary>
	 * Spawns Cards based on images in the Card_Sprites directory. All players need
	 * the same images and image names as the host.
	 * </summary>*/
public class SpawnCards : NetworkBehaviour {

	public GameObject card;

	void Start () {
		DirectoryInfo card_folder = new DirectoryInfo (Application.dataPath + "/../Card_Sprites");
		FileInfo[] png_files = card_folder.GetFiles ("*.png"); // can use jpg files too

		foreach (FileInfo file in png_files) {
			GameObject new_card = (GameObject)Instantiate (card, transform.position, Quaternion.identity);
			Card card_component = new_card.GetComponent<Card> ();
			card_component.m_filename = file.Name;
			card_component.LoadFront ();
			NetworkServer.Spawn (new_card);
		}
	}
}
