using UnityEngine;
using System;

public class Assignment : IComparable<Assignment> {
	public bool isCompleted = false;
	
	public int mastery = 0;
	public int orderVal;
  public string surveyLink = "";
	public string assignmentTitle = "";
	public string fullAssignTitle = "";
	public string displayTitle = "";
  public string fileName = "";
	public string type = "";
	
	public float timeAtLoad;
	
	public string sceneToLoad;
	public bool hasImages;
	
	public GameObject associatedGUIObject;
	public string[] content;
	public string imgDir;

	//uses the CompareTo interface
	public int CompareTo(Assignment compareAssignment)
	{
		// A null value means that this object is greater. 
		if (compareAssignment == null)
			return 1;
		
		else 
			return this.orderVal.CompareTo(compareAssignment.orderVal);
	}

	public Assignment(string assignTitle, string templateType, string newFileName = "NA",bool usesImg = false, int order = -1, string newSurveyLink = "NA", string displayName = "NA"){
		hasImages = usesImg; 
    orderVal = order;
    fileName = newFileName;
    surveyLink = newSurveyLink;
		type = templateType;
		assignmentTitle = assignTitle;
    if(displayName == "NA"){
      displayTitle = UppercaseFirst(assignmentTitle.Split('.')[0]).Replace("_", " ");
    }else{
      displayTitle = displayName;
    }
		fullAssignTitle = type + "_" + assignmentTitle.Split('.')[0];
	}
	static string UppercaseFirst(string s){
		if (string.IsNullOrEmpty(s)){
			return string.Empty;
		}
		return char.ToUpper(s[0]) + s.Substring(1);
	}
}
