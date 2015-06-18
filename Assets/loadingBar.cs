using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class loadingBar : MonoBehaviour {

  public Image uiImage;
  public Color[] colors;
	// Use this for initialization
	void Start () {
    uiImage = GetComponent<Image>();
	}
	
	// Update is called once per frame
	void Update () {
    uiImage.color = Color.Lerp(colors[0], colors[1], Time.deltaTime*5f);//Random.Range(100,255),Random.Range(0,100),Random.Range(150,200),255);
	}
}
