using UnityEngine;
using System.Collections.Generic;

public class Shaft : Room {
	public Sprite singleTexture;
	public Sprite topTexture;
	public Sprite midTexture;
	public Sprite bottomTexture;

	private Stack<GameObject> sprites;
	private int spriteCount;

	void Awake() {
		spriteRenderer = GetComponent<SpriteRenderer>();
		ConstructionEffect = Resources.Load ("Effects/Prefabs/Dust Cloud Particle") as GameObject;

		spriteCount = 0;
		sprites = new Stack<GameObject>();
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

	public Room checkSplit(int y) {
		if (height == 1) {
			this.Destroy ();
			return null;
		} else {
			Room splitRoom = null;

			if (y == cellY + height - 1) { // top of shaft?
				height--;
			} else {
				// Was the shaft split?
				if ( y > cellY ) {
					splitRoom = Instantiate (this, transform.localPosition, Quaternion.identity) as Room;

					splitRoom.name = name;
					splitRoom.transform.parent = transform.parent;
					splitRoom.height = y - cellY;

					foreach(Transform child in splitRoom.transform) {
						Destroy (child.gameObject);
					}

					(splitRoom as Shaft).updateSprites(splitRoom.height - 1);
				}

				// Move above deletion
				transform.Translate ( new Vector3(0.0f , (y - cellY + 1)*2.0f, 0.0f));
				height -= (y - cellY + 1); print ("HEIGHT: " + height);
				cellY = y+1;
				updateSprites (height - 1);

			}

			updateSprites (height - 1);
			Instantiate(ConstructionEffect, new Vector3(cellX*2.0f, y*2.0f, 0.0f), Quaternion.identity);
			return splitRoom;
		}
	}
}
