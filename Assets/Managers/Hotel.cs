using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Hotel : MonoBehaviour {

	public int width;
	public int height;
	public int gold;
	public Room[] rooms;
	public RoomManager roomManager;	// Grid of rooms
	public MonsterManager monsterManager;

	public Text goldText;

	void Start() {
		gold = 1000;
	}

	void Update() {
		goldText.text = "GOLD: " + gold +"G";
	}
}
