using UnityEngine;
using System.Collections;

public class RCMButton : MonoBehaviour {

	public void DestroyRCM () {
		GameObject.Destroy(transform.parent.gameObject);
	}
}
