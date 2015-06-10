    using UnityEngine;
using System.Collections;
using UnityEngine.UI;


//holds the visual/GUI variables for each assignment so that they can be modified according to Assignment information
//texts etc must be set in the inspector

public class AssignmentGUI : MonoBehaviour {
	public Text description, title, templateType, completetionStatus;
	public Slider mastery;
	public Image picture;
	public int assignmentIndex;
	public Canvas myCanvas;
	Vector2 sizeDelt;
	RectTransform rectTransform; 
//	void Start () {
//		sizeDelt = 
//		myCanvas = GameObject.Find ("Canvas").GetComponent<Canvas>();
//		print(myCanvas.pixelRect.width);
//		print (myCanvas.pixelRect.height);
//		gameObject.GetComponent<RectTransform> ().localPosition = new Vector3 (myCanvas.pixelRect.width / 2, myCanvas.pixelRect.height / 2, 0);
//	}

}


