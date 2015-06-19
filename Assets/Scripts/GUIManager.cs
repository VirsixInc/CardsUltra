using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;


public class GUIManager : MonoBehaviour {
	public List<GameObject> completedAssignments, incompleteAssignments;
	public Assignment assignment;
	public Transform parentAssignmentHolder;
	public GameObject assignmentGUIPrefab;
	int totalAssignmentsPlaced;
	float screenWidth, screenHeight, assignmentCardHeight = 500, totalHeightOfAssignmentCards;
	public float upperBound; //for clamping scroll
	public static GUIManager s_instance;
	public Canvas myCanvas;
	public ScrollingMenu thisScrollingMenu;
	public List<Sprite> listOfMenuImages;
	public Text errorText;
	public Image blackBackground;
	public GameObject topMenuButton, bottomMenuButton;

	public GameObject loginPanel, MainMenuPanel;


	//MenuButton fields
	Fader[] faders;
	AnimationSlide[] animationSlides;

	void Awake(){
		DontDestroyOnLoad(transform.gameObject);
		s_instance = this;
		faders = GetComponentsInChildren<Fader> ();
		animationSlides = GetComponentsInChildren<AnimationSlide> ();
	}

	void Start () {
		errorText = GameObject.Find ("UserMessage").GetComponent<Text>();
		myCanvas = GameObject.Find ("Canvas").GetComponent<Canvas>();
		screenWidth = myCanvas.GetComponent<RectTransform> ().rect.width;
		screenHeight = myCanvas.GetComponent <RectTransform> ().rect.height;
//		assignmentCardHeight = assignmentGUIPrefab.GetComponent<RectTransform> ().rect.width;
	}

	public void SetErrorText(string x) {
		errorText.text = x;
	}

	// Use this for initialization
	public void LoadAllAssignments(List<Assignment> arrayOfAssignments){
		//parse associatedGameObjects into either mastered or unmastered
		for (int i = 0; i < arrayOfAssignments.Count; i++) {
			arrayOfAssignments[i].associatedGUIObject = Instantiate(assignmentGUIPrefab) as GameObject;
			arrayOfAssignments[i].associatedGUIObject.GetComponent<AssignmentGUI>().assignmentIndex = i;
			arrayOfAssignments[i].associatedGUIObject.GetComponent<AssignmentGUI>().title.text = arrayOfAssignments[i].displayTitle;
			arrayOfAssignments[i].associatedGUIObject.GetComponent<Image>().sprite = GUIManager.s_instance.listOfMenuImages[i%listOfMenuImages.Count];
			if (arrayOfAssignments[i].isCompleted) {
				completedAssignments.Add(arrayOfAssignments[i].associatedGUIObject);
				arrayOfAssignments[i].associatedGUIObject.transform.SetParent(parentAssignmentHolder, false);
			}
			else {
				incompleteAssignments.Add(arrayOfAssignments[i].associatedGUIObject);
				arrayOfAssignments[i].associatedGUIObject.transform.SetParent(parentAssignmentHolder, false);
			}
		}
		float numberOfRowsOfAssignments = Mathf.Ceil((float)arrayOfAssignments.Count / 2);
		totalHeightOfAssignmentCards = numberOfRowsOfAssignments * assignmentCardHeight;
		PlaceAssignments ();
	}
	
	void PlaceAssignments() {
		//sets layout of GUI assignment objects
		Vector3 assignmentPosition;
		
		for (int i = 0; i < incompleteAssignments.Count; i++) { //FOR INCOMPLETE ASSIGNMENTS
			totalAssignmentsPlaced++;
			if (i%2 == 0){
				//adds how many completed assignments there are to the Y-value to that it all appears stacked on top one another
				assignmentPosition = new Vector3(-screenWidth*.25f, (-assignmentCardHeight * i)/2 + screenHeight/2 - assignmentCardHeight/2, 0);
			}
			else {
				assignmentPosition = new Vector3(screenWidth*.25f, (-assignmentCardHeight * (i-1))/2 + screenHeight/2 - assignmentCardHeight/2, 0); //i-1 puts it at proper height
			}
			incompleteAssignments[i].transform.localPosition = assignmentPosition;
		}
		if (totalHeightOfAssignmentCards > screenHeight) {
			thisScrollingMenu.lowerBound = totalHeightOfAssignmentCards - screenHeight;
			thisScrollingMenu.lowerBoundPosition = new Vector3 (0, thisScrollingMenu.lowerBound, 0);
		} else {
			thisScrollingMenu.lowerBound = 0;
			thisScrollingMenu.lowerBoundPosition = Vector3.zero;
		}
		
	}

