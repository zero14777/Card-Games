using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Token : Draggable {

	public static GameObject CreateNewToken (float x_pos = 0, float y_pos = 0) {
		GameObject new_token = (GameObject)Instantiate (GameManager.Instance.m_token_prefab, new Vector3 (x_pos, y_pos, -0.05f), Quaternion.identity);
		NetworkServer.Spawn (new_token);
		return new_token;
	}

	private void OnMouseOver () {
		if (Input.GetKeyDown(KeyCode.Delete)) {
			Player.s_local_player.CmdDeleteToken (this.gameObject);
		}
	}
}
