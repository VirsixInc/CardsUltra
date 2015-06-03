using UnityEngine;
using System.Collections;

public class PictureSpawner : MonoBehaviour {

	[Range(0,1)]
	public float fractionOfJourney;
	public float speed;
	public Transform startTransform, endTransform;
	float startTime;
	float journeyDistance, distCovered;

	// Use this for initialization
	void Start () {
		journeyDistance = Vector3.Distance (startTransform.position, endTransform.position);
		Reset ();
	}

	void Reset () {
		if (fractionOfJourney != 0) {
			distCovered = fractionOfJourney * journeyDistance;
		} else {
			gameObject.transform.position = new Vector3(startTransform.position.x, transform.position.y, 0f);
			distCovered = 0;
		}
	}

	// Update is called once per frame
	void Update () {
		distCovered += Time.deltaTime * speed;
		fractionOfJourney = distCovered / journeyDistance;
		gameObject.transform.position = Vector3.Lerp (new Vector3(startTransform.position.x, transform.position.y, 0f),
		                                              new Vector3(endTransform.position.x, transform.position.y, 0f),
		                                              fractionOfJourney);
		if (fractionOfJourney > .97f) {
			fractionOfJourney = 0;
			Reset ();
		}
	}
}
