﻿using UnityEngine;
using System.Collections.Generic;

public class Monster : MonoBehaviour {
	public MonsterManager monsterManager;
	public Room room;

	public bool prefersAlone;
	public bool prefersCompany;

	public Room targetRoom;
	private Room moveToRoom;
	private float moveToXPos;
	public Stack<Shaft> path;

	public int targetY; // for use with shafts

	private RoomManager roomMgr;

	private Animator animator;

	private PopupText popupText;
	private SpriteRenderer spriteRenderer;

	private int hotelWidth;
	private float hotelBounds = 8.0f;
	public float ground;
	public int floor;

	private float speed;
	private float velY;

	private bool clicked;
	private string aiState;
	private int aiStateDuration;
	private int sleepNeed, eatNeed, funNeed;
	private bool isLeaving;
	private int annoyance;

	// Use this for initialization
	void Start () {
		floor = -1; ground = 0.0f;
		//Initialize variables
		roomMgr = monsterManager.roomManager;

		spriteRenderer = GetComponent<SpriteRenderer>();
		animator = GetComponent<Animator>();
		animator.Play("idle", -1, float.NegativeInfinity);

		popupText = Resources.Load<PopupText>("Effects/Prefabs/Popup Text");
		
		speed = 0.015f; velY = 0.0f;
		aiState = "IDLE";
		aiStateDuration = 0;
		sleepNeed = 1000; eatNeed = 500; funNeed = 500;

		path = new Stack<Shaft> ();

		Queue<string> needs = getMonsterNeeds ();
		while (needs.Count > 0)
			print (needs.Dequeue ());

		isLeaving = false;

		getTargetRoom ();
	}

	public void Initialize(int startHotelWidth) {
		hotelWidth = startHotelWidth;
		float randomX = -hotelBounds;
		if (Random.Range(0, 2) == 0) {
			randomX = 2.0f * hotelWidth + hotelBounds - 1.5f;
			transform.localScale = new Vector3 (-transform.localScale.x, 1, 1);
		}
		transform.localPosition = new Vector3 (randomX, -1, 100.0f);
	}
	
	// Update is called once per frame
	void Update () {
		if (clicked) {
			animator.Play("pickup", -1, float.NegativeInfinity);
			Vector3 mousePos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
			transform.localPosition = new Vector3 (mousePos.x, mousePos.y - 1, 100.0f);
		} else {
			// Check if above ground first
			if (floor >= 0) ground = floor * 2.0f;

			if (room != null && room.room_type == "shaft") return;
			if (transform.localPosition.y + 1 > ground) {
				velY -= 0.01f;
				transform.Translate(new Vector3(0.0f, velY, 0.0f));
				if (transform.localPosition.y + 1 > ground) {
					animator.Play("pickup", -1, float.NegativeInfinity);
				} else
					animator.Play("idle", -1, float.NegativeInfinity);
			} else {
				velY = 0.0f;
				transform.localPosition = new Vector3(transform.localPosition.x,
				                                      ground - 1,
				                                      transform.localPosition.z);

				updateAI(false);
			}
		}
		// Change monster to gray to show that it is outside room
		float annoyanceHue = (500.0f - annoyance)/500.0f;
		if (room == null) {
			if (floor < 0) {
				spriteRenderer.color = new Color(0.5f, 0.5f * annoyanceHue, 0.5f * annoyanceHue, 0.65f);
			} else {
				spriteRenderer.color = new Color(0.5f, 0.5f * annoyanceHue, 0.5f * annoyanceHue, 1.0f);
			}
		}
	}

	// Click a monster
	void OnMouseDown() {
		if (!isLeaving) {
			clicked = true;

			if (room != null) {
				room.monsters.Remove (this);
				roomMgr.matchmaker.matchRoom (room);
				room.updateSprite ();
			}
			room = null; moveToRoom = null; moveToRoom = null; floor = -1; ground = 0.0f;
			transform.localPosition = new Vector3 (transform.localPosition.x, transform.localPosition.y, 100.0f);
		}
	}

