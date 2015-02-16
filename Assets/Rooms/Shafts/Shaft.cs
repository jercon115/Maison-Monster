using UnityEngine;
using System.Collections;

public class Shaft : Room {
	public Sprite singleTexture;
	public Sprite topTexture;
	public Sprite midTexture;
	public Sprite bottomTexture;

	private bool elvRequested;
	private RoomManager roomManager;
	private Hotel hotel;
	private SpriteRenderer myRenderer;

	private bool below, above;

	public void Start() {
		elvRequested = false;

		roomManager = transform.parent.GetComponent<RoomManager>();
		hotel = roomManager.transform.parent.GetComponent<Hotel>();
		myRenderer = GetComponent<SpriteRenderer>();

		checkBelowAbove ();

		updateAdjacent (true);
	}

	private void checkBelowAbove() {
		int x = Mathf.FloorToInt(transform.localPosition.x / 2.0f),
		y = Mathf.FloorToInt(transform.localPosition.y / 2.0f);
		
		below = false; above = false;
		
		if (y > 0) {
			Room room = roomManager.getRoomAt (x, y - 1);
			if (room is Shaft)
				if (room.GetType () == GetType ())
					below = true;
		}

		if (y < hotel.height-1) {
			Room room = roomManager.getRoomAt (x, y + 1);
			if (room is Shaft)
				if (room.GetType () == GetType ())
				above = true;
		}

		updateTexture ();
	}

	private void updateTexture() {
		if (below) {
			if (above) {
				myRenderer.sprite = midTexture; //Mid
			} else
				myRenderer.sprite = topTexture; //Top
		} else {
			if (above) {
				myRenderer.sprite = bottomTexture; //Bottom
			} else
				myRenderer.sprite = singleTexture; //Single
		}
	}

	public void openAbove() {
		above = true;
		updateTexture ();
	}
	
	public void openBelow() {
		below = true;
		updateTexture ();
	}

	public void closeAbove() {
		above = false;
		updateTexture ();
	}

	public void closeBelow() {
		below = false;
		updateTexture ();
	}

	private void updateAdjacent(bool openClose) { // 1 is open, 0 is close
		int x = Mathf.FloorToInt(transform.localPosition.x / 2.0f),
		y = Mathf.FloorToInt(transform.localPosition.y / 2.0f);

		if (y > 0) {
			Room room = roomManager.getRoomAt (x, y - 1);
			if (room is Shaft) {
				if (room.GetType () == GetType ()) {
					if (openClose) {
						(room as Shaft).openAbove();
					} else
						(room as Shaft).closeAbove();
				}
			}
		}
		
		if (y < hotel.height-1) {
			Room room = roomManager.getRoomAt (x, y + 1);
			if (room is Shaft) {
				if (room.GetType () == GetType ()) {
					if (openClose) {
						(room as Shaft).openBelow();
					} else
						(room as Shaft).closeBelow();
				}
			}
		}
	}

	public override void Destroy() {
		updateAdjacent (false);
		print (GetType());

		Destroy (gameObject);
	}
}
