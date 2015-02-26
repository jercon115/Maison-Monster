using UnityEngine;
using System.Collections;

public class PopupText : MonoBehaviour {

	public int duration;
	public float speed;
	public string text_display;
	public Color text_color;

	private TextMesh textMesh;

	// Use this for initialization
	void Start () {
		textMesh = GetComponent<TextMesh>();
		textMesh.text = text_display;
	}
	
	// Update is called once per frame
	void Update () {
		if (duration > 0) {
			textMesh.text = text_display;
			textMesh.color = text_color;
			transform.localPosition = new Vector3 (
				transform.localPosition.x, transform.localPosition.y + speed, transform.localPosition.z);
			duration -= 1;
		} else {
			Destroy(gameObject);
		}
	}
}
