using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;


public class Sequence  {
	
	public List<string> sequenceOfStrings;
	public float sequenceMastery = 0; //incremented by .25, used to adjust difficulty
	public int initIndex; //used to keep track of sequences throughout scene switching etc... similar to an ID
	public Sprite imgAssoc;
	bool imageLoaded = false;

	public void loadImage(string path){
		byte[] currImg = File.ReadAllBytes(path);
		Texture2D newImg = new Texture2D(256,256);
		newImg.LoadImage(currImg);
		imgAssoc = Sprite.Create(newImg,new Rect(0,0,newImg.width, newImg.height),new Vector2(0.5f, 0.5f));
		imageLoaded = true;
	}
}
