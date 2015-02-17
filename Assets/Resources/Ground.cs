using UnityEngine;
using System.Collections;

public class Ground : MonoBehaviour {

	// Use this for initialization
	void Start () {
		float worldScreenHeight = Camera.main.orthographicSize * 2.0f;
		float worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;

		transform.localScale = new Vector3 (worldScreenWidth * 2.0f, 20.0f, 1.0f);
		transform.localPosition = new Vector3(Camera.main.transform.localPosition.x,
		                                      -11.0f,
		                                      250.0f);
	}
	
	// Update is called once per frame
	void Update () {
		transform.localPosition = new Vector3(Camera.main.transform.localPosition.x,
		                                      -11.0f,
		                                      250.0f);
	}
}
