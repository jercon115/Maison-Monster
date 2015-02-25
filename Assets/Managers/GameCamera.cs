﻿using UnityEngine;
using System.Collections;

public class GameCamera : MonoBehaviour {

	bool minusDown = false, equalsDown = false;

	void Update () {
		// Camera movement
		int vert = 0, horiz = 0;
		if (Input.GetKey (KeyCode.W) || Input.GetKey (KeyCode.UpArrow))
			vert++;
		if (Input.GetKey (KeyCode.S) || Input.GetKey (KeyCode.DownArrow))
			vert--;
		if (Input.GetKey (KeyCode.D) || Input.GetKey (KeyCode.RightArrow))
			horiz++;
		if (Input.GetKey (KeyCode.A) || Input.GetKey (KeyCode.LeftArrow))
			horiz--;
		transform.Translate (new Vector3(horiz*0.1f,vert*0.1f));

		// Camera zooming

		if (Input.GetKey (KeyCode.Minus)) {
		    if (!minusDown)  {
				minusDown = true; camera.orthographicSize++;
			}
		} else {
			minusDown = false;
		}
		if (Input.GetKey (KeyCode.Equals)) {
			if (!equalsDown)  {
				equalsDown = true; camera.orthographicSize--;
			}
		} else {
			equalsDown = false;
		}
		if (Input.GetAxis ("Mouse ScrollWheel") < 0)
		    camera.orthographicSize++;
		if (Input.GetAxis("Mouse ScrollWheel") > 0)
		    camera.orthographicSize--;
	}
}
