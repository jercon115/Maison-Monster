using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Room : MonoBehaviour {
	public int width, height;
	public int cellX, cellY;
	public int cost;
	public int income;
	public int capacity;
	public string room_type;
	public List<Monster> monsters;

	private bool highlighted;

	protected SpriteRenderer spriteRenderer;
	protected GameObject ConstructionEffect;
	protected PopupText popupText;
	protected RoomManager roomMgr;
	protected Hotel hotel;

	protected void popupCostText(float x, float y) {
		Vector3 popUpPos = new Vector3(x, y, -5.0f);
		PopupText newPopupText = Instantiate(popupText, popUpPos, Quaternion.identity) as PopupText;
		newPopupText.text_display = "-" + cost; newPopupText.text_color = Color.red;
		hotel.gold -= cost;
	}

	// Initialization of variables and position
	public virtual void Setup(RoomManager myRoomMgr, int x, int y) {
		// Setup variables
		ConstructionEffect = Resources.Load ("Effects/Prefabs/Dust Cloud Particle") as GameObject;
		popupText = Resources.Load<PopupText>("Effects/Prefabs/Popup Text");
		
		roomMgr = myRoomMgr;
		hotel = roomMgr.hotel;
		cellX = x; cellY = y;
		
		spriteRenderer = GetComponent<SpriteRenderer>();
		if (monsters.Count < capacity)
			spriteRenderer.color = new Color (0.4f, 0.4f, 0.4f, 1f);
		highlighted = false;
		
		// Setup position
		transform.parent = roomMgr.transform;
		transform.localPosition =
			new Vector3 (x * 2.0f + (width-1), y * 2.0f + (height-1), 10.0f);
	}

	// Called when constructing this room
	public virtual void Construct(RoomManager myRoomMgr, int x, int y) {
		// Initialize and setup room
		Setup (myRoomMgr, x, y);

		// Construction dust cloud effect and update room cells
		for (int i = 0; i < width; i++) {
			for (int j = 0; j < height; j++) {
				roomMgr.cells[cellX + i, cellY + j] = this;
				Instantiate (ConstructionEffect, new Vector3 ((cellX + i) * 2.0f, (cellY + j) * 2.0f, 0.0f), Quaternion.identity);
			}
		}
		// Popup text for cost
		popupCostText (cellX * 2.0f + (width - 1) * 1.0f, cellY * 2.0f);
	}
	
	public virtual void demolishCell(int x, int y) {
		for (int i = 0; i < width; i++) {
			for (int j = 0; j < height; j++) {
				Instantiate(ConstructionEffect, new Vector3((cellX+i)*2.0f, (cellY+j)*2.0f, 0.0f), Quaternion.identity);
				roomMgr.cells[cellX + i, cellY +j] = null;
			}
		}
		Destroy (gameObject);
	}

	public void updateSprite() {
		if (!highlighted) {
			if (capacity == 0 || monsters.Count > 0) {
				spriteRenderer.color = new Color (1f, 1f, 1f, 1f);
			} else {
				spriteRenderer.color = new Color (0.4f, 0.4f, 0.4f, 1f);
			}
		}
	}

	public virtual void highlightSprite(Color color, bool turnOnHighlight) {
		if (turnOnHighlight == false) {
			highlighted = false;
			updateSprite ();
		} else {
			highlighted = true;
			spriteRenderer.color = color;
		}
	}
}