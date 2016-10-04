using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class PlayLog : NetworkBehaviour {

	// Singleton

	private static PlayLog m_instance;
	public static PlayLog Instance {
		get { return m_instance; }
	}

	void Start () {
		m_instance = this;
	}
		
	[SyncVar(hook = "OnUpdateLog")]
	private string m_log = "";
	public InputField m_input_text;
	public Scrollbar m_scroll;

	public void SendChatMsg () {
		if (m_input_text.text != "") {
			Player.s_local_player.CmdSendMessage (m_input_text.text);
		}
		m_input_text.text = "";
	}

	/// <summary>
	/// Updates the game log with a new string. Must be
	/// called from a player command.
	/// </summary>
	public void LogEvent (string log_event) {
		if (!isServer) {
			return;
		}
		if (m_log != "") {
			m_log += "\n";
		}
		m_log += log_event;
	}

	public void OnUpdateLog (string log) {
		this.gameObject.GetComponent<Text> ().text = log;
		StartCoroutine(MoveScrollBar ());
	}

	// keeps the scroll bar looking at the most recent event

	public IEnumerator MoveScrollBar () {
		yield return new WaitForSeconds(0.1f);
		m_scroll.value = 0;
	}
}
