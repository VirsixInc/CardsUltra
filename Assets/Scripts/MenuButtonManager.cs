using UnityEngine;
using System.Collections;

public class MenuButtonManager : MonoBehaviour {

	//this class handles the triggering of menu button events
	Fader[] faders;
	AnimationSlide[] animationSlides;
	void Awake(){
		faders = GetComponentsInChildren<Fader> ();
		animationSlides = GetComponentsInChildren<AnimationSlide> ();
	}
	
	public void EnableMenu() {
		ActivateMenuButtons ();
	}
	
	public void ActivateMenuButtons () {
		if (GameObject.FindGameObjectWithTag ("scrollingMenu") != null) {
			GameObject.FindGameObjectWithTag ("scrollingMenu").GetComponent<ScrollingMenu>().isInMenu = true;
		}
		if (faders != null) {
			foreach (Fader f in faders) {
				f.StartFadeIn ();
			}
		}
		if (animationSlides != null) {
			foreach (AnimationSlide a in animationSlides) {
				a.Reset ();
			}
		}
	}
	
	public void DeActivateMenuButtons () {
		GameObject.FindGameObjectWithTag("MainCamera").GetComponent<BlurLerp>().UnBlur();
		if (GameObject.FindGameObjectWithTag ("scrollingMenu") != null) {
			GameObject.FindGameObjectWithTag ("scrollingMenu").GetComponent<ScrollingMenu>().isInMenu = false;
		}
		if (faders != null) {
			foreach (Fader f in faders) {
				f.StartFadeOut (0.2f);
			}
		}
		if (animationSlides != null) {
			foreach (AnimationSlide a in animationSlides) {
				a.Slide ();
			}
		}
	}
}
