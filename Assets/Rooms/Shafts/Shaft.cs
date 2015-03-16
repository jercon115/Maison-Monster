using UnityEngine;
using System;
using System.Collections.Generic;

public class Shaft : Room {
	public Sprite singleTexture;
	public Sprite topTexture;
	public Sprite midTexture;
	public Sprite bottomTexture;

	private Stack<GameObject> sprites;
	private int spriteCount;

	// for use with pathfinding
	public List<Shaft> connectedShafts;
	
	public int[] leftmostCells;
	public int[] rightmostCells;

	public int distFromStart;

	// Initializing variables and position, called when constructing this room and cloning a shaft during a split
	public override void Setup(RoomManager myRoomMgr, int x, int y) {
		// Setup variables
		ConstructionEffect = Resources.Load ("Effects/Prefabs/Dust Cloud Particle") as GameObject;
		popupText = Resources.Load<PopupText>("Effects/Prefabs/Popup Text");
		
		roomMgr = myRoomMgr;
		hotel = roomMgr.hotel;
		cellX = x; cellY = y;
		
		spriteRenderer = GetComponent<SpriteRenderer>();
		spriteCount = 0;
		sprites = new Stack<GameObject>();

		connectedShafts = new List<Shaft> ();
		distFromStart = -1; // -1 means there is no currently known path between shaft to target

		// Initialize values for array for leftmost cells and rightmost cells at each floor
		leftmostCells = new int[height];
		rightmostCells = new int[height];
		for (int i = 0; i < height; i++) {
			leftmostCells[i] = -1; rightmostCells[i] = -1;
		}

		// Add shaft to room manager's list of shafts
		if (!roomMgr.shafts.Contains (this))
			roomMgr.shafts.Add (this);

		// Delete sprites that carried over from cloning
		foreach(Transform child in transform) {
			if (child.name == "ShaftTexture") Destroy (child.gameObject);
		}

		// Setup position
		transform.parent = roomMgr.transform;
		transform.localPosition =
			new Vector3 (x * 2.0f, y * 2.0f, 10.0f);

		// Check if it can be merged with another shaft
		if (cellY > 0) checkMerge (roomMgr.cells [cellX, cellY - 1]); // below
		if (cellY < hotel.height-height) checkMerge(roomMgr.cells [cellX, cellY+height]); // above
		
		// Update room cells
		for(int i = 0; i < width; i++)
			for(int j = 0; j < height; j++)
				roomMgr.cells[cellX + i, cellY + j] = this;

		// Update reachable ranges for any shafts in floors intersecting room (will include this shaft)
		Queue<Shaft> updatedShafts = roomMgr.updateShaftsReachableRanges (cellY, cellY + height - 1);
		roomMgr.updateShaftsConnections (updatedShafts);
	}

	// Called when constructing this room
	public override void Construct(RoomManager myRoomMgr, int x, int y) {
		Setup (myRoomMgr, x, y);

		// Construction dust cloud effect
		Instantiate(ConstructionEffect, new Vector3(x*2.0f, y*2.0f, 0.0f), Quaternion.identity);
		
		// Popup text for cost
		popupCostText (x*2.0f, y*2.0f);
	}

