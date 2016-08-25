using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LocalGameManager : MonoBehaviour {

	// Singleton

	private static LocalGameManager m_instance;
	public static LocalGameManager Instance {
		get { return m_instance; }
	}

	// Set Values

	void Start () {
		m_instance = this;
		//m_hover_text = GameObject.Find ("Hover Text").GetComponent<Text> ();
	}

	// Hover Text for mousing over objects

	public Text m_hover_text;

	public void SetHoverText (string text) {
		m_hover_text.text = text;
	}

	public void MoveHoverText () {
		m_hover_text.transform.position = Input.mousePosition;
	}
}
