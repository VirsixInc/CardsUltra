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

	Vector3 LerpWithoutClamp(Vector3 A, Vector3 B, float t){
		return A + (B - A) * t;
	}


	void Update() {
		//reset when we press spacebar	
	
		if (currentLerpTime < lerpTime) {
			currentLerpTime += Time.deltaTime; //nice because you dont need a bool for completion
			float t = currentLerpTime / lerpTime;

			t = Mathf.Clamp01(t);
			t = (Mathf.Sin(t * Mathf.PI * (0.2f + 2.5f * t * t * t)) * Mathf.Pow(1f - t, 2.2f) + t) * (1f + (1.2f * (1f - t)));
			print (t);
			transform.localPosition = LerpWithoutClamp(endPos, center, t);
		}
		

	}
}
