using UnityEngine;
using System.Collections;

public class Monster : MonoBehaviour {

	public Sprite[] sprites;
	private SpriteRenderer spriteRenderer;

	public MonsterManager monsterManager;
	public Room room;

	private int hotelWidth;
	private float hotelBounds = 8.0f;

	private float speed;
	private float velY;

	private bool clicked;
	private string aiState;
	private int aiStateDuration;
	private int sleepNeed;

	// Use this for initialization
	void Start () {
		spriteRenderer = GetComponent<SpriteRenderer>();
		
		speed = 0.01f;
		velY = 0.0f;
		aiState = "IDLE";
		aiStateDuration = Random.Range (120, 240);
		sleepNeed = 500;
	}

	public void Initialize(int startHotelWidth) {
		hotelWidth = startHotelWidth;
		float randomX = -hotelBounds;
		if (Random.Range(0, 2) == 0) {
			randomX = 2.0f * hotelWidth + hotelBounds - 1.5f;
			transform.localScale = new Vector3 (-transform.localScale.x, 1, 1);
		}
		transform.localPosition = new Vector3 (randomX, 0, 100.0f);
	}
	
	// Update is called once per frame
	void Update () {
		if (clicked) {
			spriteRenderer.sprite = sprites [3];
			Vector3 mousePos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
			transform.localPosition = new Vector3 (mousePos.x, mousePos.y, 100.0f);
		} else {
			// Check if above ground first
			float ground = 0.0f;
			if (room != null) ground = room.transform.localPosition.y;

			if (transform.localPosition.y > ground) {
				velY -= 0.01f;
				transform.Translate(new Vector3(0.0f, velY, 0.0f));
				if (transform.localPosition.y > ground) {
					spriteRenderer.sprite = sprites [3];
				} else
					spriteRenderer.sprite = sprites[0];
			} else {
				velY = 0.0f;
				transform.localPosition = new Vector3(transform.localPosition.x,
				                                      ground,
				                                      transform.localPosition.z);

				updateAI(false);
			}
		}
	}

	// Click a monster
	void OnMouseDown() {
		clicked = true;

		if (room != null) {
			room.monsters.Remove(this);
			room.updateSprite();
		}
		room = null;
		transform.localPosition = new Vector3 (transform.localPosition.x, transform.localPosition.y, 100.0f);
	}

	// Let go of a monster
	void OnMouseUp() {
		spriteRenderer.sprite = sprites[0];
		clicked = false;

		int cellX = Mathf.FloorToInt (transform.localPosition.x/2.0f + 0.5f),
			cellY = Mathf.FloorToInt (transform.localPosition.y/2.0f + 0.5f);

		Room[,] cells = monsterManager.roomManager.cells;
		if (cells [cellX, cellY] != null && cells [cellX, cellY].monsters.Count < cells [cellX, cellY].capacity) {
			room = cells [cellX, cellY];
			room.monsters.Add(this);
			room.updateSprite();

			transform.localPosition = new Vector3 (transform.localPosition.x, transform.localPosition.y, 100.0f);

		}

		transform.localScale = new Vector3 (1-2*Random.Range(0,2), 1, 1);
		updateAI(true);
		collisionCheck();
	}

	// AI for the monster
	void updateAI(bool reset) {
		if (room == null) {
			transform.position += new Vector3 (speed * transform.localScale.x, 0, 0);
			boundsCheck ();
			spriteRenderer.sprite = sprites[1];

			spriteRenderer.sortingOrder = 1;
			spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f, 1f);
			return;
		} else {
			spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
			if (sleepNeed > 0) {
				aiState = "SLEEP";
				if (sleepNeed % 5 == 0) monsterManager.hotel.gold += room.income;
				sleepNeed--;

				spriteRenderer.sprite = sprites [2];
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
				
				spriteRenderer.sprite = sprites[1];
			} else if (aiState != "IDLE") {
				aiState = "IDLE";
				aiStateDuration = Random.Range (120, 240);
				
				spriteRenderer.sprite = sprites [0];
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
			print (hotelWidth);
			monsterManager.deleteMonster (this);
		}
	}
}