	// Demolishing a cell
	public override void DemolishCell(int x, int y) {
		// Dust cloud effect
		Instantiate(ConstructionEffect, new Vector3(x*2.0f, y*2.0f, 0.0f), Quaternion.identity);

		// Update cell at x and y of deletion
		roomMgr.cells [x, y] = null;

		// New room if the shaft was split into two
		Shaft splitShaft = null;

		if (height == 1) {
			this.DestroyRoom ();
			return;
		} else {

			
			if (y == cellY + height - 1) { // top of shaft?
				height--;
			} else {
				// Was the shaft split?
				if ( y > cellY ) {
					splitShaft = Instantiate (this) as Shaft;

					// Update room's name, height, and room manager's room cells, then setup
					splitShaft.name = name;
					splitShaft.height = y - cellY;
					splitShaft.Setup (roomMgr, cellX, cellY);
					
					// Update sprites for splitRoom
					splitShaft.updateSprites(splitShaft.height - 1);

					// Copy connections if still connected
					foreach(Shaft shaft in connectedShafts) {
						if (shaft.cellY <= splitShaft.cellY + splitShaft.height - 1
						    && shaft.cellY + shaft.height - 1 >= splitShaft.cellY)
							splitShaft.checkAndUpdateConnection(shaft);

					}
				}

				int newHeight = height - (y - cellY + 1);

				// Update leftmostCells and rightmostCells
				Array.Copy (leftmostCells, y-cellY+1, leftmostCells, 0, newHeight);
				Array.Resize(ref leftmostCells, newHeight);
				Array.Copy (rightmostCells, y-cellY+1, rightmostCells, 0, newHeight);
				Array.Resize(ref rightmostCells, newHeight);

				// Move above deletion
				transform.Translate ( new Vector3(0.0f , (y - cellY + 1)*2.0f, 0.0f));
				height = newHeight; print ("HEIGHT: " + height);
				cellY = y+1;

				// Check connections
				Queue<Shaft> checkShafts = new Queue<Shaft>(connectedShafts);

				while(checkShafts.Count > 0)
					checkAndUpdateConnection(checkShafts.Dequeue ());
			}
			
			updateSprites (height - 1);
		}

		// Update reachable ranges for any shafts in floors intersecting room
		Queue<Shaft> updatedShafts = roomMgr.updateShaftsReachableRanges (y, y);

		// Add this shaft and split off shaft (if any) to list of shafts to update connections for
		if (!updatedShafts.Contains (this)) updatedShafts.Enqueue (this);
		if (splitShaft != null &&!updatedShafts.Contains (splitShaft)) updatedShafts.Enqueue (splitShaft);

		// Finally update connections
		roomMgr.updateShaftsConnections (updatedShafts);
	}

	public override void DestroyRoom() {
		// Delete any child objects
		foreach(Transform child in transform) {
			Destroy (child.gameObject);
		}
		// Update the connection lists in the other shafts that are connected to this one that is to be deleted
		Queue<Shaft> removeShafts = new Queue<Shaft> (connectedShafts);
		while(removeShafts.Count > 0)
			disconnectShafts (removeShafts.Dequeue ());

		// Remove from room manager's list of shafts
		if (roomMgr.shafts.Contains (this))
			roomMgr.shafts.Remove (this);

		// Finally, destroy the game object
		Destroy (gameObject);
	}

	public void checkMerge(Room otherRoom) {
		if (otherRoom == null) return;
		if (otherRoom.GetType () == GetType ()) {
			Shaft otherShaft = (Shaft)otherRoom;

			// Merge other room into this room
			// by adding other room's list of connected shafts
			foreach(Shaft shaft in otherShaft.connectedShafts) {
				connectShafts (shaft);
			}
			// and by update height and position
			if (otherRoom.transform.localPosition.y < transform.localPosition.y) {
				transform.localPosition = otherRoom.transform.localPosition;

				cellY = otherRoom.cellY;
			}
			height += otherRoom.height;
			updateSprites ( height - 1);

			// Resize array
			Array.Resize(ref leftmostCells, height);
			Array.Resize(ref rightmostCells, height);

			// Delete other room
			otherRoom.DestroyRoom ();
		}
	}

	public void updateSprites(int newCount) {
		if (newCount < 0) return;

		if (spriteCount < newCount) {
			// Remove top
			if (spriteCount > 0) {
				Destroy( sprites.Pop() );
				spriteCount--;
			}
		} else {
			// Remove sprites until and including the new top
			while(spriteCount >= newCount && spriteCount > 0) {
				Destroy( sprites.Pop() );
				spriteCount--;
			}
		}

		// Add sprites until the new top
		while(spriteCount < newCount) {
			GameObject tmpObj = new GameObject();
			SpriteRenderer tmpRenderer = tmpObj.AddComponent (typeof(SpriteRenderer)) as SpriteRenderer;
			if (spriteCount == newCount - 1) {
				tmpRenderer.sprite = topTexture;
			} else  {
				tmpRenderer.sprite = midTexture;
			}
			tmpObj.name = "ShaftTexture";
			tmpObj.transform.localPosition = new Vector3(0.0f, spriteCount*2.0f+2.0f, 0.0f);
			tmpObj.transform.SetParent(transform, false);

			sprites.Push (tmpObj);
			spriteCount++;
		}

		// Update own sprite as either bottom or single sprite
		if (spriteCount == 0) {
			spriteRenderer.sprite = singleTexture;
		} else {
			spriteRenderer.sprite = bottomTexture;
		}
	}

