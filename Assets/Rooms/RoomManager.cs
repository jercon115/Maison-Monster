using UnityEngine;
using System;
using System.Collections.Generic;

public class RoomManager : MonoBehaviour {
	
	public Hotel hotel;
	public Room[,] cells;
	public Matchmaker matchmaker;

	public List<Room> lobbies;
	public List<Shaft> shafts;

	// Use this for initialization
	public void Start () {
		cells = new Room[hotel.width, hotel.height];
		lobbies = new List<Room> ();

		path = null; // TEST STACK FOR PATH
	}

	public void MakeRoom(int x, int y, Room newroom) {
		if (roomLocationValid (x, y, newroom.width, newroom.height)) {
			Room tmpRoom = Instantiate (newroom) as Room;
			tmpRoom.Construct (this, x, y);
			switch (tmpRoom.room_type) {
			case "lobby":
				lobbies.Add (tmpRoom);
				break;
			case "misc":
				break;
			default:
				matchmaker.matchRoom (tmpRoom);
				break;
			}
		}
	}

	public void DeleteRoom(int x, int y) {
		if (cells [x, y] != null && cells [x, y].monsters.Count == 0) {
			if (cells [x, y].room_type == "lobby") lobbies.Remove (cells [x, y]);
			cells[x, y].DemolishCell(x, y);

			print ("Destroyed");
		}
	}

	public bool roomLocationValid(int x, int y, int width, int height) {
		if (x < 0 || x >= hotel.width-(width-1) || y < 0 || y >= hotel.height-(height-1))
			return false;

		bool foundation_exists = false;
		for(int i = 0; i < width; i++) {
			for(int j = 0; j < height; j++) {
				if (cells[x + i, y + j] != null) return false;
				if (y > 0) {
					if (cells[x + i, y - 1] != null)
						foundation_exists = true;
				} else if (y == 0)
					foundation_exists = true;
			}
		}
		if (!foundation_exists) return false;
		return true;
	}

	private Room previousHighlightedRoom = null;

	public void highlightRoomAt(int x, int y) {
		Room targetRoom = cells[x, y];
		if (targetRoom != null) {
			if (targetRoom != previousHighlightedRoom) {
				if (previousHighlightedRoom != null)
					unhighlightPrevRoom ();
				targetRoom.highlightSprite (new Color32 (255, 0, 0, 255), true);
				previousHighlightedRoom = targetRoom;
			}
		} else {
			unhighlightPrevRoom();
		}
	}

	public void unhighlightPrevRoom() {
		if (previousHighlightedRoom != null) {
			previousHighlightedRoom.highlightSprite(new Color (1f, 1f, 1f, 1f), false);
			previousHighlightedRoom = null;
		}
	}

	public Room getRoomAt(int x, int y) {
		return cells[x, y];
	}
	
	public bool roomExistsAt(int x, int y) {
		if (x < 0 || x > cells.GetLength (0) - 1 ||
		    y < 0 || y > cells.GetLength (1) - 1)
			return false;
		if (getRoomAt (x, y) == null)
						return false;
		return true;
	}

	public Room getNearestLobby (float xPos) {
		int cellX = Mathf.FloorToInt (xPos / 2.0f + 0.5f);

		int minDist = int.MaxValue; Room nearestLobby = null;
		for (int i = 0; i < lobbies.Count; i++) {
			int dist = Math.Abs (lobbies[i].cellX - cellX);
			if (dist < minDist) {
				minDist = dist; nearestLobby = lobbies[i];
			}
		}

		return nearestLobby;
	}

	// Updates the reachable ranges of shafts in the corresponding floors
	// as well as returns a queue of shafts that were affected (the leftmost
	// and rightmost values changed)
	public Queue<Shaft> updateShaftsReachableRanges(int y1, int y2) {
		// Queue for shafts whose reachable ranges were changed
		// and thus may need to update connections, will return them
		Queue<Shaft> returnShafts = new Queue<Shaft> ();

		// Queue to hold shafts found that connects to current floor
		// and whose reachable ranges may need to be updated
		Queue<Shaft> foundShafts = new Queue<Shaft>();

		// Two integers to store leftmost and rightmost cells of current cluster
		int leftmost = -1, rightmost = -1;

		for (int j = y1; j <= y2; j++) {
			for(int i = 0; i < hotel.width; i++) {
				// Ground floor all reachable
				if (j == 0) {
					if (cells[i,j] is Shaft) {
						Shaft shaft = (Shaft)cells[i,j];
						int index = j-shaft.cellY;

						shaft.leftmostCells[index] = 0;
						shaft.rightmostCells[index] = hotel.width-1;

						if (!returnShafts.Contains (shaft))
							returnShafts.Enqueue (shaft); // Check connections no matter what
					}

					continue;
				}

				if (cells[i, j] != null) {
					if (cells[i, j] is Shaft)
						foundShafts.Enqueue ((Shaft)cells[i,j]);

					if (leftmost == -1) {
						leftmost = i; rightmost = i;
					} else
						rightmost++;
				}

				if (cells[i,j] == null || i == hotel.width-1) {
					while(foundShafts.Count > 0) {
						Shaft shaft = foundShafts.Dequeue();
						int index = j-shaft.cellY;

						// Check if reachable ranges changed => shaft was affected
						if(shaft.leftmostCells[index] != leftmost || shaft.rightmostCells[index] != rightmost) {
							// Add the shaft to list of affected shafts, they will need to update connections
							if (!returnShafts.Contains (shaft))
								returnShafts.Enqueue (shaft);

							// Update shaft's reachable ranges
							shaft.leftmostCells[index] = leftmost;
							shaft.rightmostCells[index] = rightmost;
						}
					}

					// reset leftmost found cell and rightmost found cell (new cluster of cells)
					leftmost = -1; rightmost = -1;
				}
			}
		}

		return returnShafts;
	}

