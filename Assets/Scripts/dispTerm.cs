using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class dispTerm{
	public GameObject disp;
	public Text txtDisp;
	public string currArg;
	public string reqArg;
	
	public Vector3 start;
	public Vector3 end;
	public float current;
	
	public dispTerm(GameObject dispToSet, Text txtToSet){
		disp = dispToSet;
		txtDisp = txtToSet;
		start = disp.transform.localPosition;
		end = start;
		end.x = end.x*-1f;
		current = 0f;
	}
	
	public void setDisp(bktTerm termToUse){
		currArg = termToUse.answer;
		reqArg = termToUse.category;
		txtDisp.text = currArg;
	}
	
}