	// returns true if a cell is directly reachable from this shaft
	public bool isReachable(int x, int y) {
		int floor = y - cellY;
		if (floor < 0 || floor >= height)
			return false; // Not on a floor that shaft connects to

		// otherwise check if x is within reachable range for that floor
		return (x >= leftmostCells [floor] && x <= rightmostCells [floor]);
	}

	// add connected shaft if it isn't in the list already
	public void connectShafts(Shaft addShaft) {
		if (!connectedShafts.Contains (addShaft))
			connectedShafts.Add (addShaft);

		if (!addShaft.connectedShafts.Contains (this))
			addShaft.connectedShafts.Add (this);
	}

	// remove a connection to another shaft
	public void disconnectShafts(Shaft removeShaft) {
		if (connectedShafts.Contains (removeShaft))
			connectedShafts.Remove (removeShaft);
		
		if (removeShaft.connectedShafts.Contains (this))
			removeShaft.connectedShafts.Remove (this);
	}

	public void checkAndUpdateConnection(Shaft otherShaft) {
		// Get range of overlapping floors
		int y1 = Math.Max (cellY, otherShaft.cellY);
		int y2 = Math.Min (cellY + height - 1, otherShaft.cellY + otherShaft.height - 1);

		// Assume shafts are not connected for now
		disconnectShafts (otherShaft);

		// For each floor, check if each elevator are within each other's reachable ranges
		// Technically they should be the same reachable ranges, however to be extra careful
		for (int i = y1; i <= y2; i++) {
			if(isReachable (otherShaft.cellX, i) && otherShaft.isReachable (cellX, i)) {
				connectShafts(otherShaft); break; // Then shafts are connected, can stop checking
			}
		}
	}

	public int getConnectionFloor(Shaft otherShaft, int y) {
		int y1 = Math.Max (cellY, otherShaft.cellY);
		int y2 = Math.Min (cellY + height - 1, otherShaft.cellY + otherShaft.height - 1);

		int dist = int.MaxValue; int returnFloor = -1;
		for (int i = y1; i <= y2; i++) {
			// Check if reachable
			if(isReachable (otherShaft.cellX, i) && otherShaft.isReachable (cellX, i)) {
				int newDist = Mathf.Abs (i - y);

				if (newDist < dist) {
					dist = newDist;
					returnFloor = i;
				}
			}
		}

		return returnFloor;
	}

	// Calculates distances from a single source for use with findPath
	public void calculateDistancesFromStart(int prevDist) {
		int newDist = prevDist + 1;

		if (distFromStart >= 0 && distFromStart < newDist)
			return;

		distFromStart = newDist;
		foreach (Shaft shaft in connectedShafts)
			shaft.calculateDistancesFromStart (newDist);
	}

	// Initializes distFromTarget for calculateDistancesFromStart
	public void initDistancesFromStart() {
		distFromStart = -1;
	}

	// For debugging purposes
	protected void drawAllConnections() {
		foreach(Shaft shaft in connectedShafts) {
			if (shaft == null) continue;
			Debug.DrawLine (transform.localPosition, shaft.transform.localPosition, Color.green, Time.deltaTime, false);
		}
	}

	// Enter monster, see where he needs to go
	public override bool Enter(Monster mon) {
		if (monsters.Count < capacity) {
			Shaft next = mon.path.Peek ();
			if (next == null) {
				mon.targetY = mon.targetRoom.cellY;
			} else {
				mon.targetY = getConnectionFloor (next, mon.room.cellY);
			}

			monsters.Add (mon);
			updateSprite ();
			return true;
		} else {
			return false;
		}
	}

	void Update() {
		drawAllConnections ();

		Queue<Monster> removeMonsters = new Queue<Monster>();
		foreach (Monster monster in monsters) {
			float monY = monster.transform.localPosition.y;
			float toY = monster.targetY*2.0f;
			float speed = 0.05f;

			if  (Mathf.Abs (monY-toY) <= speed) {
				monster.transform.Translate (new Vector3(0.0f,toY-monY,0.0f));

				// Monster reached target floor, remove froom
				monster.room = null;
				removeMonsters.Enqueue (monster);

				continue;
			}

			while(monY < toY)
				monster.transform.Translate (new Vector3(0.0f,speed,0.0f));

			while(monY > toY)
				monster.transform.Translate (new Vector3(0.0f,-speed,0.0f));
		}

		while (removeMonsters.Count > 0) {
			monsters.Remove (removeMonsters.Dequeue ());
		}
	}
}
