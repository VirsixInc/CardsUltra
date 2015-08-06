using UnityEngine;
using System.Collections;

public class bktTerm{
	public string answer;
	public string category;
	public Sprite imgAssoc;
	public int mastery = 0;
	public bool mastered = false;
	public string imgPath;
	public bool imageLoaded;
	public bktTerm(string newAnswer, string newCategory, string imgPathToUse = null){
		if(imgPathToUse != null){
			imgPath = imgPathToUse;
		}
		answer = newAnswer;
		category = newCategory;
		
	}
}

