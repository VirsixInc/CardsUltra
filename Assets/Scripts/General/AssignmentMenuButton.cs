using UnityEngine;
using System.Collections;

public class AssignmentMenuButton : MonoBehaviour {

	float selectTimer;

	void OnFingerDown () {
		selectTimer = Time.time;

	}

	void OnFingerUp () {
		if (Time.time - selectTimer < 1) {
			//guimanager 
		}
	}
}
