using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class indiCard : MonoBehaviour {

  public cardManager manager;
  public bool highLighted;

  private bool incorrect = false;
  private GameObject txtObj, imgObj;
  private float speed = 0f, deltaSpeed = 0.45f, startingSpeed;
  private Outline thisOut;
  private Vector2 startingOutSize;
  private Image thisRend;

  public Color[] colors = new Color[3];
  public Sprite[] imagesToSet = new Sprite[2];
  void Start() {
    thisRend = GetComponent<Image>();
    Button but = GetComponent<Button>();
    but.onClick.AddListener(() => manager.cardHandler(int.Parse(gameObject.name))); 
    thisOut = GetComponent<Outline>();
    startingOutSize = thisOut.effectDistance;
    txtObj = transform.GetChild(1).gameObject;
    imgObj = transform.GetChild(0).gameObject;
    startingSpeed = speed;
  }
	void Update () {
    if(highLighted){
      thisOut.effectColor = colors[2];
      thisOut.effectDistance = new Vector2(5f,5f);
    }else{
      thisOut.effectColor = Color.black;
      thisOut.effectDistance = startingOutSize;
    }
    if(incorrect){
      thisOut.effectColor = colors[1]; 
      thisOut.effectDistance = startingOutSize;
      transform.eulerAngles = Vector3.Lerp(transform.eulerAngles,new Vector3(0f,180f,0f),speed*Time.deltaTime);
      speed += deltaSpeed;
      if(transform.eulerAngles.y > 60f && txtObj.activeSelf){
        txtObj.SetActive(false);
        imgObj.SetActive(false);
        thisRend.sprite = imagesToSet[1];
      }
      if(transform.eulerAngles.y > 170f){
      }
    }
	}

  void OnDisable(){
    transform.eulerAngles = new Vector3(0,0,0);
    thisOut.effectColor = Color.black;
    incorrect = false;
    txtObj.SetActive(true);
    imgObj.SetActive(true);
    thisRend.sprite = imagesToSet[0];
    speed = startingSpeed;
  }

  public void incorrectAnswer(){
    incorrect = true;
  }
}
