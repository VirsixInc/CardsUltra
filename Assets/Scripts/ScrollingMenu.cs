using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ScrollingMenu : MonoBehaviour {

	float velocity = 0, deceleration = -1f;
	float accelerationRatio = .1f;
	public float lowerBound;
	float startTime, lerpTime = 1f, fracJourney;
	bool isLerpingBackInBounds = false;
	Vector3 lowerBoundPosition = Vector3.zero; //assigned from other class no good
	public bool isDragging = false, isSwiping = false;
	GameObject myCanvas;
	Vector2 pos;
	float currentY, initY;

	public static ScrollingMenu s_instance;

	void Awake () {
		s_instance = this;
	}

	void Start () {
		myCanvas = GameObject.Find ("Canvas");
	}

	void OnGUI () {
		Event e = Event.current;
		if (e.type == EventType.mouseDown) {
			isSwiping = false;
			isDragging = true;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(myCanvas.transform as RectTransform, Input.mousePosition, myCanvas.GetComponent<Canvas>().worldCamera, out pos);
			initY = pos.y - transform.localPosition.y;
		}
		if (e.type == EventType.mouseUp) {
			isDragging = false;
		} 
		if (e.delta.y > 0) {
			isSwiping = true;
			print ("SWIPE");
		}
	

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
	void Rebound() {
		isLerpingBackInBounds = true;
		startTime = Time.time;
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

		if (transform.localPosition.y >= 0 && transform.localPosition.y <= lowerBound) {
			isLerpingBackInBounds = false;
		}

		if (isDragging) {
			RectTransformUtility.ScreenPointToLocalPointInRectangle(myCanvas.transform as RectTransform, Input.mousePosition, myCanvas.GetComponent<Canvas>().worldCamera, out pos);
			currentY = pos.y;
			transform.localPosition = new Vector3(0f, -(initY-currentY), 0f);
		}
		
	
		
//		scrollbar.value = transform.localPosition.y / GUIManager.s_instance.upperBound;
		
	}

}
