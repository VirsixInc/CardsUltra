using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Bucket{
	
	public GameObject objAssoc;
	public Text objText;
	public string category;
	public indiCard thisIndiCard;
	public Bucket(GameObject objRef, Text objTxtRef, string categoryToUse){
		objAssoc = objRef;
		objText = objTxtRef;
		category = categoryToUse;
		objText.text = category;
		
	}
};
