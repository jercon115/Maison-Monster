using UnityEngine;
using System.Collections;

public class ConstructionGUIManager : MonoBehaviour {

	public Hotel hotel;
	public Guidelines guidelines;
	public GameObject target;
	public Canvas canvas;
	private SpriteRenderer targetRenderer;

	private int selected;
	private int roomCount;
	private Sprite[] roomSprites;
	private GameObject[] palette;

	private int targetX;
	private int targetY;


	// Use this for initialization
	void Start () {
		selected = -1;
		roomCount = hotel.rooms.Length;

		guidelines.setSize (hotel.width, hotel.height);
		targetRenderer = target.GetComponent<SpriteRenderer>();

		targetX = 0; targetY = 0;

		updateRoomSprites ();
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		Vector3 canvasPos = mousePos - Camera.main.transform.localPosition;

		setTargetPosition (mousePos);
		if (canvasPos.x <= -4.0) {
			if (Input.GetMouseButtonDown (0)) {
				int newSelection = Mathf.FloorToInt ((5.0f-canvasPos.y)/2.0f);
				updateSelected (newSelection);
				print (newSelection);
			}
		} else {
			float xTemp = mousePos.x/2.0f + 0.5f;
			int y = Mathf.FloorToInt (mousePos.y/2.0f + 0.5f);

			if (Input.GetMouseButtonDown (0) && roomSelected ()) {
				int width = hotel.rooms[selected].width;
				xTemp -= (width-1)*0.5f; int x = Mathf.FloorToInt (xTemp);
				
				print ("X: " + x + "Y: " +y);
				hotel.roomManager.MakeRoom (x, y, hotel.rooms[selected]);
			} else if (Input.GetMouseButtonDown (1)) {
				int x = Mathf.FloorToInt (xTemp);

				print ("X: " + x + "Y: " +y);
				hotel.roomManager.DeleteRoom (x, y);
				updateTargetSprite();
			}
		}
	}

	void setTargetPosition(Vector3 mousePos) {
		int width = 0;
		if (roomSelected ())
			width = hotel.rooms [selected].width;

		int newTargetX = Mathf.FloorToInt (mousePos.x/2.0f - (width-1)*0.5f + 0.5f),
			newTargetY = Mathf.FloorToInt (mousePos.y/2.0f + 0.5f);

		if (newTargetX != targetX || newTargetY  != targetY) {
			targetX = newTargetX; targetY = newTargetY;
			updateTargetSprite();

		}
	}

	void updateTargetSprite() {
		if (roomSelected ()) {
			int width = hotel.rooms [selected].width;
			int height = hotel.rooms [selected].height;

			float x = targetX * 2.0f + (width - 1),
			y = targetY * 2.0f;

			target.transform.localPosition = new Vector3 (x, y, 10.0f);

			if (hotel.roomManager.roomLocationValid (targetX, targetY, width, height)) {
					targetRenderer.color = new Color32 (255, 255, 255, 128);
			} else {
					targetRenderer.color = new Color32 (255, 0, 0, 128);
			}
		}
	}

	void updateRoomSprites() {
		roomSprites = new Sprite[roomCount];
		palette = new GameObject[roomCount];
		for(int i = 0; i < roomCount; i++) {
			roomSprites[i] = hotel.rooms[i].GetComponent<SpriteRenderer>().sprite;

			palette[i] = Instantiate (Resources.Load ("EmptySprite")) as GameObject;

			palette[i].transform.Translate(-6.0f , 4.0f - (2.0f * i) , 0.0f);
			palette[i].transform.localScale = new Vector3(0.75f, 0.75f);
			palette[i].transform.parent = canvas.transform;

			SpriteRenderer renderer = palette[i].GetComponent<SpriteRenderer>();
			renderer.sprite = roomSprites[i];
			renderer.sortingLayerName = "UI";
		}
	}

	void updateSelected(int newSelection) {
		selected = newSelection;
		if (roomSelected()) {
			targetRenderer.sprite = roomSprites[selected];
			print ("changed: " + selected);
		} else
			targetRenderer.sprite = null;
	}

	bool roomSelected() {
		return (selected >= 0 && selected < roomCount);
	}
}