	public void updateShaftsConnections(Queue<Shaft> updatedShafts) {
		// Update connections
		Queue<Shaft> otherShafts; // Queue to hold other shafts
		while(updatedShafts.Count > 0) {
			Shaft shaft = updatedShafts.Dequeue ();
			otherShafts = new Queue<Shaft>(updatedShafts);
			
			// check connections with other shafts
			while(otherShafts.Count > 0)
				shaft.checkAndUpdateConnection (otherShafts.Dequeue ());
		}
	}

	public List<Shaft> getReachableShaftsAt(int x, int y) {
		List<Shaft> returnShafts = new List<Shaft>();

		// Check left first
		int i = x;
		while(i >= 0 && (cells[i, y] != null || y == 0)) {
			if (cells[i, y] is Shaft) returnShafts.Add ((Shaft)cells[i,y]);
			i--;
		}
		// Check right
		i = x+1;
		while(i < hotel.width && (cells[i, y] != null || y == 0)) {
			if (cells[i, y] is Shaft) returnShafts.Add ((Shaft)cells[i,y]);
			i++;
		}

		return returnShafts;
	}

	public void calculateShaftDistances(int x, int y) {
		// Outside hotel?
		if (x < 0 || x >= hotel.width || y < 0 || y > hotel.width)
			return;

		// Pointing to empty cell above ground level?
		if (cells [x, y] == null && y > 0)
			return;

		// Find shafts accessible from starting position
		List<Shaft> startShafts = getReachableShaftsAt (x, y);

		// Initialize all of the hotel's shafts distances from target by setting them to -1
		foreach(Shaft shaft in shafts) shaft.initDistancesFromStart ();

		// Calculate distances from target
		foreach(Shaft shaft in startShafts) shaft.calculateDistancesFromStart (-1);
	}

	public Stack<Shaft> createPathFromShaftDistances(int x, int y) {
		Stack<Shaft> path = new Stack<Shaft>();

		// Find shafts accessible from starting position
		Queue<Shaft> checkShafts = new Queue<Shaft>(getReachableShaftsAt(x, y));

		int dist = int.MaxValue; Shaft nextShaft = null;

		// Go through each shaft in queue and find one with minimum distance
		while (checkShafts.Count > 0) {
			Shaft checkShaft = checkShafts.Dequeue ();

			// If ditance is less than current minimum distance found so far, update
			// minimum distance found so far and also the the next shaft to take
			if (checkShaft.distFromStart < dist && checkShaft.distFromStart > -1) {
				dist = checkShaft.distFromStart;
				nextShaft = checkShaft;
			}

			// If the queue is empty, then we've checked all shafts, add onto path
			// and get the next set of connected shafts if we haven't reached the end
			// Also check if the minimum found distance is zero, which means we can stop
			if (checkShafts.Count == 0 || dist == 0) {
				// If a shaft was not found, then that means we've hit a dead end, return
				// an empty path to signal that no path is possible
				if (nextShaft == null) {
					path.Clear ();
					return path;
				}

				path.Push (nextShaft); // Add shaft with minimunm found distance to path

				// If the minimum found distance is 0, then we've reached the end
				if (dist == 0) {
					checkShafts.Clear ();
					break;
				}

				// Add connected shafts to queue
				foreach(Shaft shaft in nextShaft.connectedShafts)
					checkShafts.Enqueue (shaft);

				nextShaft = null; // Set nextShaft to null to initialize for next set of shafts
			}
		}

		return path;
	}

	private Vector2 getMouseCell() {
		Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		int x = Mathf.FloorToInt (mousePos.x/2.0f + 0.5f);
		int y = Mathf.FloorToInt (mousePos.y/2.0f + 0.5f);

		return new Vector2 (x, y);
	}

	public Stack<Shaft> path; // TEST STACK FOR PATH

	void Update() {
		Vector2 mouseCell;
		int x, y;

		if (Input.GetKeyDown (KeyCode.Q)) {
			mouseCell = getMouseCell ();
			x = (int)mouseCell.x; y = (int)mouseCell.y;

			if (x >= 0 && x < hotel.width && y >= 0 && y < hotel.height)
				calculateShaftDistances (x, y);
		} else {
			if (Input.GetKeyDown (KeyCode.E)){
				mouseCell = getMouseCell ();
				x = (int)mouseCell.x; y = (int)mouseCell.y;
				
				if (x >= 0 && x < hotel.width && y >= 0 && y < hotel.height)
					path = createPathFromShaftDistances (x,y);
			}
		}

		if (path != null)
			drawPath ();
	}

	private void drawPath() {
		if (path.Count == 0)
			return;

		Stack<Shaft> drawStack = new Stack<Shaft> (path);

		Shaft shaft = drawStack.Pop ();
		Vector3 drawPos = shaft.transform.localPosition + new Vector3(0.0f,0.5f,0.0f);

		while (drawStack.Count > 0) {
			shaft = drawStack.Pop ();
			Vector3 nextPos = shaft.transform.localPosition+ new Vector3(0.0f,0.5f,0.0f);

			Debug.DrawLine (drawPos, nextPos, Color.red, Time.deltaTime, false);
			drawPos = nextPos;
		}
	}
}
