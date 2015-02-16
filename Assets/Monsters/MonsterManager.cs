using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MonsterManager : MonoBehaviour {
	
	public Hotel hotel;
	public RoomManager roomManager;
	public List<Monster> monsters;

	private int spawnDuration;

	// Use this for initialization
	void Start () {
		spawnDuration = 0;
	}

	void Update () {
		if (monsters.Count < 10 && spawnDuration <= 0) {
			monsters.Add( Instantiate (monsters[0]) as Monster );
			monsters [monsters.Count - 1].monsterManager = this;
			monsters [monsters.Count - 1].hotelWidth = hotel.width;
			float randomX = -2.5f;
			if (Random.Range(0, 2) == 0) {
				randomX = 2.0f * hotel.width + 1.0f;
				monsters[monsters.Count-1].transform.localScale = new Vector3 (-monsters[monsters.Count-1].transform.localScale.x, 1, 1);
			}
			monsters[monsters.Count - 1].transform.localPosition = new Vector3 (randomX, 0, 100.0f);

			spawnDuration = Random.Range (120, 240);
		}

		if (spawnDuration > 0) {
			spawnDuration--;
		}
	}

	public void deleteMonster(Monster monster) {
		monsters.Remove (monster);
		Destroy (monster.gameObject);
	}
}
