using UnityEngine;
using System.Collections;


public class RoomManager : MonoBehaviour {

	public GameObject ConstructionEffect;
	public Hotel hotel;
	private Room[,] cells;

	// Use this for initialization
	public void Start () {
		cells = new Room[hotel.width, hotel.height];
	}

	public void MakeRoom(int x, int y, Room newroom) {
		if (roomLocationValid (x, y, newroom.width, newroom.height)) {
			Vector3 roomPos =
				new Vector3 (x * 2.0f + (newroom.width-1), y * 2.0f + (newroom.height-1), 10.0f);
			cells [x, y] = Instantiate (newroom, roomPos, Quaternion.identity) as Room;
			cells [x, y].transform.parent = transform;

			Instantiate(ConstructionEffect, new Vector3(x*2.0f, y*2.0f, 0.0f),  Quaternion.identity);
			for(int i = 1; i < newroom.width; i++) {
				for(int j = 0; j < newroom.height; j++) {
					cells[x + i, y + j] = cells[x, y];
					Instantiate(ConstructionEffect, new Vector3((x+i)*2.0f, (y+j)*2.0f, 0.0f), Quaternion.identity);
				}
			}
		}
	}

	public void DeleteRoom(int x, int y) {
		if (cells [x, y] != null) {
			int width = cells [x, y].width, height = cells [x, y].height;
			int startX = Mathf.RoundToInt((cells [x, y].transform.localPosition.x - (width-1))/2.0f);
			int startY = Mathf.RoundToInt((cells [x, y].transform.localPosition.y - (height-1))/2.0f);

			cells [x, y].Destroy();
			Instantiate(ConstructionEffect, new Vector3(startX*2.0f, startY*2.0f, 0.0f),  Quaternion.identity);
			for (int i = 0; i < width; i++) {
				for (int j = 0; j < height; j++) {
					cells [startX + i, startY + j] = null;
					Instantiate(ConstructionEffect, new Vector3((startX+i)*2.0f, (startY+j)*2.0f, 0.0f), Quaternion.identity);
				}
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
