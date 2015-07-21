using UnityEngine;
using System.Collections;

public class bucket : MonoBehaviour {

  public int index;
  public GameObject manager;
	// Use this for initialization
	void Start () {
    manager = GameObject.Find("GameManager");
	}
	
	// Update is called once per frame
	void Update () {
	
	}

  public void configBucket(int currDex){
    index = currDex;
  }
  
  void OnMouseDown(){
    manager.SendMessage("bucketHandler", index);
  }

}
