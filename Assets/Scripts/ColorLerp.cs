using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ColorLerp : MonoBehaviour {
	float lerpTime = .2f;
	float currentLerpTime = .3f;

	
	public Color highlighted, idle;
	bool isIdle = true;

	
	EventTrigger thisEventTrigger;
	EventTrigger.Entry entry = new EventTrigger.Entry();
	EventTrigger.Entry entry2 = new EventTrigger.Entry();
	
	PointerEventData eventData;
	
	// Use this for initialization
	void Start () {
		idle = GetComponent<Image> ().color;
		gameObject.GetComponent<Image> ().color = idle;
		thisEventTrigger = GetComponent<EventTrigger>();
		entry.eventID = EventTriggerType.PointerDown;
		entry2.eventID = EventTriggerType.PointerUp;
		entry.callback.AddListener( (eventData) => {OnFingerDown(); } );
		entry2.callback.AddListener( (eventData) => {OnFingerUp (); } );
		
		thisEventTrigger.triggers.Add(entry);
		thisEventTrigger.triggers.Add(entry2);
	}
	
	public void OnFingerDown() {
		isIdle = true;
		StartLerp ();
	}
	
	public void OnFingerUp(){
		isIdle = false;
		StartLerp ();
		
	}
	
	void StartLerp() {
		currentLerpTime = 0;
	}
	
	void Update() {
		if (isIdle) {
			if (currentLerpTime < lerpTime) {
				currentLerpTime += Time.deltaTime; //nice because you dont need a bool for completion
				float t = currentLerpTime / lerpTime;
				gameObject.GetComponent<Image> ().color = Color.Lerp (idle, highlighted, t);
			}
		} else {
			currentLerpTime += Time.deltaTime; //nice because you dont need a bool for completion
			float t = currentLerpTime / lerpTime;
			gameObject.GetComponent<Image> ().color = Color.Lerp (highlighted, idle, t);
		}
		
	}
}
