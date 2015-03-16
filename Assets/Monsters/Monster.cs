using UnityEngine;
using System.Collections.Generic;

public class Monster : MonoBehaviour {
	public MonsterManager monsterManager;
	public Room room;

	public bool prefersAlone;
	public bool prefersCompany;

	public Room targetRoom;

	private RoomManager roomMgr;

	private Animator animator;

	private PopupText popupText;
	private SpriteRenderer spriteRenderer;

	private int hotelWidth;
	private float hotelBounds = 8.0f;

	private float speed;
	private float velY;

	private bool clicked;
	private string aiState;
	private int aiStateDuration;
	private int sleepNeed, eatNeed, funNeed;
	private bool isLeaving;

	// Use this for initialization
	void Start () {
		//Initialize variables
		roomMgr = monsterManager.roomManager;

		spriteRenderer = GetComponent<SpriteRenderer>();
		animator = GetComponent<Animator>();
		animator.Play("idle", -1, float.NegativeInfinity);

		popupText = Resources.Load<PopupText>("Effects/Prefabs/Popup Text");
		
		speed = 0.01f; velY = 0.0f;
		aiState = "IDLE";
		aiStateDuration = Random.Range (120, 240);
		sleepNeed = 1000; eatNeed = 500; funNeed = 500;

		Queue<string> needs = getMonsterNeeds ();
		while (needs.Count > 0)
			print (needs.Dequeue ());

		isLeaving = false;
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
			float ground = 0.0f;
			if (room != null) ground = room.transform.localPosition.y;

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
	}

	// Click a monster
	void OnMouseDown() {
		if (!isLeaving) {
			clicked = true;

			if (room != null) {
					room.monsters.Remove (this);
					room.updateSprite ();
			}
			room = null;
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
					room = cells [cellX, cellY];
					room.monsters.Add (this);
					room.updateSprite ();

					transform.localPosition = new Vector3 (transform.localPosition.x, transform.localPosition.y, 100.0f);

			}

			transform.localScale = new Vector3 (1 - 2 * Random.Range (0, 2), 1, 1);
			updateAI (true);
			collisionCheck ();
		}
	}

	// AI for the monster
	void updateAI(bool reset) {
		if (room == null) {
			transform.position += new Vector3 (speed * transform.localScale.x, 0, 0);
			boundsCheck ();
			animator.Play("walk", -1, float.NegativeInfinity);

			spriteRenderer.sortingOrder = 1;
			if (sleepNeed > 0) {
				spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
			} else {
				spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f, 0.65f);
				speed = 0.04f;
				isLeaving = true;
			}
			return;
		} else {
			spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
			if (sleepNeed > 0 && room.room_type == "sleep") {
				aiState = "SLEEP";
				if (sleepNeed % 50 == 0) {
					int revenue = room.income;
					if (prefersAlone && room.monsters.Count > 1) revenue /= 5;
					if (prefersCompany && room.monsters.Count < 2) revenue /= 5;

					monsterManager.hotel.gold += revenue;

					Vector3 popUpPos = transform.localPosition; popUpPos.y += 1.0f; popUpPos.z = -5.0f;
					PopupText newPopupText = Instantiate(popupText, popUpPos, Quaternion.identity) as PopupText;
					newPopupText.text_display = "+" + revenue; newPopupText.text_color = Color.yellow;
				}
				sleepNeed--;

				animator.Play("sleep", -1, float.NegativeInfinity);
				spriteRenderer.sortingOrder = 0;
				aiStateDuration = 0;

				return;
			} else if (eatNeed > 0 && room.room_type == "eat") {
				aiState = "EAT";
				if (eatNeed % 50 == 0) {
					int revenue = room.income;
					
					monsterManager.hotel.gold += revenue;
					
					Vector3 popUpPos = transform.localPosition; popUpPos.y += 1.0f; popUpPos.z = -5.0f;
					PopupText newPopupText = Instantiate(popupText, popUpPos, Quaternion.identity) as PopupText;
					newPopupText.text_display = "+" + revenue; newPopupText.text_color = Color.yellow;
				}
				eatNeed--;
				
				animator.Play("eat", -1, float.NegativeInfinity);
				spriteRenderer.sortingOrder = 0;
				aiStateDuration = 0;
				
				return;
			} else if (funNeed > 0 && room.room_type == "fun") {
				aiState = "FUN";
				if (funNeed % 50 == 0) {
					int revenue = room.income;
					
					monsterManager.hotel.gold += revenue;
					
					Vector3 popUpPos = transform.localPosition; popUpPos.y += 1.0f; popUpPos.z = -5.0f;
					PopupText newPopupText = Instantiate(popupText, popUpPos, Quaternion.identity) as PopupText;
					newPopupText.text_display = "+" + revenue; newPopupText.text_color = Color.yellow;
				}
				funNeed--;
				
				animator.Play("fun", -1, float.NegativeInfinity);
				spriteRenderer.sortingOrder = 0;
				aiStateDuration = 0;
				
				return;
			}
		}

		if (reset) {
			aiState = "IDLE";
			aiStateDuration = Random.Range (120, 240);
			return;
		}

		if (aiStateDuration <= 0) {
			if (aiState != "MOVE") {
				aiState = "MOVE";
				aiStateDuration = Random.Range (60, 120);
				
				transform.localScale = new Vector3 (1-2*Random.Range(0,2), 1, 1);
				
				animator.Play("walk", -1, float.NegativeInfinity);
			} else if (aiState != "IDLE") {
				aiState = "IDLE";
				aiStateDuration = Random.Range (120, 240);
				
				animator.Play("idle", -1, float.NegativeInfinity);;
			}
		} else {
			aiStateDuration -= 1;
		}
		
		if (aiState == "MOVE") {
			transform.position += new Vector3 (speed * transform.localScale.x, 0, 0);
			collisionCheck();
		}

		return;
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
			if (sleepNeed > 0) { 
				monsterManager.hotel.addHappiness (-200.0f);
			} else {
				if (eatNeed <= 0) monsterManager.hotel.addHappiness (50.0f);
				if (funNeed <= 0) monsterManager.hotel.addHappiness (50.0f);
			}
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

	public Queue<Shaft> getReachableShafts() {
		Queue<Shaft> returnShafts = null;

		return returnShafts;
	}
}