	// Let go of a monster
	void OnMouseUp() {
		if (!isLeaving) {
			animator.Play ("idle", -1, float.NegativeInfinity);
			clicked = false;

			int cellX = Mathf.FloorToInt (transform.localPosition.x / 2.0f + 0.5f),
			cellY = Mathf.FloorToInt (transform.localPosition.y / 2.0f + 0.5f);

			Room[,] cells = roomMgr.cells;
			if (cells [cellX, cellY] != null && cells [cellX, cellY].monsters.Count < cells [cellX, cellY].capacity) {
				room = cells [cellX, cellY]; targetRoom = room;
				room.monsters.Add (this);
				room.updateSprite ();
				floor = cellY;
				transform.localPosition = new Vector3 (transform.localPosition.x, transform.localPosition.y, 100.0f);
			} else {
				getTargetRoom ();
			}
			path.Clear (); 
			 

			transform.localScale = new Vector3 (1 - 2 * Random.Range (0, 2), 1, 1);
			updateAI (true); print ("RESET");
			collisionCheck ();
		}
	}

	// AI for the monster
	void updateAI(bool reset) {
		if (isLeaving && floor == -1) {
			if (annoyance > 0) annoyance -= 1;
			transform.position += new Vector3 (speed * transform.localScale.x, 0, 0);
			boundsCheck();
			return;
		}

		// Inside room? //
		if (room != null) {
			if (fillNeed (room.room_type)) {
				if (annoyance > 0) annoyance -= 1;
				aiState = "BUSY";
				return;
			} else {
				getTargetRoom();
				room.monsters.Remove (this); room.updateSprite ();
				room = null;
			}
		}

		// Get target room and next in path //
		if (targetRoom != null) {
			// Follow path //
			if (moveToRoom == null) {
				// Follow path to next in path //
				if (path.Count > 0) {
					moveToRoom = path.Pop();
				// In target level, move to target room //
				} else {
					moveToRoom = targetRoom;
				}
				moveToXPos = Random.Range (moveToRoom.cellX * 2.0f - 0.5f, moveToRoom.cellX * 2.0f + (moveToRoom.width-1)*2.0f + 0.5f);
			}
		}

		if (moveToRoom != null) {
			// Move to x position to target room //
			if (moveToXPos > transform.localPosition.x) {
				transform.localScale = new Vector3 (1, 1, 1);
			} else {
				transform.localScale = new Vector3 (-1, 1, 1);
			}
			// At next room? //
			if (Mathf.Abs (moveToXPos - transform.localPosition.x) > 0.2) {
				// Keep walking //
				animator.Play ("walk", -1, float.NegativeInfinity);
				transform.position += new Vector3 (speed * transform.localScale.x, 0, 0);
				if (annoyance > 0) annoyance -= 1;
			} else {
				// Try and enter, if full, wait //

				if (moveToRoom.Enter (this)) {
					room = moveToRoom; moveToRoom = null;
					collisionCheck ();
					if (room.room_type == "lobby" && sleepNeed <= 0) {
						floor = -1;
						room.monsters.Remove (this);
						room.updateSprite ();
						room = null; isLeaving = true;
						speed = 0.04f;
					}
				} else {
					animator.Play ("idle", -1, float.NegativeInfinity);
					if (annoyance < 500) {
						annoyance += 1;
					} else {
						monsterManager.hotel.addHappiness(-0.2f);
					}
				}
			}
			return;
		}

		if (annoyance < 500) {
			annoyance += 1;
		} else {
			monsterManager.hotel.addHappiness(-0.2f);
		}

		return;
	}

	void getTargetRoom() {
		targetRoom = null;
		if (room != null) {
			monsterManager.matchMonster(this);
		} else {
			targetRoom = roomMgr.getNearestLobby(transform.localPosition.x);
		}

		if (targetRoom != null) {
			if (room != null) {
				room.monsters.Remove (this);
				room.updateSprite ();
				roomMgr.matchmaker.matchRoom (room);
			}
			room = null;
		} else {
			animator.Play ("idle", -1, float.NegativeInfinity);
		}
	}

