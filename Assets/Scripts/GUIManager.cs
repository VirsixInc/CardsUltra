using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;


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


	void Start () {
		myCanvas = GameObject.Find ("Canvas").GetComponent<Canvas>();
		screenWidth = myCanvas.GetComponent<RectTransform> ().rect.width;
		screenHeight = myCanvas.GetComponent <RectTransform> ().rect.height;
//		assignmentCardHeight = assignmentGUIPrefab.GetComponent<RectTransform> ().rect.width;
	}
	
	void Awake () {
		s_instance = this;
	}
	// Use this for initialization
	public void LoadAllAssignments(List<Assignment> arrayOfAssignments){
		//parse associatedGameObjects into either mastered or unmastered
		for (int i = 0; i < arrayOfAssignments.Count; i++) {
			arrayOfAssignments[i].associatedGUIObject = Instantiate(assignmentGUIPrefab) as GameObject;
			arrayOfAssignments[i].associatedGUIObject.GetComponent<AssignmentGUI>().assignmentIndex = i;
			arrayOfAssignments[i].associatedGUIObject.GetComponent<AssignmentGUI>().title.text = arrayOfAssignments[i].assignmentTitle;

			if (arrayOfAssignments[i].isCompleted) {
				completedAssignments.Add(arrayOfAssignments[i].associatedGUIObject);
				arrayOfAssignments[i].associatedGUIObject.transform.SetParent(parentAssignmentHolder, false);
			}
			else {
				incompleteAssignments.Add(arrayOfAssignments[i].associatedGUIObject);
				arrayOfAssignments[i].associatedGUIObject.transform.SetParent(parentAssignmentHolder, false);
			}
		}
		float numberOfRowsOfAssignments = Mathf.Ceil(arrayOfAssignments.Count / 2);
		totalHeightOfAssignmentCards = numberOfRowsOfAssignments * assignmentCardHeight;
		if (totalHeightOfAssignmentCards > screenHeight) {
			thisScrollingMenu.lowerBound = totalHeightOfAssignmentCards - screenHeight/2;
		} else {
			thisScrollingMenu.lowerBound = 0f;
		}
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

	
		

	}
	
}
