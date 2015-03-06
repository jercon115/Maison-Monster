using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Hotel : MonoBehaviour {

	// Game variables //
	public int width;
	public int height;
	public int gold;

	private float happiness;
	private float maxHappiness;
	private int numStars;

	// Managers //
	public Room[] rooms;
	public RoomManager roomManager;	// Grid of rooms
	public MonsterManager monsterManager;

	// Hud //
	public Sprite[] faces;
	public Text goldText;
	public Slider hp_bar;
	public Image hp_face;
	public Image[] hud_stars;
	public Sprite[] stars;

	void Start() {
		gold = 2000;
		maxHappiness = 1000; happiness = maxHappiness/2;
		hp_face.sprite = faces [2];
		hp_bar.value = happiness/maxHappiness;
	}

	void Update() {
		goldText.text = gold.ToString();
	}

	public void addHappiness(float num) {
		happiness += num;
		hp_bar.value = happiness/maxHappiness;
		float hp = hp_bar.value;
		if (hp < 1.0 && hp >= 0.8) {
			hp_face.sprite = faces [4];
		} else if (hp < 0.8 && hp >= 0.6) {
			hp_face.sprite = faces [3];
		} else if (hp < 0.6 && hp >= 0.4) {
			hp_face.sprite = faces [2];
		} else if (hp < 0.4 && hp >= 0.2) {
			hp_face.sprite = faces [1];
		} else if (hp < 0.2 && hp > 0.0) {
			hp_face.sprite = faces [0];
		} else if (hp <= 0.0) {
			print ("GameOver");
			Application.LoadLevel("GameOver");
		}
	}

	public int getNumStars() {
		return numStars;
	}

	public void addStar (int num) {
		numStars += num;
		if (numStars  == 0) {
			hud_stars[0].sprite = stars[0];
			hud_stars[1].sprite = stars[0];
			hud_stars[2].sprite = stars[0];
			hud_stars[3].sprite = stars[0];
			hud_stars[4].sprite = stars[0];
		} else if (numStars == 1) {
			hud_stars[0].sprite = stars[1];
			hud_stars[1].sprite = stars[0];
			hud_stars[2].sprite = stars[0];
			hud_stars[3].sprite = stars[0];
			hud_stars[4].sprite = stars[0];
		} else if (numStars  == 2) {
			hud_stars[0].sprite = stars[1];
			hud_stars[1].sprite = stars[1];
			hud_stars[2].sprite = stars[0];
			hud_stars[3].sprite = stars[0];
			hud_stars[4].sprite = stars[0];
		} else if (numStars  == 3) {
			hud_stars[0].sprite = stars[1];
			hud_stars[1].sprite = stars[1];
			hud_stars[2].sprite = stars[1];
			hud_stars[3].sprite = stars[0];
			hud_stars[4].sprite = stars[0];
		} else if (numStars  == 4) {
			hud_stars[0].sprite = stars[1];
			hud_stars[1].sprite = stars[1];
			hud_stars[2].sprite = stars[1];
			hud_stars[3].sprite = stars[1];
			hud_stars[4].sprite = stars[0];
		} else if (numStars  == 5) {
			hud_stars[0].sprite = stars[1];
			hud_stars[1].sprite = stars[1];
			hud_stars[2].sprite = stars[1];
			hud_stars[3].sprite = stars[1];
			hud_stars[4].sprite = stars[1];
		} else if (numStars > 5) {
			Application.LoadLevel("Win");
		}
	}
}
