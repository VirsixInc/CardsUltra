using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;


public class Sequence  {
	
	public string[] sequenceOfStrings;
	public float sequenceMastery = 0;
	public int initIndex; //used to keep track of sequences throughout scene switching etc... similar to an ID
	public Sprite imgAssoc;
	public bool imageLoaded = false;
	public string imgPath;
	public Sequence(){}
	public Sequence(string[] newSequenceOfStrings, string imgPathToUse = null){
		if(imgPathToUse != null){
			imgPath = imgPathToUse;
		}
		sequenceOfStrings = newSequenceOfStrings;
	}


	public void loadImage(string path){
		byte[] currImg = File.ReadAllBytes(path);
		Texture2D newImg = new Texture2D(256,256);
		newImg.LoadImage(currImg);
		imgAssoc = Sprite.Create(newImg,new Rect(0,0,newImg.width, newImg.height),new Vector2(0.5f, 0.5f));
		imageLoaded = true;
	}
}
