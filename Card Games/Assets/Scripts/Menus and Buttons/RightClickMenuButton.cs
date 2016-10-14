using UnityEngine;

public class RightClickMenuButton : MonoBehaviour {
	public void DestroyRightClickMenu () {
		GameObject.Destroy(transform.parent.gameObject);
	}
}
