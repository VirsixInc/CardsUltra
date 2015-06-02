using UnityEngine;
using System.Collections;

//this UI card script makes a card fly to a position with a certain speed determined by lerp time


public class TransitionCard : MonoBehaviour {
	
	float lerpTime = 3f;
	float currentLerpTime;
	
	float moveDistance = 10f;
	
	Vector3 center;
	Vector3 endPos;
	public Transform startTransform;
	void Start() {
		center = new Vector3(0f,0f,0f);
		endPos = startTransform.localPosition;
		StartLerp ();
	}

	void StartLerp() {
		currentLerpTime = 0;
	}

	void Update() {
		//reset when we press spacebar	
	
		if (currentLerpTime < lerpTime) {
			currentLerpTime += Time.deltaTime; //nice because you dont need a bool for completion
			float t = currentLerpTime / lerpTime;
			Mathf.Pow(t,3);
			t = t*t*t*(t*(6f*t-15f) + 10f);
			transform.localPosition = Vector3.Lerp(endPos, center, t);
		}
		

	}
}
