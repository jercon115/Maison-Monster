using UnityEngine;
using System.Collections.Generic;

public class ElevatorShaft : Shaft {
	public GameObject elevatorPrefab;

	private int numElevators;

	private Elevator elevator;

	void Awake() {
		elevator = Instantiate (elevatorPrefab, transform.localPosition, Quaternion.identity) as Elevator;
		elevator.transform.parent = transform;
	}
}
