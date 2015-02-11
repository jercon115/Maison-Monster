using UnityEngine;
using System.Collections;

public class Guidelines : MonoBehaviour {
	
	public void setSize(int width, int height) {
		transform.localScale = new Vector3(width * 2.0f, height * 2.0f, 1.0f);
		transform.Translate (width * 1.0f - 1.0f , height * 1.0f - 1.0f, 20.0f);
		renderer.material.mainTextureScale = new Vector3(width * 1.0f, height * 1.0f);
	}
}
