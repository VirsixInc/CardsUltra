using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MasteryAnimator : MonoBehaviour {
	Slider thisSlider;
	public bool playWinSound = true;
	public GameObject touchToPlay;
	// Use this for initialization
	void Start () {
		if(SoundManager.s_instance!=null&&playWinSound)SoundManager.s_instance.PlaySound (SoundManager.s_instance.m_win);

		thisSlider = GetComponent<Slider> ();
		StartCoroutine ("AnimateSlider");
	}
	
	// Update is called once per frame
	IEnumerator AnimateSlider(){
		while (thisSlider.value < 1f) {
			yield return new WaitForSeconds (.01f);
			thisSlider.value += .01f;
			if (thisSlider.value > .8f) {

			}
		}
		if (touchToPlay!=null)touchToPlay.SetActive(true);
		print ("TRUE");
	}

}
