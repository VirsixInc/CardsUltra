using UnityEngine;
using System.Collections;

public class PictureSpawner : MonoBehaviour {

	[Range(0,1)]
	public float fractionOfJourney;
	public float speed;
	public Transform startTransform, endTransform;
	float startTime;
	float journeyDistance;

	// Use this for initialization
	void Start () {
		journeyDistance = Vector3.Distance (startTransform.position, endTransform.position);
		Reset ();
	}

	void Reset () {
		startTime = Time.time;
	}

	// Update is called once per frame
	void Update () {
		float distCovered = (Time.time - startTime) * speed;
		fractionOfJourney = distCovered / journeyDistance;
		gameObject.transform.position = Vector3.Lerp (startTransform.position, endTransform.position, fractionOfJourney);
		if (fractionOfJourney > .97f) {
//			fractionOfJourney = 0;
			Reset ();
		}
	}
}
