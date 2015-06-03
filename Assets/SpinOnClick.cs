using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

[RequireComponent(typeof(EventTrigger))]

public class SpinOnClick : MonoBehaviour {

	bool isRotating = false; 
	float angle = 360.0f; // Degree per time unit
	float time = 1.0f; // Time unit in sec
	Vector3 axis = Vector3.up; // Rotation axis, here it the yaw axis

	float rotationleft=360;
	float rotationspeed=800;

	EventTrigger thisEventTrigger;
	EventTrigger.Entry entry = new EventTrigger.Entry();
	PointerEventData eventData;
	
	// Use this for initialization
	void Start () {
		thisEventTrigger = GetComponent<EventTrigger>();
		entry.eventID = EventTriggerType.PointerUp;
		entry.callback.AddListener( (eventData) => {OnFingerUp(); } );
		thisEventTrigger.delegates.Add(entry);
	}

	
	private void Update()
	{
		float rotation = rotationspeed * Time.deltaTime;
		if (isRotating) {
			if (rotationleft > rotation) {
				rotationleft -= rotation;
			} else {
				rotation = rotationleft;
				rotationleft = 0;
				isRotating = false;
			}
			transform.Rotate (0, 0, rotation);	
		}
	}
	public void OnFingerUp() {
		print ("FINGERUP");
		isRotating = true;
	}
}
