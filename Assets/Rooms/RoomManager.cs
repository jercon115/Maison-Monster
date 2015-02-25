using UnityEngine;
using System.Collections;


public class RoomManager : MonoBehaviour {
	
	public Hotel hotel;
	public Room[,] cells;

	private GameObject ConstructionEffect;
	private PopupText popupText;

	// Use this for initialization
	public void Start () {
		cells = new Room[hotel.width, hotel.height];
		ConstructionEffect = Resources.Load ("Effects/Prefabs/Dust Cloud Particle") as GameObject;
		popupText = Resources.Load<PopupText>("Effects/Prefabs/Popup Text");
	}

	public void MakeRoom(int x, int y, Room newroom) {
		if (roomLocationValid (x, y, newroom.width, newroom.height)) {
			Vector3 roomPos =
				new Vector3 (x * 2.0f + (newroom.width-1), y * 2.0f + (newroom.height-1), 10.0f);
			Room tmpRoom = Instantiate (newroom, roomPos, Quaternion.identity) as Room;
			tmpRoom.transform.parent = transform;
			tmpRoom.cellX = x; tmpRoom.cellY = y;

			if (tmpRoom is Shaft) {
				if (y > 0) (tmpRoom as Shaft).checkMerge(cells [x, y-1]);
				if (y < hotel.height-1) (tmpRoom as Shaft).checkMerge(cells [x, y+1]);
			}

			for(int i = 0; i < tmpRoom.width; i++) {
				for(int j = 0; j < tmpRoom.height; j++) {
					cells[tmpRoom.cellX + i, tmpRoom.cellY + j] = tmpRoom;
					if (!(tmpRoom is Shaft) || (i == x - tmpRoom.cellX && j == y - tmpRoom.cellY)) {
						Instantiate(ConstructionEffect, new Vector3((tmpRoom.cellX+i)*2.0f, (tmpRoom.cellY+j)*2.0f, 0.0f), Quaternion.identity);
					}
				}
			}
			Vector3 popUpPos = new Vector3((tmpRoom.cellX)*2.0f + (newroom.width-1)*1.0f, (tmpRoom.cellY)*2.0f, 0.0f); popUpPos.z = -5.0f;
			PopupText newPopupText = Instantiate(popupText, popUpPos, Quaternion.identity) as PopupText;
			newPopupText.text_display = "-" + newroom.cost; newPopupText.text_color = Color.red;
				hotel.gold -= newroom.cost;
		}
	}

	public void DeleteRoom(int x, int y) {
		if (cells [x, y] != null && cells [x, y].monsters.Count == 0) {
			int width = cells [x, y].width, height = cells [x, y].height;

			Room destroyRoom = cells [x, y];
			for (int i = 0; i < width; i++) {
				for (int j = 0; j < height; j++) {
					cells [destroyRoom.cellX + i, destroyRoom.cellY + j] = null;
				}
			}

			if ( destroyRoom is Shaft) {
				Room splitRoom = (destroyRoom as Shaft).checkSplit (y);
				for (int i = 0; i < destroyRoom.width; i++) {
					for (int j = 0; j < destroyRoom.height; j++) {
						cells [destroyRoom.cellX + i, destroyRoom.cellY + j] = destroyRoom;
					}
				}

				if (splitRoom != null) {
					for (int i = 0; i < splitRoom.width; i++) {
						for (int j = 0; j < splitRoom.height; j++) {
							cells [splitRoom.cellX + i, splitRoom.cellY + j] = splitRoom;
						}
					}
				}
			} else {
				destroyRoom.Destroy();
			}

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

	public Room getRoomAt(int x, int y) {
		return cells[x, y];
	}
}
