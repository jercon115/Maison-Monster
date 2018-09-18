using UnityEngine;
using System.Collections;

public class GameCamera : MonoBehaviour {

	public GameObject horizon;
	public int maxOrthoSize, minOrthoSize;

	public AudioClip[] musicTracks;
	private AudioSource music;
	private int prevTrack;

	bool minusDown = false, equalsDown = false;

	void Start () {
		music = GetComponent<AudioSource> ();
		prevTrack = Mathf.FloorToInt (Random.Range (0, musicTracks.Length + 1));
		music.clip = musicTracks[prevTrack];
		music.Play ();
	}

	void Update () {
		// Camera music
		if (!music.isPlaying) {
			prevTrack = ( prevTrack + Mathf.FloorToInt(Random.Range(1,musicTracks.Length-1)) ) % musicTracks.Length;
			music.clip = musicTracks[prevTrack];
			music.Play ();
		}

		// Camera movement
		int vert = 0, horiz = 0;
		if (Input.GetKey (KeyCode.W) || Input.GetKey (KeyCode.UpArrow))
			vert++;
		if (Input.GetKey (KeyCode.S) || Input.GetKey (KeyCode.DownArrow))
			vert--;
		if (Input.GetKey (KeyCode.D) || Input.GetKey (KeyCode.RightArrow)) {
			horiz++;
			Vector3 horizPos = horizon.transform.localPosition; horizPos.x += 0.07f;
			horizon.transform.localPosition = horizPos;
		}
		if (Input.GetKey (KeyCode.A) || Input.GetKey (KeyCode.LeftArrow)) {
			horiz--;
			Vector3 horizPos = horizon.transform.localPosition; horizPos.x -= 0.07f;
			horizon.transform.localPosition = horizPos;
		}
		transform.Translate (new Vector3(horiz*0.1f,vert*0.1f));

		// Camera zooming

		if (Input.GetKey (KeyCode.Minus)) {
		    if (!minusDown)  {
				minusDown = true;
				if (Camera.main.orthographicSize <= maxOrthoSize)
                    Camera.main.orthographicSize++;
			}
		} else {
			minusDown = false;
		}
		if (Input.GetKey (KeyCode.Equals)) {
			if (!equalsDown)  {
				equalsDown = true;
				if (Camera.main.orthographicSize >= minOrthoSize)
                    Camera.main.orthographicSize--;
			}
		} else {
			equalsDown = false;
		}
		if (Input.GetAxis ("Mouse ScrollWheel") < 0) {
			if (Camera.main.orthographicSize <= maxOrthoSize)
                Camera.main.orthographicSize++;
		}
		if (Input.GetAxis ("Mouse ScrollWheel") > 0) {
			if (Camera.main.orthographicSize >= minOrthoSize)
                Camera.main.orthographicSize--;
		}
	}
}
