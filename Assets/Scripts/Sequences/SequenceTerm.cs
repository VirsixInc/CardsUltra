using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class SequenceTerm : Term  {
	
	public string[] arrayOfStrings;
	public int initIndex; //used to keep track of sequences throughout scene switching etc... similar to an ID
	public SequenceTerm(){}
	public SequenceTerm(string[] newSequenceOfStrings, string imgPathToUse = null){
		if(imgPathToUse != null){
			imgPath = imgPathToUse;
		}
		arrayOfStrings = newSequenceOfStrings;
	}
	
}
