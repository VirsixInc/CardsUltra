using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Quitbutton : MonoBehaviour {
	//Set in inspector
	public void CallGUIManager () {
		print ("call GUI");
		GUIManager.s_instance.EnableMenu ();
	}

}
