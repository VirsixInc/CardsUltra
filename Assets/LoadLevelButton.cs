using UnityEngine;
using System.Collections;

public class LoadLevelButton : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	public void LoadLevel(int index) {
		AppManager.s_instance.ClickHandler(index);

	}

}
