using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Room : MonoBehaviour {

	public int width;
	public int cellX, cellY;
	public int cost;
	public int income;
	public int capacity;
	public List<Monster> monsters;

	private SpriteRenderer spriteRenderer;

	// Use this for initialization
	void Start () {
		spriteRenderer = GetComponent<SpriteRenderer>();
		if (monsters.Count < capacity)
			spriteRenderer.color = new Color (0.4f, 0.4f, 0.4f, 1f);
	}

	public void updateSprite() {
		if (monsters.Count > 0) {
			spriteRenderer.color = new Color (1f, 1f, 1f, 1f);
		} else {
			spriteRenderer.color = new Color (0.4f, 0.4f, 0.4f, 1f);
		}
	}
}