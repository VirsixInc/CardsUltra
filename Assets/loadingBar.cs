using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class loadingBar : MonoBehaviour {

  public Image uiImage;
	// Use this for initialization
	void Start () {
    uiImage = GetComponent<Image>();
	}
	
	// Update is called once per frame
	void Update () {
    uiImage.color = new Color(Random.Range(100,255),Random.Range(0,100),Random.Range(150,200),255);
	}
}
