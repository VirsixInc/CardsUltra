using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(EventTrigger))]

public class DynamicButton : MonoBehaviour {

	float lerpTime = .2f;
	float currentLerpTime = .3f;
	float enlargeScale = 1f;

	public Color highlighted, idle;
	bool isIdle = true;
	Vector3 idleSize;
	Vector3 highLightedSize;

	EventTrigger thisEventTrigger;
	EventTrigger.Entry entry = new EventTrigger.Entry();
	EventTrigger.Entry entry2 = new EventTrigger.Entry();

	PointerEventData eventData;
	
	// Use this for initialization
	void Start () {
		gameObject.GetComponent<Image> ().color = idle;
		thisEventTrigger = GetComponent<EventTrigger>();
		entry.eventID = EventTriggerType.PointerDown;
		entry2.eventID = EventTriggerType.PointerUp;
		entry.callback.AddListener( (eventData) => {OnFingerDown(); } );
		entry2.callback.AddListener( (eventData) => {OnFingerUp (); } );

		thisEventTrigger.triggers.Add(entry);
		thisEventTrigger.triggers.Add(entry2);

		
		idleSize = new Vector3(1f,1f,1f);
		highLightedSize = idleSize * enlargeScale;
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
				transform.localScale = Vector3.Lerp (idleSize, highLightedSize, t);
				gameObject.GetComponent<Image> ().color = Color.Lerp (idle, highlighted, t);
			}
		} else {
			currentLerpTime += Time.deltaTime; //nice because you dont need a bool for completion
			float t = currentLerpTime / lerpTime;
			transform.localScale = Vector3.Lerp (highLightedSize, idleSize, t);
			gameObject.GetComponent<Image> ().color = Color.Lerp (highlighted, idle, t);
		}
		
	}


}
