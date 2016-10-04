using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class GameMenu : NetworkBehaviour {
	public void DisconnectGame () {
		NetworkManager.singleton.StopHost ();
	}
}
