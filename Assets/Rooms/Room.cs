using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Room : MonoBehaviour {
	public int width, height;
	public int cellX, cellY;
	public int cost;
	public int income;
	public int capacity;
	public List<Monster> monsters;

	private bool highlighted;

	protected SpriteRenderer spriteRenderer;
	protected GameObject ConstructionEffect;
	protected RoomManager roomMgr;

	// Use this for initialization
	void Awake () {
		spriteRenderer = GetComponent<SpriteRenderer>();
		if (monsters.Count < capacity)
			spriteRenderer.color = new Color (0.4f, 0.4f, 0.4f, 1f);

		ConstructionEffect = Resources.Load ("Effects/Prefabs/Dust Cloud Particle") as GameObject;

		highlighted = false;
	}

	public void updateSprite() {
		if (!highlighted) {
			if (capacity == 0 || monsters.Count > 0) {
				spriteRenderer.color = new Color (1f, 1f, 1f, 1f);
			} else {
				spriteRenderer.color = new Color (0.4f, 0.4f, 0.4f, 1f);
			}
		}
	}

	public void highlightSprite(Color color, bool turnOnHighlight) {
		if (turnOnHighlight == false) {
			highlighted = false;
			updateSprite ();
		} else {
			highlighted = true;
			spriteRenderer.color = color;
		}
	}

	public virtual void Destroy() {
		for (int i = 0; i < width; i++) {
			for (int j = 0; j < height; j++) {
				Instantiate(ConstructionEffect, new Vector3((cellX+i)*2.0f, (cellY+j)*2.0f, 0.0f), Quaternion.identity);
			}
		}
		Destroy (gameObject);
	}
}