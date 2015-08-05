using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class ImitateColor : MonoBehaviour {
	
	// Update is called once per frame
	void Update () {
		GetComponent<Text> ().color = transform.parent.gameObject.GetComponent<Image> ().color;
	}
}
