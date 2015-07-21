using UnityEngine;
using System.Collections;

public enum SlideDirection {Up, Down, Left, Right}
public class AnimationSlide : MonoBehaviour {

	//this is for menu GUI items to look extra cool when they pop up
	public float startTime, fadeTime = 1;

	public float moveDistance = 10000f;
	public SlideDirection thisSlideDirection = SlideDirection.Up;
	Vector3 startPos, endPos, moveDirection;
	bool isSliding = false;

	void Start () {
		startPos = transform.localPosition;
		endPos = transform.localPosition + moveDirection * moveDistance;
		switch (thisSlideDirection) {
		case SlideDirection.Down :
			moveDirection = new Vector3(0,-1,0);
			break;
			
		case SlideDirection.Up :
			moveDirection = new Vector3(0,1,0);
			break;
			
		case SlideDirection.Left :
			moveDirection = new Vector3(1,0,0);
			break;
			
		case SlideDirection.Right :
			moveDirection = new Vector3(-1,0,0);
			break;
		}

	}

	public void Reset() {
		isSliding = false;
		transform.localPosition = startPos;
	}

	public void Slide() {
		isSliding = true;
		startTime = Time.time;
	}
	
	void Update() {
		if (isSliding) {
			float timePassed = (Time.time - startTime);
			float fracJourney = timePassed / fadeTime;
			//increment timer once per frame

			transform.localPosition = Vector3.Lerp (startPos, endPos, fracJourney);
			if (fracJourney >= 1) {
				isSliding = false;
			}
		} 
	}
}
