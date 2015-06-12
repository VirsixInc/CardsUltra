using UnityEngine;
using System.Collections;

public class LoadLevelButton : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	public void LoadLevel() {
		AppManager.s_instance.ClickHandler(GameObject.FindGameObjectWithTag("scrollingMenu").GetComponent<ScrollingMenu>().currentLevelToBePlayed);

	}

}
