using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class Timer1 : MonoBehaviour {
	public static Timer1 s_instance;

  public float timeLeft;
  public bool pause;
  public bool timesUp;
  public float normTime;
  public Color[] colorsToLerp = new Color[2];
  public bool useMat;


  private float totalTime;
  private float amtOfSeconds;

  private Material timerMat;

  private Text thisText;

	void Awake () {
		s_instance = this;
    if(useMat){
      timerMat = transform.parent.GetComponent<Image>().material;
    }
    thisText = GetComponent<Text>();
	}

	void Update () {
    if(pause || timesUp){    
    }else{
      timeLeft = totalTime-Time.time;
      normTime = Mathf.Abs((float)(timeLeft/amtOfSeconds)-1);
      if(useMat){
        timerMat.SetFloat("_Angle", Mathf.Lerp(-3.14f, 3.14f, normTime));
        timerMat.SetColor("_Color", Color.Lerp(colorsToLerp[1], colorsToLerp[0], (float)(timeLeft/amtOfSeconds)));
      }
      thisText.text = ((int)(timeLeft+1)).ToString();
      if(timeLeft < 0f){
        timesUp = true;
      } 
    }
	}

	public void Reset(float amtOfTime){
    totalTime = Time.time + amtOfTime;
    amtOfSeconds = amtOfTime;
    pause = false;
    timesUp = false;
	}

  public void Pause(){
    pause = true;
  }
  public void Unpause(){
    pause = false;
  }
}
