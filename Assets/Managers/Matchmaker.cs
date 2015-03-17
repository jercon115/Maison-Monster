using UnityEngine;
using System.Collections.Generic;

public class Matchmaker : MonoBehaviour {

	public Hotel hotel;
	public RoomManager roomMgr;

	private List<Monster>[] monsterWaitlists;
	private List<Room>[] roomWaitlists;

	void Start () {
		// Initialize variables
		monsterWaitlists = new List<Monster>[3];
		for (int i = 0; i < monsterWaitlists.Length; i++)
			monsterWaitlists [i] = new List<Monster> ();

		roomWaitlists = new List<Room>[3];
		for (int i = 0; i < roomWaitlists.Length; i++)
			roomWaitlists [i] = new List<Room> ();
	}

	int getRoomTypeID(string type) {
		switch (type) {
			case "sleep":
				return 0;

			case "eat":
				return 1;

			case "fun":
				return 2;

			default:
				return -1;
		}
	}

	public void matchMonster(Monster monster) {
		print ("MATCHING");
		Queue<string> needsOrder = monster.getMonsterNeeds ();

		// Setup distance values from monster as single source
		roomMgr.calculateShaftDistances(monster.room.cellX, monster.room.cellY);

		Room targetRoom = null;

		while(needsOrder.Count > 0) {
			int roomTypeID = getRoomTypeID(needsOrder.Dequeue());

			if (roomTypeID < 0) continue; // skip if invalid need

			List<Room> waitingRooms = roomWaitlists[roomTypeID];

			// If there are rooms available, match monster and room, otherwise, make monster wait
			int minDist = int.MaxValue;
			foreach(Room room in waitingRooms) {
				if (room == null) continue; // Room was deleted. Skip.

				List<Shaft> shafts = roomMgr.getReachableShaftsAt (room.cellX, room.cellY);
				foreach(Shaft shaft in shafts) {
					int dist = shaft.distFromStart;

					if (dist < minDist && dist > -1) {
						minDist = dist;
						targetRoom = room;
					}
				}
			}

			if (targetRoom != null) {
				// If a reachable room was found, match is made
				monster.targetRoom = targetRoom;
				if (targetRoom.monsters.Count + 1 >= targetRoom.capacity)
					waitingRooms.Remove (targetRoom); // remove room from waiting list

				// Create path
				int cellX = 0, cellY = 0;
				if (monster.floor > -1) {
					cellX = Mathf.FloorToInt (transform.localPosition.x / 2.0f + 0.5f);
					cellY = monster.floor;
				}

				monster.path = new Stack<Shaft>();
				if(!roomMgr.isDirectlyReachable (cellX, cellY, targetRoom.cellX, targetRoom.cellY))
					monster.path = roomMgr.createPathFromShaftDistances(targetRoom.cellX, targetRoom.cellY);

				break;
			}
		}

		if (targetRoom == null && needsOrder.Count == 0) { // No available rooms at all, need to add monster to waiting lists
			if (monster.getNeedValue ("sleep") <= 0) {
				monster.targetRoom = roomMgr.getNearestLobby(monster.transform.localPosition.x);
				monster.path = roomMgr.createPathFromShaftDistances(monster.targetRoom.cellX, monster.targetRoom.cellY);
			} else {
				makeMonsterWait (monster);
			}
		}
	}

	public void matchRoom(Room room) {
		print ("MATCHING");
		int roomTypeID = getRoomTypeID(room.room_type);
		if (roomTypeID < 0)
			return; // not a room that can be matched

		List<Monster> waitingMonsters = monsterWaitlists[roomTypeID];

		// Setup distance values from room as single source
		roomMgr.calculateShaftDistances (room.cellX, room.cellY);

		// If there are rooms available, match monster and room, otherwise, make monster wait
		int minDist = int.MaxValue; Monster targetMonster = null;
		foreach(Monster monster in waitingMonsters) {
			if (monster == null) continue; // Monster was deleted. Skip.
			
			List<Shaft> shafts = monster.getReachableShafts();
			foreach (Shaft shaft in shafts) {
				int dist = shaft.distFromStart;
				
				if (dist < minDist && dist > -1 && 
				    monster.targetRoom == null) { // hasn't been matched yet
					minDist = dist;
					targetMonster = monster;
				}
			}
		}
		
		if (targetMonster != null) {
			print ("MATCH FOUND");
			// If a monster was found, match is made
			targetMonster.targetRoom = room;
			if (targetMonster.room != null) {
				if (targetMonster.room.monsters.Contains (targetMonster))
					targetMonster.room.monsters.Remove(targetMonster);
				targetMonster.room.updateSprite ();
				targetMonster.room = null;
		    }
			waitingMonsters.Remove (targetMonster); // remove room from waiting list

			// Need to flip path because path was from room to monster
			int cellX = 0, cellY = 0;
			if (targetMonster.floor > -1) {
				cellX = Mathf.FloorToInt (targetMonster.transform.localPosition.x / 2.0f + 0.5f);
				cellY = targetMonster.floor;
			}

			targetMonster.path = new Stack<Shaft>();
			if (!roomMgr.isDirectlyReachable (cellX, cellY, room.cellX, room.cellY))
				targetMonster.path = flipPath(roomMgr.createPathFromShaftDistances(cellX, cellY));
		} else {
			// No available monsters at all, need to add room to waiting lists
			makeRoomWait (room);
		}
	}

	private Stack<Shaft> flipPath(Stack<Shaft> oldPath) {
		Stack<Shaft> newPath = new Stack<Shaft> ();

		while (oldPath.Count > 0) {
			newPath.Push (oldPath.Pop ());
		}

		return newPath;
	}
	
	private void makeMonsterWait(Monster monster) {
		Queue<string> needs = monster.getMonsterNeeds ();

		// Add monster to each waiting list of its needs
		while (needs.Count > 0) {
			int roomTypeID = getRoomTypeID (needs.Dequeue());
			List<Monster> waitingList = monsterWaitlists[roomTypeID];

			if (!waitingList.Contains (monster))
				waitingList.Add (monster);
		}
	}

	private void makeRoomWait(Room room) {
		int roomTypeID = getRoomTypeID (room.room_type);

		List<Room> waitingList = roomWaitlists [roomTypeID];
		if (!waitingList.Contains (room))
			waitingList.Add (room);
	}
}
