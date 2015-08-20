using UnityEngine;
using System;

public class Assignment {
	public bool isCompleted = false;
	
	public int mastery = 0;
	public int orderVal;
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
	
	public Assignment(string assignTitle, string templateType, string newFileName = "NA",bool usesImg = false){
		hasImages = usesImg; 
    fileName = newFileName;
		type = templateType;
		assignmentTitle = assignTitle;
		displayTitle = UppercaseFirst(assignmentTitle.Split('.')[0]).Replace("_", " ");
		fullAssignTitle = type + "_" + assignmentTitle.Split('.')[0];
	}
	static string UppercaseFirst(string s){
		if (string.IsNullOrEmpty(s)){
			return string.Empty;
		}
		return char.ToUpper(s[0]) + s.Substring(1);
	}
}