	bool fillNeed(string room_type) {
		int needValue = getNeedValue (room_type);
		if (needValue <= 0) return false;

		float annoyanceHue = (500.0f - annoyance)/500.0f;
		if (needValue % 50 == 0) {
			int revenue = room.income;
			
			monsterManager.hotel.gold += revenue;
			if (room_type == "fun" || room_type == "eat" || room_type == "health") {
				spriteRenderer.color = new Color(0f, 1f * annoyanceHue, 1f * annoyanceHue, 1f);
				monsterManager.hotel.addHappiness (1.0f);
			} else {
				spriteRenderer.color = new Color(1f, 1f * annoyanceHue, 1f * annoyanceHue, 1f);
			}

			Vector3 popUpPos = transform.localPosition; popUpPos.y += 1.0f; popUpPos.z = -5.0f;
			PopupText newPopupText = Instantiate(popupText, popUpPos, Quaternion.identity) as PopupText;
			newPopupText.text_display = "+" + revenue; newPopupText.text_color = Color.yellow;
		}
		subNeedValue (room_type);
		
		switch (room_type) {
			case "sleep":
				animator.Play ("sleep", -1, float.NegativeInfinity);
				break;
			case "fun":
				animator.Play ("fun", -1, float.NegativeInfinity);
				break;
			case "eat":
				animator.Play ("eat", -1, float.NegativeInfinity);
				break;
			case "health":
				animator.Play ("health", -1, float.NegativeInfinity);
				break;
			default:
				break;
			}

		spriteRenderer.sortingOrder = 0;
		return true;
	}

	void collisionCheck () {
		if (room != null) {
			float roomLeftWallX = room.cellX * 2.0f - 0.5f,
				roomRightWallX = room.cellX * 2.0f + (room.width-1)*2.0f + 0.5f;
			if (transform.localPosition.x < roomLeftWallX) {
				transform.localPosition = new Vector3 (roomLeftWallX, transform.localPosition.y, 100.0f);
				transform.localScale = new Vector3 (-transform.localScale.x, 1, 1);
			} else if (transform.localPosition.x > roomRightWallX) {
				transform.localPosition = new Vector3 (roomRightWallX, transform.localPosition.y, 100.0f);
				transform.localScale = new Vector3 (-transform.localScale.x, 1, 1);
			}
		}
	}

	void boundsCheck() {
		if (transform.localPosition.x < -hotelBounds || transform.localPosition.x > 2.0f * hotelWidth + hotelBounds - 1.5f) {
			monsterManager.deleteMonster (this);
		}
	}

	private int getRoomTypeID(string type) {
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

	public int getNeedValue(string type) {
		switch (type) {
		case "sleep":
			return sleepNeed;
			
		case "eat":
			return eatNeed;
			
		case "fun":
			return funNeed;
			
		default:
			return -1;
		}
	}

	public void subNeedValue(string type) {
		switch (type) {
		case "sleep":
			sleepNeed--;
			break;
		case "eat":
			eatNeed--;
			break;
		case "fun":
			funNeed--;
			break;
		default:
			break;
		}
	}

	public Queue<string> getMonsterNeeds() {
		List<string> needs = new List<string> (new string[]{"sleep", "eat", "fun"});
		Queue<string> returnNeeds = new Queue<string> ();

		string maxNeed = ""; int maxNeedValue;
		while (needs.Count > 0) {
			maxNeed = "";
			maxNeedValue = int.MinValue;

			foreach(string need in needs) {
				int needValue = getNeedValue(need);
				if (needValue > maxNeedValue) {
					maxNeedValue = needValue;
					maxNeed = need;
				}
			}

			if (maxNeed != "") {
				if (maxNeedValue > 0) returnNeeds.Enqueue (maxNeed); // Check if max need and ignore needs at zero value
				needs.Remove (maxNeed);
			}
		}

		return returnNeeds;
	}

	public List<Shaft> getReachableShafts() {
		List<Shaft> returnShafts;

		if (floor == -1) {
			returnShafts = roomMgr.getReachableShaftsAt (0, 0);
		} else {
			int cellX = Mathf.FloorToInt (transform.localPosition.x / 2.0f + 0.5f);

			returnShafts = roomMgr.getReachableShaftsAt (cellX, floor);
		}
		

		return returnShafts;
	}

	public void leaveRoom(bool bailout , bool findNewRoom) {
		moveToRoom = null; 
		if (bailout) {
			floor = -1;
			ground = 0.0f;
		}

		if (room != null) {
			room.monsters.Remove (this);
			room.updateSprite ();
			room = null;
		}
		
		if (findNewRoom)
			getTargetRoom ();
	}
}
