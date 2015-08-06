using UnityEngine;
using System.Collections;
using System.IO;

public class Term{
	public string answer;
	public string question;
	public Sprite imgAssoc;
	public int mastery = 0;
	public bool mastered = false;
	public string imgPath;
	public bool imageLoaded;

	public void loadImage(string path){
		byte[] currImg = File.ReadAllBytes(path);
		Texture2D newImg = new Texture2D(256,256);
		newImg.LoadImage(currImg);
		imgAssoc = Sprite.Create(newImg,new Rect(0,0,newImg.width, newImg.height),new Vector2(0.5f, 0.5f));
		imageLoaded = true;
	}
}
