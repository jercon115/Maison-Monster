using UnityEngine;

public class Room : MonoBehaviour {
	public int width, height;

	public virtual void Destroy() {
		Destroy (gameObject);
	}
}