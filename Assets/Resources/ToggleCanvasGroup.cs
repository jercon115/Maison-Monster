using UnityEngine;
using System.Collections;

public class ToggleCanvasGroup : MonoBehaviour {

	private CanvasGroup canvasGroup;

	// Use this for initialization
	void Awake () {
		canvasGroup = GetComponent<CanvasGroup> ();
	}
	
	public void toggleAlphaAndInteractivity() {
		if (canvasGroup.alpha == 0 || canvasGroup.interactable == false) {
			canvasGroup.alpha = 1;
			canvasGroup.interactable = true;
		} else if (canvasGroup.alpha == 1 || canvasGroup.interactable == true) {
			canvasGroup.alpha = 0;
			canvasGroup.interactable = false;
		}
	}
}
