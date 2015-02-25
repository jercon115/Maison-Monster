using UnityEngine;
using System.Collections;

public class GameCamera : MonoBehaviour {

	public GameObject horizon;
	public int maxOrthoSize, minOrthoSize;
	bool minusDown = false, equalsDown = false;

	void Update () {
		// Camera movement
		int vert = 0, horiz = 0;
		if (Input.GetKey (KeyCode.W) || Input.GetKey (KeyCode.UpArrow))
			vert++;
		if (Input.GetKey (KeyCode.S) || Input.GetKey (KeyCode.DownArrow))
			vert--;
		if (Input.GetKey (KeyCode.D) || Input.GetKey (KeyCode.RightArrow)) {
			horiz++;
			Vector3 horizPos = horizon.transform.localPosition; horizPos.x += 0.07f;
			horizon.transform.localPosition = horizPos;
		}
		if (Input.GetKey (KeyCode.A) || Input.GetKey (KeyCode.LeftArrow)) {
			horiz--;
			Vector3 horizPos = horizon.transform.localPosition; horizPos.x -= 0.07f;
			horizon.transform.localPosition = horizPos;
		}
		transform.Translate (new Vector3(horiz*0.1f,vert*0.1f));

		// Camera zooming

		if (Input.GetKey (KeyCode.Minus)) {
		    if (!minusDown)  {
				minusDown = true;
				if (camera.orthographicSize <= maxOrthoSize)
					camera.orthographicSize++;
			}
		} else {
			minusDown = false;
		}
		if (Input.GetKey (KeyCode.Equals)) {
			if (!equalsDown)  {
				equalsDown = true;
				if (camera.orthographicSize >= minOrthoSize)
					camera.orthographicSize--;
			}
		} else {
			equalsDown = false;
		}
		if (Input.GetAxis ("Mouse ScrollWheel") < 0) {
			if (camera.orthographicSize <= maxOrthoSize)
				camera.orthographicSize++;
		}
		if (Input.GetAxis ("Mouse ScrollWheel") > 0) {
			if (camera.orthographicSize >= minOrthoSize)
				camera.orthographicSize--;
		}
	}
}
