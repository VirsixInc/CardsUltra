﻿using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

[RequireComponent(typeof(EventTrigger))]
public class AssignmentStartButton : MonoBehaviour {

	EventTrigger thisEventTrigger;
	EventTrigger.Entry entry = new EventTrigger.Entry();
	EventTrigger.Entry entry2 = new EventTrigger.Entry();
	PointerEventData eventData;
	Vector3 positionAtFingerDown;
	float selectTimer;
	GameObject mainMenuCanvas;
	
	void OnFingerDown () {
		selectTimer = Time.time;
		positionAtFingerDown = transform.position;
	}

	
	// Use this for initialization
	void Start () {
		mainMenuCanvas = GameObject.FindGameObjectWithTag ("mainMenuCanvas");
		thisEventTrigger = GetComponent<EventTrigger>();
		entry.eventID = EventTriggerType.PointerUp;
		entry2.eventID = EventTriggerType.PointerDown;

		entry.callback.AddListener( (eventData) => {CallManager(); } );
		entry2.callback.AddListener( (eventData) => {OnFingerDown(); } );

		thisEventTrigger.delegates.Add(entry);
		thisEventTrigger.delegates.Add(entry2);

	}

	void CallManager() {
		if (Time.time - selectTimer < .2f && Vector3.Distance(positionAtFingerDown, transform.position) < 1f) {
			//		SoundManager.s_instance.PlaySound (SoundManager.s_instance.m_start);
			GameObject.FindGameObjectWithTag ("scrollingMenu").GetComponent<ScrollingMenu>().currentLevelToBePlayed =
				gameObject.GetComponent<AssignmentGUI>().assignmentIndex;
			mainMenuCanvas.GetComponent<MenuButtonManager>().EnableMenu();
			GameObject.FindGameObjectWithTag("MainCamera").GetComponent<BlurLerp>().Blur();

			//AppManager.s_instance.currentAssignment = transform.parent.GetComponent<Assignment> ();
			//		AppManager.s_instance.currentAppState = AppState.Playing;
		}

	}
}
