using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Quitbutton : MonoBehaviour {
	//Set in inspector
	public Slider mastery;

	public void SaveAndQuit () {
		int masteryOutput = Mathf.CeilToInt(mastery.value*100);
		StartCoroutine(AppManager.s_instance.uploadAssignMastery(AppManager.s_instance.currentAssignments[AppManager.s_instance.currIndex].fullAssignTitle, masteryOutput));
		StartCoroutine(AppManager.s_instance.uploadAssignTime(AppManager.s_instance.currentAssignments[AppManager.s_instance.currIndex].fullAssignTitle, (int)(AppManager.s_instance.currentAssignments[AppManager.s_instance.currIndex].timeAtLoad)));
		//		int masteryOutput = Mathf.CeilToInt (mastery.value * 100);
		//		AppManager.s_instance.saveAssignmentMastery (AppManager.s_instance.currentAssignments [AppManager.s_instance.currIndex], masteryOutput);
		Application.LoadLevel ("AssignmentMenu");
}
}
