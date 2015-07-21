using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class ImitateColor : MonoBehaviour {

	Color thisColor;
	void Start () {
		thisColor = GetComponent<Text> ().color;
	}
	// Update is called once per frame
	void Update () {
		thisColor = transform.parent.gameObject.GetComponent<Image> ().color;
	}
}
