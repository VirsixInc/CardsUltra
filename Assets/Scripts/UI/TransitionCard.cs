using UnityEngine;
using System.Collections;

//this UI card script makes a card fly to a position with a certain speed determined by lerp time


public class TransitionCard : MonoBehaviour {
	
	float lerpTime = 3f;
	float currentLerpTime =5f;
	
	public bool isOnScreen = false;
	public Vector3 center;
	Vector3 endPos;
	public Transform startTransform;
	void Awake() {
		center = new Vector3(0f,0f,0f);
		endPos = startTransform.localPosition;
	}

	public void StartLerpToOffScreen() {
		currentLerpTime = 0;
		isOnScreen = true;
	}

	public void StartLerpToOnScreen() {
		currentLerpTime = 0;
		isOnScreen = false;
	}

	Vector3 LerpWithoutClamp(Vector3 A, Vector3 B, float t){
		return A + (B - A) * t;
	}


	void Update() {
		//reset when we press spacebar	
	
		if (currentLerpTime < lerpTime && isOnScreen == false) {
			currentLerpTime += Time.deltaTime; //nice because you dont need a bool for completion
			float t = currentLerpTime / lerpTime;
			t = Mathf.Clamp01 (t);
			t = (Mathf.Sin (t * Mathf.PI * (0.2f + 2.5f * t * t * t)) * Mathf.Pow (1f - t, 2.2f) + t) * (1f + (1.2f * (1f - t)));
			transform.localPosition = LerpWithoutClamp (endPos, center, t);
		} 

		else if (currentLerpTime < lerpTime && isOnScreen == true) {
			currentLerpTime += Time.deltaTime;
			float t = currentLerpTime / lerpTime;
			t = Mathf.Clamp01 (t);
			t = (Mathf.Sin (t * Mathf.PI * (0.2f + 2.5f * t * t * t)) * Mathf.Pow (1f - t, 2.2f) + t) * (1f + (1.2f * (1f - t)));
			transform.localPosition = LerpWithoutClamp (center, endPos, t);
		}

		

	}
}
