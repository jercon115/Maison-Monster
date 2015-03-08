using UnityEngine;
using System.Collections.Generic;


public class RoomManager : MonoBehaviour {
	
	public Hotel hotel;
	public Room[,] cells;

	// Use this for initialization
	public void Start () {
		cells = new Room[hotel.width, hotel.height];
	}

	public void MakeRoom(int x, int y, Room newroom) {
		if (roomLocationValid (x, y, newroom.width, newroom.height)) {
			Room tmpRoom = Instantiate (newroom) as Room;
			tmpRoom.Construct (this, x, y);
		}
	}

	public void DeleteRoom(int x, int y) {
		if (cells [x, y] != null && cells [x, y].monsters.Count == 0) {
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
		return true;
	}

	public void updateShaftsReachableRanges(int y1, int y2) {
		Queue<Shaft> foundShafts = new Queue<Shaft>();
		int numFoundShafts = 0;
		int leftmost = -1, rightmost = -1;

		for (int j = y1; j <= y2; j++) {
			for(int i = 0; i < hotel.width; i++) {
				if (cells[i, j] != null) {
					if (cells[i, j] is Shaft) {
						foundShafts.Enqueue ((Shaft)cells[i,j]);
						numFoundShafts++;
					}

					if (leftmost == -1) {
						leftmost = i; rightmost = i;
					} else
						rightmost++;
				}

				if (cells[i,j] == null || i == hotel.width-1) {
					while(numFoundShafts > 0) {
						Shaft shaft = foundShafts.Dequeue();
						int index = j-shaft.cellY;
						shaft.leftmostCells[index] = leftmost;
						shaft.rightmostCells[index] = rightmost;

						numFoundShafts--;
					}

					leftmost = -1; rightmost = -1;
				}
			}
		}
	}
}
