using UnityEngine;
using System.Collections.Generic;

public struct ElevatorRequest {
	public int floor;
	public bool upOrDown; // 1 is up, 0 is down

	public ElevatorRequest(int newFloor, bool newUpOrDown) {
		floor = newFloor;
		upOrDown = newUpOrDown;
	}
}

public class Elevator : MonoBehaviour {
	public float speed;

	private Queue<ElevatorRequest> requests;
	private ElevatorRequest currentRequest;
	private int numRequests;
	private int wait;

	void Awake () {
		requests = new Queue<ElevatorRequest>();
		currentRequest = new ElevatorRequest(-1, false); // no request
		numRequests = 0;
		wait = 0;
	}

	void Update () {
		if (currentRequest.floor < 0) {
			if (numRequests > 0) {
				currentRequest = requests.Dequeue ();
				numRequests--;
				wait = 30; // wait for 30 frames
			}
		} else {
			if (wait > 0) {
				wait--;
				return;
			}

			float targetY = currentRequest.floor*2.0f - transform.parent.localPosition.y;

			// Move to request
			if (Mathf.Abs(targetY - transform.localPosition.y) <= speed) {
				transform.localPosition = new Vector3(transform.localPosition.x,
				                                      targetY,
				                                      transform.localPosition.z);
				currentRequest.floor = -1;
			} else {
				if (targetY > transform.localPosition.y) {
					transform.Translate(new Vector3(0.0f,speed,0.0f));
				} else
					transform.Translate(new Vector3(0.0f,-speed,0.0f));
			}
		}
	}

	public void addRequest(int floor, bool upOrDown) {
		requests.Enqueue (new ElevatorRequest (floor, upOrDown));
		numRequests++;
	}
}
