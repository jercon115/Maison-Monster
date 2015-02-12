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
		if (roomLocationValid (x, y, newroom.width)) {
			cells [x, y] = Instantiate (newroom) as Room;
			cells [x, y].transform.parent = transform;
			cells [x, y].transform.localPosition =
				new Vector3 (x * 2.0f + (newroom.width-1), y * 2.0f, 10.0f);

			Instantiate(ConstructionEffect, new Vector3(x*2.0f, y*2.0f, 0.0f),  Quaternion.identity);
			for(int i = 1; i < newroom.width; i++) {
				cells[x + i, y] = cells[x, y];
				Instantiate(ConstructionEffect, new Vector3((x+i)*2.0f, y*2.0f, 0.0f), Quaternion.identity);
			}
		}
	}

	public void DeleteRoom(int x, int y) {
		if (cells [x, y] != null) {
			int width = cells [x, y].width;
			int startX = Mathf.RoundToInt((cells [x, y].transform.localPosition.x - (width-1))/2.0f);

			Destroy (cells [x, y].gameObject);
			Instantiate(ConstructionEffect, new Vector3(startX*2.0f, y*2.0f, 0.0f),  Quaternion.identity);
			for (int i = 0; i < width; i++) {
				cells [startX + i, y] = null;
				Instantiate(ConstructionEffect, new Vector3((startX+i)*2.0f, y*2.0f, 0.0f), Quaternion.identity);
			}

			print ("Destroyed");
		}
	}

	public bool roomLocationValid(int x, int y, int width) {
		if (x < 0 || x >= hotel.width-(width-1) || y < 0 || y >= hotel.height)
			return false;

		bool foundation_exists = false;
		for(int i = 0; i < width; i++) {
			if (cells[x + i, y] != null) return false;
			if (y > 0) {
				if (cells[x + i, y - 1] != null)
					foundation_exists = true;
			} else if (y == 0)
				foundation_exists = true;
		}
		if (!foundation_exists) return false;
		return true;
	}
}
