using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ScrollingMenu : MonoBehaviour {

	float velocity = 0, deceleration = -1f;
	float accelerationRatio = .3f;
	public float lowerBound;
	float startTime, lerpTime = 1f, fracJourney;
	bool isLerpingBackInBounds = false;
	Vector3 lowerBoundPosition;
	void Start () {

	}


	void OnGUI () {
		Event e = Event.current;
		if (e.type == EventType.mouseDrag) {
			velocity -= accelerationRatio*Mathf.Ceil(e.delta.y);
		}
		if (Mathf.Abs (velocity) > 1) {
			velocity += deceleration * (Mathf.Abs (velocity) / velocity);
		} else if (Mathf.Abs (velocity) > .3f) {
			velocity += deceleration/10 * (Mathf.Abs (velocity) / velocity);
		} else {
			velocity = 0;
		}
	}

	void Update () {
		if (isLerpingBackInBounds == false) {
			transform.Translate (new Vector3 (0, velocity, 0));
		}
//Reset
//USING LERP
		if (transform.localPosition.y < 0 || transform.localPosition.y > lowerBound) {
			velocity = 0;
			if (isLerpingBackInBounds == false) {
				Rebound();
			}
			fracJourney = (Time.time - startTime)/lerpTime;
			if (fracJourney > .9f) {
				fracJourney = 1;
				isLerpingBackInBounds = false;
			}
			if (transform.localPosition.y < 0) {
				transform.localPosition = Vector3.Lerp(transform.localPosition, lowerBoundPosition, fracJourney);
			}
			else if (transform.localPosition.y > lowerBound){
				transform.localPosition = Vector3.Lerp(transform.localPosition, Vector3.zero, fracJourney);
			}
		}

//USING TRANSLATE
//		if (transform.localPosition.y < 0) {
//			velocity = 0f;
//			transform.Translate (new Vector3 (0, 1f, 0));
//			isLerpingBackInBounds = true;
//		}
//		if (transform.localPosition.y > lowerBound) {
//			velocity = 0f;
//			transform.Translate (new Vector3 (0, -1f, 0));
//			isLerpingBackInBounds = true;
//		}
//
		if (transform.localPosition.y >= 0 && transform.localPosition.y <= lowerBound) {
			isLerpingBackInBounds = false;
		}

	}

	void Rebound() {
		isLerpingBackInBounds = true;
		startTime = Time.time;
	}

}
