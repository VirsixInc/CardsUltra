﻿using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

[RequireComponent(typeof(EventTrigger))]
public class AssignmentStartButton : MonoBehaviour {

	EventTrigger thisEventTrigger;
	EventTrigger.Entry entry = new EventTrigger.Entry();
	EventTrigger.Entry entry2 = new EventTrigger.Entry();

	PointerEventData eventData;

	float selectTimer;
	
	void OnFingerDown () {
		selectTimer = Time.time;
		
	}

	
	// Use this for initialization
	void Start () {
		thisEventTrigger = GetComponent<EventTrigger>();
		entry.eventID = EventTriggerType.PointerUp;
		entry2.eventID = EventTriggerType.PointerDown;

		entry.callback.AddListener( (eventData) => {CallManager(); } );
		entry2.callback.AddListener( (eventData) => {OnFingerDown(); } );

		thisEventTrigger.delegates.Add(entry);
		thisEventTrigger.delegates.Add(entry2);

	}

	void CallManager() {
		if (Time.time - selectTimer < .2f && ScrollingMenu.s_instance.isSwiping ==false) {
			//guimanager 
			//		SoundManager.s_instance.PlaySound (SoundManager.s_instance.m_start);
			AppManager.s_instance.ClickHandler(gameObject.GetComponent<AssignmentGUI>().assignmentIndex);
			//AppManager.s_instance.currentAssignment = transform.parent.GetComponent<Assignment> ();
			//		AppManager.s_instance.currentAppState = AppState.Playing;
		}

	}
}
