using UnityEngine;
using System.Collections;

public class Shake : MonoBehaviour {
	
		
	//This script takes a UI image and shakes it around in 2D directions to make look cool
	
	float shakeAmount = 2f;
	float shakeSpeed = .025f;
	bool isShaking;
	public float shakeTime = .2f;
	float timeAtEnd;

	void Start () {
		StartShake ();
	}

	public void StartShake()
	{
		timeAtEnd = Time.time;
		isShaking = true;
		StartCoroutine ("ShakeImage");
	}

	//I could refactor this to use SIN function
	IEnumerator ShakeImage()
	{
		while (isShaking)
		{
//			float xAmount = (float)Random.Range(-shakeAmount, shakeAmount);
//			float yAmount = (float)Random.Range(-shakeAmount, shakeAmount);
			gameObject.transform.Translate(new Vector3(shakeAmount, 0, 0), gameObject.transform);
			yield return new WaitForSeconds(shakeSpeed);
			gameObject.transform.Translate(new Vector3(-shakeAmount, -0, 0), gameObject.transform);
			yield return new WaitForSeconds(shakeSpeed);
		}
	}	

	void Update () {
		if (Time.time > shakeTime + timeAtEnd) {
			isShaking = false;
		}
	}
}