	public void SlideFromLoginToMain() {
		loginPanel.GetComponent<TransitionCard> ().StartLerpToOffScreen ();
		MainMenuPanel.GetComponent<TransitionCard> ().StartLerpToOnScreen ();
		ScrollingMenu.s_instance.Initialize ();
	}

	public void SlideFromMainToLogin() {
		loginPanel.GetComponent<TransitionCard> ().StartLerpToOnScreen ();
		MainMenuPanel.GetComponent<TransitionCard> ().StartLerpToOffScreen ();
	}

	//------------------------------ MENUBUTTON MANAGER ------------------------------//


	
	public void EnableMenu() {
		ActivateMenuButtons ();
		SetBlur ();
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
		UnBlur ();
		if (GameObject.FindGameObjectWithTag ("scrollingMenu") != null) {
			GameObject.FindGameObjectWithTag ("scrollingMenu").GetComponent<ScrollingMenu>().isInMenu = false;
		}
		bottomMenuButton.SetActive (false);
		topMenuButton.SetActive (false);
//		if (faders != null) {
//			foreach (Fader f in faders) {
//				f.StartFadeOut (0.2f);
//			}
//		}
//		if (animationSlides != null) {
//			foreach (AnimationSlide a in animationSlides) {
//				print ("A SLIDE");
//				a.Slide ();
//			}
//		}
	}

	public void LoadLevel() {
		StartCoroutine ("FadeOutBeforeLoad");
	}

	IEnumerator FadeOutBeforeLoad () {
		
		yield return new WaitForSeconds (3f);
		AppManager.s_instance.ClickHandler(GameObject.FindGameObjectWithTag("scrollingMenu").GetComponent<ScrollingMenu>().currentLevelToBePlayed);
	}

	//------------------------------ BLUR & IN GAME MENU ------------------------------//

	public void SetBlur () {
		topMenuButton.SetActive (true);
		bottomMenuButton.SetActive (true);
		Camera.main.gameObject.GetComponent<Blur>().enabled = true;
		StartCoroutine ("BlurIn");
	}
	public void UnBlur() {
		StartCoroutine ("BlurOut");
		
	}
	
	IEnumerator BlurIn () {
		while (Camera.main.GetComponent<Blur> ().iterations < 12) {
			Camera.main.GetComponent<Blur> ().iterations += 1;
			yield return new WaitForSeconds(0.03f);
		}
	}
	
	IEnumerator BlurOut () {
		while (Camera.main.GetComponent<Blur> ().iterations > 0) {
			Camera.main.GetComponent<Blur> ().iterations -= 1;
			yield return new WaitForSeconds(0.03f);
		}
		Camera.main.GetComponent<Blur>().enabled = false;
		topMenuButton.SetActive (false);
		bottomMenuButton.SetActive (false);
		
	}

	//------------------------------ Blur Menu Button Functionality ------------------------------//

	public void BlurMenuButton1 () {
		switch (AppManager.s_instance.currentAppState) {
		case AppState.Playing :

			;
			break;
		case AppState.AssignmentMenu :

			;
			break;
		}
	}

	public void BlurMenuButton2 () {
		switch (AppManager.s_instance.currentAppState) {
		case AppState.Playing :
			
			;
			break;
		case AppState.AssignmentMenu :
			AppManager.s_instance.ClickHandler(thisScrollingMenu.currentLevelToBePlayed);
			;
			break;
		}
		DeActivateMenuButtons ();
	}

	public void SetBlurMenuButtons() {
		switch (AppManager.s_instance.currentAppState) {
		case AppState.Playing :
			
			;
			break;
		case AppState.AssignmentMenu :
			 
			;
			break;
		}
	}


}
