using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LoginButton : MonoBehaviour {

	public InputField username, password;

	public void SendUserData () {
		if (username.text != "" && password.text != "") {
			StartCoroutine(AppManager.s_instance.loginAcct(username.text, password.text));
			//Application.LoadLevel("AssignmentMenu");
		}
	}

  public void updateFields(string[] data){
    username.text = data[0];
    password.text = data[1];
  }


}
