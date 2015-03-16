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
		Queue<string> needsOrder = monster.getMonsterNeeds ();

		// Setup distance values from monster as single source
		roomMgr.calculateShaftDistances(monster.room.cellX, monster.room.cellY);

		while(needsOrder.Count > 0) {
			int roomTypeID = getRoomTypeID(needsOrder.Dequeue());
			List<Room> waitingRooms = roomWaitlists[roomTypeID];

			// If there are rooms available, match monster and room, otherwise, make monster wait
			int minDist = int.MaxValue; Room targetRoom = null;
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
				waitingRooms.Remove (targetRoom); // remove room from waiting list

				// Create path
				monster.path = roomMgr.createPathFromShaftDistances(targetRoom.cellX, targetRoom.cellY);

				break;
			} else {
				print ("NO ROOM FOUND");
				if ( needsOrder.Count == 0) // No available rooms at all, need to add monster to waiting lists
					makeMonsterWait (monster);
			}
		}
	}

	public void matchRoom(Room room) {
		int roomTypeID = getRoomTypeID(room.room_type);
		List<Monster> waitingMonsters = monsterWaitlists[roomTypeID];

		// Setup distance values from room as single source
		roomMgr.calculateShaftDistances (room.cellX, room.cellY);

		// If there are rooms available, match monster and room, otherwise, make monster wait
		int minDist = int.MaxValue; Monster targetMonster = null;
		foreach(Monster monster in waitingMonsters) {
			if (monster == null) continue; // Monster was deleted. Skip.
			
			Queue<Shaft> shafts = monster.getReachableShafts();
			while(shafts.Count > 0) {
				int dist = shafts.Dequeue ().distFromStart;
				
				if (dist < minDist && dist > -1 && 
				    targetMonster.targetRoom == null) { // hasn't been matched yet
					minDist = dist;
					targetMonster = monster;
				}
			}
		}
		
		if (targetMonster != null) {
			// If a reachable room was found, match is made
			targetMonster.targetRoom = room;
			waitingMonsters.Remove (targetMonster); // remove room from waiting list

			// Need to flip path because path was from room to monster
			targetMonster.path = flipPath(roomMgr.createPathFromShaftDistances(room.cellX, room.cellY));
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
