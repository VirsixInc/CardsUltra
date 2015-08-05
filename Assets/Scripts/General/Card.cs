using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Card{
	
	public GameObject objAssoc;
	public Text objText;
	public Image objImg;
	public string answer;
	public string question;
	public indiCard thisIndiCard;
	public Card(GameObject objRef, Text objTxtRef, Image objImageRef){
		objAssoc = objRef;
		objImg = objImageRef;
		objText = objTxtRef;
	}
	public void setCard(Term termToUse, bool useImage){
		if(useImage){
			objImg.sprite = termToUse.imgAssoc; 
		}
		answer = termToUse.answer;
		question = termToUse.question;
		objText.text = answer;
		objAssoc.SetActive(true);
	}
};

