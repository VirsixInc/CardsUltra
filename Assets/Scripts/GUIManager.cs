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
	float screenWidth, screenHeight, assignmentCardHeight;
	public float upperBound; //for clamping scroll
	public static GUIManager s_instance;
	public Canvas myCanvas;


	void Start () {
		myCanvas = GameObject.Find ("MainCanvas").GetComponent<Canvas>();
		screenWidth = myCanvas.GetComponent<RectTransform> ().rect.width;
		screenHeight = myCanvas.GetComponent <RectTransform> ().rect.height;
		assignmentCardHeight = assignmentGUIPrefab.GetComponent<RectTransform> ().rect.width;
	}
	
	void Awake () {
		s_instance = this;
	}
	// Use this for initialization
	public void LoadAllAssignments(List<Assignment> arrayOfAssignments){
//	public void LoadAllAssignments(){   	
		//parse associatedGameObjects into either mastered or unmastered
		for (int i = 0; i < arrayOfAssignments.Count; i++) {
			arrayOfAssignments[i].associatedGUIObject = Instantiate(assignmentGUIPrefab) as GameObject;
			
			if (arrayOfAssignments[i].isCompleted) {
				completedAssignments.Add(arrayOfAssignments[i].associatedGUIObject);
				arrayOfAssignments[i].associatedGUIObject.transform.SetParent(parentAssignmentHolder, false);
			}
			else {
				incompleteAssignments.Add(arrayOfAssignments[i].associatedGUIObject);
				arrayOfAssignments[i].associatedGUIObject.transform.SetParent(parentAssignmentHolder, false);
			}
		}
	}
	
	void PlaceAssignments() {
		//sets layout of GUI assignment objects
		Vector3 assignmentPosition;
		
		for (int i = 0; i < incompleteAssignments.Count; i++) { //FOR INCOMPLETE ASSIGNMENTS
			totalAssignmentsPlaced++;
			if (i%2 == 0){
				//adds how many completed assignments there are to the Y-value to that it all appears stacked on top one another
				assignmentPosition = new Vector3(-screenWidth*.25f, assignmentCardHeight * i, 0);
			}
			else{
				assignmentPosition = new Vector3(-screenWidth*.25f, assignmentCardHeight * (i-1), 0); //i-1 puts it at proper height
			}
			incompleteAssignments[i].transform.localPosition = assignmentPosition;
		}

		for (int i = 0; i < completedAssignments.Count; i++) {
			if ((i+totalAssignmentsPlaced)%2 == 0){
				//adds how many completed assignments there are to the Y-value to that it all appears stacked on top one another
				assignmentPosition = new Vector3(-screenWidth*.25f, assignmentCardHeight * i, 0);
			}
			else{
				assignmentPosition = new Vector3(-screenWidth*.25f, assignmentCardHeight * (i-1), 0); //i-1 puts it at proper height
			}
			incompleteAssignments[i].transform.localPosition = assignmentPosition;
		}
	
		

	}
	
}
