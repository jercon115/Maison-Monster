using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MonsterManager : MonoBehaviour {
	
	public Hotel hotel;
	public RoomManager roomManager;
	public Monster[] monster_types;

	private List<Monster> monsters;

	private int spawnDuration;

	// Use this for initialization
	void Start () {
		spawnDuration = 0;
		monsters = new List<Monster> ();
	}

	void Update () {
		if (monsters.Count < 20 && spawnDuration <= 0) {
			int randomMonster = Mathf.RoundToInt (Random.Range(0,monster_types.Length+1));
			monsters.Add( Instantiate (monster_types[randomMonster]) as Monster );
			monsters [monsters.Count - 1].monsterManager = this;
			monsters[monsters.Count - 1].Initialize(hotel.width);

			spawnDuration = Random.Range (240, 420);
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
