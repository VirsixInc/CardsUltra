using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class circleGraphics : MonoBehaviour {


  public Color[] colors = new Color[2];
  RawImage imgToUse;
  int colIndex;
  bool newCol;
  public float speed;
  private Color startColor;
	// Use this for initialization
	void Start () {
    imgToUse = GetComponent<RawImage>();
    startColor = imgToUse.color;
	}
	
	// Update is called once per frame
	void Update () {
    if(newCol){
      imgToUse.color = Color.Lerp(imgToUse.color, colors[colIndex], Time.deltaTime*(speed+(5*colIndex)));
      if(((Vector4)(imgToUse.color-colors[colIndex])).magnitude <0.1f){
        newCol = false;
      }
    }else{
			imgToUse.color = Color.Lerp(startColor, Color.white, Time.deltaTime*speed);

    }
	}

  public void correct(){
    if(!newCol){
      newCol = true;
      colIndex = 0;
    }
  }

  public void incorrect(){
    if(!newCol){
      newCol = true;
      colIndex = 1;
    }
  }
}
