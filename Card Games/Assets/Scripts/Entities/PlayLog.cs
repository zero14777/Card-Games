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

		m_log.Callback = DoUpdateLog;
	}

	private SyncListString m_log = new SyncListString ();
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
		m_log.Add(log_event);
	}

	private void DoUpdateLog (SyncListString.Operation op, int index) {
		this.gameObject.GetComponent<Text> ().text = "";
		foreach (string line in m_log) {
			this.gameObject.GetComponent<Text> ().text += line;
			this.gameObject.GetComponent<Text> ().text += "\n";
		}
		StartCoroutine(MoveScrollBar ());
	}

	// keeps the scroll bar looking at the most recent event

	private IEnumerator MoveScrollBar () {
		yield return new WaitForSeconds(0.1f);
		m_scroll.value = 0;
	}
}
