using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ScrollingMenu : MonoBehaviour {

	float velocity = 0, deceleration = -1f;
	float accelerationRatio = .3f;

	void Start () {

	}


	void OnGUI () {
		Event e = Event.current;
		if (e.type == EventType.mouseDrag) {
			velocity += accelerationRatio*Mathf.Ceil(e.delta.y);
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
		transform.Translate (new Vector3 (0, velocity, 0));
	}
}
