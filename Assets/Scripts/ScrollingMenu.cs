using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ScrollingMenu : MonoBehaviour {

	float velocity = 0, deceleration = -.5f;
	float accelerationRatio = .8f;
	public float lowerBound;
	float startTime, lerpTime = 3f, fracJourney;
	bool isLerpingBackInBounds = false;
	public Vector3 lowerBoundPosition; //assigned from other class no good
	public bool isDragging = false, isSwiping = false;
	GameObject myCanvas;
	Vector2 pos;
	float currentY, initY;
	public int currentLevelToBePlayed;
	public bool isInMenu = false;

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

		if (e.type == EventType.mouseDrag) {
			velocity = -accelerationRatio*Mathf.Ceil(e.delta.y);
		}

	}
	void Rebound() {
		print ("rebound");
		isLerpingBackInBounds = true;
		startTime = Time.time;
	}

	void Update () {
		if (isInMenu == false) {
			//this block of code handles of the velocity of scrolling
			if (isLerpingBackInBounds == false && velocity != 0) {
				transform.Translate (new Vector3 (0, velocity, 0));
				if (Mathf.Abs (velocity) > 1) {
					velocity += deceleration * (Mathf.Abs (velocity) / velocity);
				} else if (Mathf.Abs (velocity) > .3f) {
					velocity += deceleration / 10 * (Mathf.Abs (velocity) / velocity);
				} else {
					velocity = 0;
				}		
			}
//Reset
//USING LERP
			if (transform.localPosition.y <= 0 || transform.localPosition.y >= lowerBound) {
				velocity = 0;

				fracJourney = (Time.time - startTime) / lerpTime;
				if (fracJourney > .9f) {
				fracJourney = 1;
					isLerpingBackInBounds = false;
					velocity = 0;
				}


				if (transform.localPosition.y <= 0) {
					transform.localPosition = Vector3.Lerp (transform.localPosition, lowerBoundPosition, fracJourney);
				} else if (transform.localPosition.y >= lowerBound) {
					transform.localPosition = Vector3.Lerp (transform.localPosition, Vector3.zero, fracJourney);
				}
				if (isLerpingBackInBounds == false) {
					Rebound (); //this is a switch to only call the lerp once
				}
			}

			if (transform.localPosition.y >= 0 && transform.localPosition.y <= lowerBound) {
				isLerpingBackInBounds = false;
			}

			//moves the menu one for one with finger drag
			if (isDragging) {
				RectTransformUtility.ScreenPointToLocalPointInRectangle (myCanvas.transform as RectTransform, Input.mousePosition, myCanvas.GetComponent<Canvas> ().worldCamera, out pos);
				currentY = pos.y;
				transform.localPosition = new Vector3 (0f, -(initY - currentY), 0f);
			}
		
	
		}
//		scrollbar.value = transform.localPosition.y / GUIManager.s_instance.upperBound; TODO
		
	}

}
