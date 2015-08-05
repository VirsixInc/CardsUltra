using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ScrollingMenu : MonoBehaviour {

	public float lowerBound;
	float startTime, lerpTime = 2f, fracJourney;
	bool isLerp = false;
	public Vector3 lowerBoundPosition; //assigned from other class no good
	public bool isDragging = false, isSwiping = false;
	GameObject myCanvas;
	Vector2 pos;
	float currentY, initY;
	public int currentLevelToBePlayed;
	public bool isInMenu = false;
	Vector3 lerpStart;

	public static ScrollingMenu s_instance;

	void Awake () {
		s_instance = this;
	}

	void Start () {
		myCanvas = GameObject.Find ("Canvas");
	}

	public void Initialize() {
		Rebound ();
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
			if (transform.localPosition.y <= 0 || transform.localPosition.y >= lowerBound) {
				Rebound();
			}
		} 

	}
	void Rebound() {
		isLerp = true;
		startTime = Time.time;
		lerpStart = transform.localPosition;
	}

	void Update () {
		if (isInMenu == false) {

//USING LERP
			if (isLerp) {
				fracJourney = (Time.time - startTime) / lerpTime;
				if (fracJourney > .9f) {
					fracJourney = 1;
					isLerp = false;
				}


				if (transform.localPosition.y <= 0) { //when below the bottom of the list
					transform.localPosition = Vector3.Lerp (lerpStart, lowerBoundPosition, fracJourney);
				} else if (transform.localPosition.y >= lowerBound) { //when above the top of the list
					transform.localPosition = Vector3.Lerp (lerpStart, Vector3.zero, fracJourney);
				}
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
