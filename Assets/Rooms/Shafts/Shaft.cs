using UnityEngine;
using System.Collections.Generic;

public class Shaft : Room {
	public Sprite singleTexture;
	public Sprite topTexture;
	public Sprite midTexture;
	public Sprite bottomTexture;

	private Stack<GameObject> sprites;
	private int spriteCount;

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

		// Delete sprites that carried over from cloning
		foreach(Transform child in transform) {
			if (child.name == "ShaftTexture") Destroy (child.gameObject);
		}

		// Setup position
		transform.parent = roomMgr.transform;
		transform.localPosition =
			new Vector3 (x * 2.0f, y * 2.0f, 10.0f);
	}

	// Called when constructing this room
	public override void Construct(RoomManager myRoomMgr, int x, int y) {
		Setup (myRoomMgr, x, y);

		// Construction dust cloud effect
		Instantiate(ConstructionEffect, new Vector3(x*2.0f, y*2.0f, 0.0f), Quaternion.identity);
		
		// Popup text for cost
		popupCostText (x*2.0f, y*2.0f);

		// Check if it can be merged with another shaft
		if (cellY > 0) checkMerge (roomMgr.cells [cellX, cellY - 1]); // below
		if (cellY < hotel.height-height) checkMerge(roomMgr.cells [cellX, cellY+height]); // above
		
		// Update room cells
		for(int i = 0; i < width; i++)
			for(int j = 0; j < height; j++)
				roomMgr.cells[cellX + i, cellY + j] = this;
	}

	// Demolishing a cell
	public override void demolishCell(int x, int y) {
		// Update cell at x and y of deletion
		roomMgr.cells [x, y] = null;
		Instantiate(ConstructionEffect, new Vector3(x*2.0f, y*2.0f, 0.0f), Quaternion.identity);
		
		if (height == 1) {
			Destroy (gameObject);
		} else {
			Shaft splitRoom = null;
			
			if (y == cellY + height - 1) { // top of shaft?
				height--;
			} else {
				// Was the shaft split?
				if ( y > cellY ) {
					splitRoom = Instantiate (this) as Shaft;
					splitRoom.Setup (roomMgr, cellX, cellY);
					
					// Update room's name, height, and room manager's room cells
					splitRoom.name = name;
					splitRoom.height = y - cellY;
					for(int i = 0; i < splitRoom.height; i++)
						roomMgr.cells[cellX, cellY +i] = splitRoom;
					
					// Update sprites for splitRoom
					splitRoom.updateSprites(splitRoom.height - 1);
				}
				
				// Move above deletion
				transform.Translate ( new Vector3(0.0f , (y - cellY + 1)*2.0f, 0.0f));
				height -= (y - cellY + 1); print ("HEIGHT: " + height);
				cellY = y+1;
			}
			
			updateSprites (height - 1);
		}
	}

	public void checkMerge(Room otherRoom) {
		if (otherRoom == null) return;
		if (otherRoom.GetType () == GetType ()) {
			if (otherRoom.transform.localPosition.y < transform.localPosition.y) {
				transform.localPosition = otherRoom.transform.localPosition;

				cellY = otherRoom.cellY;
			}
			height += otherRoom.height;
			updateSprites ( height - 1);

			foreach(Transform child in otherRoom.transform) {
				Destroy (child.gameObject);
			}
			Destroy (otherRoom.gameObject);
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

	public override void highlightSprite(Color color, bool turnOnHighlight) {
		if (turnOnHighlight == false) {
			highlighted = false;
			updateSprite ();
		} else {
			highlighted = true;
			spriteRenderer.color = color;
		}
	}
}
