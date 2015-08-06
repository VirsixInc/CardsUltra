using UnityEngine;
using System.Collections;

public class CardsTerm : Term {

	public CardsTerm(string newQuestion, string newAnswer, string imgPathToUse = null){
		if(imgPathToUse != null){
			imgPath = imgPathToUse;
		}
		question = newQuestion;
		answer = newAnswer;
		
	}
}
