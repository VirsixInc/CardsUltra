using UnityEngine;
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

	void OnFingerDown () {
		selectTimer = Time.time;
		positionAtFingerDown = transform.localPosition;
	}

	
	// Use this for initialization
	void Start () {
		thisEventTrigger = GetComponent<EventTrigger>();
		entry.eventID = EventTriggerType.PointerUp;
		entry2.eventID = EventTriggerType.PointerDown;

		entry.callback.AddListener( (eventData) => {CallManager(); } );
		entry2.callback.AddListener( (eventData) => {OnFingerDown(); } );

		thisEventTrigger.triggers.Add(entry);
		thisEventTrigger.triggers.Add(entry2);

	}

	void CallManager() {
		if (Time.time - selectTimer < .15f && Vector3.Distance(positionAtFingerDown, transform.localPosition) < 1f && !ScrollingMenu.s_instance.isInMenu) {
			//		SoundManager.s_instance.PlaySound (SoundManager.s_instance.m_start);
			GameObject.FindGameObjectWithTag ("scrollingMenu").GetComponent<ScrollingMenu>().currentLevelToBePlayed =
				gameObject.GetComponent<AssignmentGUI>().assignmentIndex;
			GUIManager.s_instance.EnableMenu();

			//AppManager.s_instance.currentAssignment = transform.parent.GetComponent<Assignment> ();
			//		AppManager.s_instance.currentAppState = AppState.Playing;
		}

	}
}
