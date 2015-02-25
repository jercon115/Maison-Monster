using UnityEngine;
using System.Collections.Generic;

public class ElevatorShaft : Shaft {
	public Elevator elevator;

	private int numElevators;
	private bool mouseDown;

	void Start() {
		elevator.transform.Translate (new Vector3(0.0f,0.0f,-1.0f));
		mouseDown = false;
	}

	void Update() {
		if (Input.GetMouseButtonDown (0)) {
			if (!mouseDown) {
				Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
				int x = Mathf.FloorToInt (mousePos.x/2.0f + 0.5f);
				int floor = Mathf.FloorToInt (mousePos.y/2.0f + 0.5f);
				if (x == cellX && floor >= cellY && floor < cellY + height) {
					elevator.addRequest (floor, true);
				}
			}
			mouseDown = true;
		} else
			mouseDown = false;
	}
}
