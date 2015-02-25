using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Ground : MonoBehaviour {

	public GameObject groundSprite;
	private List<GameObject> groundSprites;
	private float cameraX, oldX, newX;

	// Use this for initialization
	void Start () {
		float worldScreenHeight = Camera.main.orthographicSize * 2.0f;
		float worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;

		transform.localScale = new Vector3 (40.0f, 20.0f, 1.0f);
		cameraX = Mathf.FloorToInt(Camera.main.transform.localPosition.x);
		newX = cameraX + cameraX % 2; oldX = newX;
		transform.localPosition = new Vector3(cameraX + cameraX%2,
		                                      -11.0f,
		                                      250.0f);
		groundSprites = new List<GameObject>();
		for (int i = 0; i < Mathf.FloorToInt(transform.localScale.x/2); i++) {
			float leftX = transform.localPosition.x - transform.localScale.x/2 - 1.0f;
			Vector3 groundPos =
				new Vector3 (leftX + i*2 + 1.0f, -1.0f, transform.localPosition.z-10.0f);
			groundSprites.Add(Instantiate (groundSprite, groundPos, Quaternion.identity) as GameObject);
		}
	}
	
	// Update is called once per frame
	void Update () {
		cameraX = Mathf.FloorToInt(Camera.main.transform.localPosition.x);
		newX = cameraX + cameraX % 2;

		transform.localPosition = new Vector3(newX,
		                                      -11.0f,
		                                      250.0f);

		if (newX != oldX) {
			if (newX < oldX) {
				GameObject ground = groundSprites[groundSprites.Count-1];
				Vector3 newPos =
					new Vector3 (ground.transform.localPosition.x - groundSprites.Count*2,
					             -1.0f, ground.transform.localPosition.z);
				groundSprites.RemoveAt(groundSprites.Count-1);
				Destroy (ground.gameObject);
				groundSprites.Insert(0, Instantiate (groundSprite, newPos, Quaternion.identity) as GameObject);
			} else if (newX > oldX) {
				GameObject ground = groundSprites[0];
				Vector3 newPos =
					new Vector3 (ground.transform.localPosition.x + groundSprites.Count*2,
					             -1.0f, ground.transform.localPosition.z);
				groundSprites.RemoveAt(0);
				Destroy (ground.gameObject);
				groundSprites.Add(Instantiate (groundSprite, newPos, Quaternion.identity) as GameObject);
			}
			oldX = newX;
		}
	}
}
