using UnityEngine;
using System.Collections;

public class ChangeScoreButton : MonoBehaviour {
	public void ChangeScore (int change) {
		Player.s_local_player.CmdChangePlayerScore (this.transform.parent.gameObject, change);
	}
}
