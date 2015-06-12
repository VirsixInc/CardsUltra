using UnityEngine;
using System.Collections;

public class BreatheAnimation : MonoBehaviour {

	Vector3 startScale;
	
	float amplitude = .02f;
	public float period = .3f;
	
	void Start() {
		startScale = transform.localScale;
	}
	
	void Update() {
		float theta = Time.timeSinceLevelLoad / period;
		float distance = amplitude * Mathf.Sin(theta);
		Vector3 change = startScale + startScale * distance;
		transform.localScale = change;
	}
}
