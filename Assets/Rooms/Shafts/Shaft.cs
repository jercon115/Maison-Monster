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
	private List<Shaft> connectedShafts;
	
	public int[] leftmostCells;
	public int[] rightmostCells;

	private int distFromStart;

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

		// Initialize values for array for leftmost cells and rightmost cells at each floor
		leftmostCells = new int[height];
		rightmostCells = new int[height];
		for (int i = 0; i < height; i++) {
			leftmostCells[i] = cellX; rightmostCells[i] = cellX;
		}

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
		roomMgr.updateShaftsReachableRanges (cellY, cellY + height - 1);
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

		// Update reachable ranges for any shafts in floors intersecting room
		roomMgr.updateShaftsReachableRanges (y, y);

		if (height == 1) {
			this.DestroyRoom ();
		} else {
			Shaft splitRoom = null;
			
			if (y == cellY + height - 1) { // top of shaft?
				height--;
			} else {
				// Was the shaft split?
				if ( y > cellY ) {
					splitRoom = Instantiate (this) as Shaft;

					// Update room's name, height, and room manager's room cells, then setup
					splitRoom.name = name;
					splitRoom.height = y - cellY;
					splitRoom.Setup (roomMgr, cellX, cellY);
					
					// Update sprites for splitRoom
					splitRoom.updateSprites(splitRoom.height - 1);
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
			}
			
			updateSprites (height - 1);
		}
	}

	public override void DestroyRoom() {
		// Delete any child objects
		foreach(Transform child in transform) {
			Destroy (child.gameObject);
		}
		// Update the connection lists in the other shafts that are connected to this one that is to be deleted
		foreach(Shaft shaft in connectedShafts) {
			shaft.connectedShafts.Remove (this);
		}
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
				shaft.addConnectedShaft (otherShaft);
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

	// add connected shaft if it isn't in the list already
	public void addConnectedShaft(Shaft addShaft) {
		if (!connectedShafts.Contains (addShaft))
			connectedShafts.Add (addShaft);
	}

	public void removeConnectedShaft(Shaft removeShaft) {
		connectedShafts.Remove (removeShaft);
	}
}
