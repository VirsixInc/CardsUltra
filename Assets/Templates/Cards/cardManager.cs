﻿using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Net;

public class Card{

  public GameObject objAssoc;
  public Text objText;
  public Image objImg;
  public string answer;
  public string question;
  public indiCard thisIndiCard;
  public Card(GameObject objRef, Text objTxtRef, Image objImageRef){
    objAssoc = objRef;
    objImg = objImageRef;
    objText = objTxtRef;
  }
  public void setCard(Term termToUse, bool useImage){
    if(useImage){
      objImg.sprite = termToUse.imgAssoc; 
    }
    answer = termToUse.answer;
    question = termToUse.question;
    objText.text = answer;
    objAssoc.SetActive(true);
  }
};

public class Term{
  public string answer;
  public string question;
  public Sprite imgAssoc;
  public int mastery = 0;
  public bool mastered = false;
  public string imgPath;
  public Term(string newQuestion, string newAnswer, Texture2D imgToUse = null){
    if(imgToUse != null){
      imgAssoc = Sprite.Create(imgToUse,new Rect(0,0,imgToUse.width, imgToUse.height),new Vector2(0.5f, 0.5f));//Resources.Load<Sprite>(filePathForImg);
    }
    question = newQuestion;
    answer = newAnswer;
  }
  /*
  public IEnumerator loadImg(){
    WWW imgToPull = new WWW(imgPath);
    yield return imgToPull;
    if(imgToPull.error == null){
      Debug.Log("IMAGE IS DONE");
      Texture2D tex = new Texture2D(256, 256, TextureFormat.RGB24, false);
      imgToPull.LoadImageIntoTexture(tex);
      //Texture2D tex = imgToPull.texture;
    }else{
      Debug.Log(imgToPull.error);
    }
  }
  */
}

public class cardManager : MonoBehaviour {

  public enum GameState{
    Idle,
    ConfigGame,
    ConfigCards,
    PlayingCards,
    ResetCards,
    ConfigKeyboard,
    ResetKeyboard,
    PlayingKeyboard,
    End};
  public GameState currentState; //public for debug purposes 
  public GameObject circGraphic;
  public GameObject background;

  public Text questDisplay;
  public InputField keyboardText;
  public Text keyboardDispText;
  public GameObject cardsView;
  public GameObject keyboardView;

  public List<Card> allCards = new List<Card>();
  public List<Term> allTerms = new List<Term>();
  public List<Term> unmasteredTerms = new List<Term>();
  
  private string direct;

  private bool useImages, handleCardPress, firstPress, handleKeyboardSubmit, firstSubmit;

  private int currentDifficulty;

  private float timeBetweenCorrAnswers;
  private float timeAtEnd;

  private int currIndex;
  private int amtOfCards;
  private int correctTermIndex;
  private int totalMastery;
  private int requiredMastery = 4;
  private int currentPhase;
  private int levenThresh = 3;

  private string[] contentForAssign;
  public string baseImagePath;
	public GameObject winningSlide;
	
  public Slider masteryMeter;

	bool soundHasPlayed = false;
  bool readyToConfigure;

  private Vector3 questDispStart, questDispEnd;

  public AppManager manager;
	
  public void configureGame(Assignment assignToUse){
    useImages = assignToUse.hasImages;
    if(useImages){
      direct = assignToUse.imgDir;
    }
    contentForAssign = assignToUse.content;
    readyToConfigure = true;
  }
	void Update () {
    switch(currentState){
      case GameState.Idle:
        if(readyToConfigure){
          currentState = GameState.ConfigGame;
        }
        break;
      case GameState.ConfigGame:
        currentState = GameState.ConfigCards;
        break;
      case GameState.ConfigCards:
        keyboardView.SetActive(false);
        cardsView.SetActive(true);
        currentDifficulty = 1;
        GameObject[] cardObjs = GameObject.FindGameObjectsWithTag("card");
        cardObjs = cardObjs.OrderBy(c=>c.name).ToArray();
        questDispStart = circGraphic.transform.localPosition;
        questDispEnd = circGraphic.transform.localPosition;
        questDispEnd.y = questDispEnd.y*-1;
        foreach(GameObject card in cardObjs){
          Card newCard = new Card(card, card.transform.Find("cardText").GetComponent<Text>(), card.transform.Find("Image").GetComponent<Image>());
          newCard.thisIndiCard = card.GetComponent<indiCard>();
          allCards.Add(newCard);
        }
        allTerms = convertCSV(parseContent(contentForAssign));
        unmasteredTerms = allTerms.ToList();

        totalMastery = unmasteredTerms.Count*requiredMastery;
//        baseImagePath = baseImagePath + manager.currentAssignments[manager.currIndex];
        currentState = GameState.ResetCards;
        break;
      case GameState.ResetCards:
        Timer1.s_instance.Reset(15f);
        foreach(Card currCard in allCards){
          currCard.objAssoc.SetActive(false);
        }
        correctTermIndex = Random.Range(0,unmasteredTerms.Count);
        currentDifficulty = Mathf.Clamp(currentDifficulty, unmasteredTerms[correctTermIndex].mastery,  3); 
        amtOfCards = (int)(3*currentDifficulty);
        List<int> uniqueIndexes = generateUniqueRandomNum(amtOfCards, unmasteredTerms.Count, correctTermIndex);
        for(int i = 0; i<uniqueIndexes.Count;i++){
          if(!useImages){
            allCards[i].setCard(unmasteredTerms[uniqueIndexes[i]], false);
          }else{
            allCards[i].setCard(unmasteredTerms[uniqueIndexes[i]], true);
          }
        }
        questDisplay.text = unmasteredTerms[correctTermIndex].question;
        firstPress = true;
        currentState = GameState.PlayingCards;
        break;
      case GameState.PlayingCards:
        if(circleDrag.c_instance.tapped){
        }else if(!circleDrag.c_instance.tapped && circleDrag.c_instance.lastCardHit != null){
          cardHandler(int.Parse(circleDrag.c_instance.lastCardHit.gameObject.name));
          circleDrag.c_instance.reset();
        }else{
          circGraphic.transform.localPosition = Vector3.Lerp(
              questDispStart,
              questDispEnd,
              Timer1.s_instance.normTime
              );
        }
        if(handleCardPress){
          if(firstPress && allCards[currIndex].answer == unmasteredTerms[correctTermIndex].answer){
            background.SendMessage("correct");
					if(SoundManager.s_instance!=null)SoundManager.s_instance.PlaySound(SoundManager.s_instance.m_correct);
            unmasteredTerms[correctTermIndex].mastery++;
            currentState = GameState.ResetCards;
            if(unmasteredTerms[correctTermIndex].mastery == requiredMastery*.75f){
              unmasteredTerms.RemoveAt(correctTermIndex);
            }
          }else if(allCards[currIndex].answer == unmasteredTerms[correctTermIndex].answer){
            background.SendMessage("correct");
            unmasteredTerms[correctTermIndex].mastery--;
            currentState = GameState.ResetCards;
          }else{
            allCards[currIndex].objAssoc.SendMessage("incorrectAnswer");
					if(SoundManager.s_instance!=null)SoundManager.s_instance.PlaySound(SoundManager.s_instance.m_wrong);

          }
          background.SendMessage("incorrect");
          Timer1.s_instance.Pause();
          firstPress = false;
          handleCardPress = false;
          masteryMeter.value = getMastery();
          if(getMastery() >= 1f){
            currentState = GameState.ConfigKeyboard;
          }


        }
        if(Timer1.s_instance.timesUp && !Timer1.s_instance.pause){
          Timer1.s_instance.Pause();
          unmasteredTerms[correctTermIndex].mastery -=2;
        }
        break;
      case GameState.ConfigKeyboard:
        keyboardView.SetActive(true);
        cardsView.SetActive(false);
        circGraphic.transform.localPosition = questDispStart;

        unmasteredTerms = allTerms.ToList();
        currentState = GameState.ResetKeyboard;
        masteryMeter.value = 0f;
        break;
      case GameState.ResetKeyboard:
        Timer1.s_instance.Reset(15f);
        firstSubmit = true;
        correctTermIndex = Random.Range(0,unmasteredTerms.Count);
        questDisplay.text = unmasteredTerms[correctTermIndex].question;
        keyboardDispText.text = "Enter text...";

        
        currentState = GameState.PlayingKeyboard;
        break;
      case GameState.PlayingKeyboard:
        if(handleKeyboardSubmit){
          if(levenThresh > levenDist(keyboardText.text.ToLower(),unmasteredTerms[correctTermIndex].answer)){
            if(firstSubmit){
              unmasteredTerms[correctTermIndex].mastery++;
            }
            currentState = GameState.ResetKeyboard;
            if(unmasteredTerms[correctTermIndex].mastery == requiredMastery*.25f){
              unmasteredTerms.RemoveAt(correctTermIndex);
            }
          }else if(firstSubmit){
            keyboardDispText.text = unmasteredTerms[correctTermIndex].answer;
            unmasteredTerms[correctTermIndex].mastery -= 2;
          }
          Timer1.s_instance.Pause();
          firstSubmit = false;
          handleKeyboardSubmit = false;
          keyboardText.text = "";
          masteryMeter.value = getMastery();
          if(getMastery() >= 1f){
            currentState = GameState.End;
            timeAtEnd = Time.time;
          }
        }
        if(Timer1.s_instance.timesUp && !Timer1.s_instance.pause){
          Timer1.s_instance.Pause();
          unmasteredTerms[correctTermIndex].mastery -=2;
        }
        break;
      case GameState.End:
        winningSlide.SetActive(true);
        if (soundHasPlayed == false) {
          if(SoundManager.s_instance!=null)SoundManager.s_instance.PlaySound(SoundManager.s_instance.m_correct);
          soundHasPlayed = true;
        }

        if(timeAtEnd + 5f < Time.time){
          Application.LoadLevel("AssignmentMenu");
          AppManager.s_instance.uploadAssignMastery(
              AppManager.s_instance.currentAssignments[currIndex].assignmentTitle,
              100);
          AppManager.s_instance.currentAssignments[currIndex].mastery = 100;
        }
        
        break;
    }
  }

  public void cardHandler (int cardIndex) {
    handleCardPress = true;
    currIndex = cardIndex;

  }

  public void keyboardHandler(){
    handleKeyboardSubmit = true;
  }

  public void switchState(int newState){
    currentState = (GameState)newState;
  }

  public int levenDist(string s, string t){
    int n = s.Length;
    int m = t.Length;
    int[,] d = new int[n + 1, m + 1];

    // Step 1
    if (n == 0){
      return m;
    }

    if (m == 0){
      return n;
    }
    for (int i = 1; i <= n; i++){
      for (int j = 1; j <= m; j++){
        int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

        d[i, j] = Mathf.Min(
            Mathf.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
            d[i - 1, j - 1] + cost);
      }
    }
    return d[n, m];
  }

  bool checkForNewPhase(){
    bool newPhase = false;
    int amtOfMasteredTerms = allTerms.Count-unmasteredTerms.Count;
    int currentMastery = amtOfMasteredTerms*requiredMastery; 
    foreach(Term currTerm in unmasteredTerms){
      currentMastery += currTerm.mastery;
    }
	
    if(currentMastery >= totalMastery/2){
      newPhase = true;
      print("NEW PHASE IS TRUE!");
    }
    return newPhase;
  }

	float getMastery(){
		float floatToReturn;
		float amtOfMasteredTerms = allTerms.Count-unmasteredTerms.Count;
		float currentMastery = amtOfMasteredTerms*requiredMastery; 
		foreach(Term currTerm in unmasteredTerms){
			currentMastery += currTerm.mastery;
		}
		floatToReturn = currentMastery / (allTerms.Count*requiredMastery);
		return floatToReturn;
	}
  List<int> generateUniqueRandomNum(int amt, int randRange, int noThisNum = -1){
    List<int> listToReturn = new List<int>();
    for(int i = 0; i<amt;i++){
      int x = Random.Range(0,randRange);
      if(!(listToReturn.Contains(x))){
        listToReturn.Add(x);
      }
    }

    if(noThisNum != -1 && !listToReturn.Contains(noThisNum)){
      listToReturn[Random.Range(0,listToReturn.Count)] = noThisNum;
    }
    return listToReturn;
  }

	List<string[]> parseContent(string[] contentToParse){
		List<string[]> listToReturn = new List<string[]>();
		string[] lines = contentToParse;
		for(int i = 0;i<lines.Length;i++){
			string[] currLine = lines[i].Split(',');
      if(currLine.Length > 0){
        for(int j = 0;j<currLine.Length;j++){
          currLine[j] = currLine[j].Replace('\\',',');
          currLine[j] = currLine[j].ToLower();
        }
        listToReturn.Add(currLine);
      }
		}

		for(int i = 0; i < listToReturn.Count; i++){
			for(int j = 0; j < listToReturn[i].Length; j++){
				string temp = listToReturn[i][j].Replace('|',',');
				listToReturn[i][j] = temp;
			}
		}

		return listToReturn;
	}

  List<Term> convertCSV(List<string[]> inputString){
    List<Term> listToReturn = new List<Term>();
    foreach(string[] thisLine in inputString){
      if(thisLine.Length > 1){
        Term termToAdd;
        if(useImages){
          if(thisLine[1][0] == ' '){
            thisLine[1] = thisLine[1].Substring(1,thisLine[1].Length-1);
          }
          string imgPathToUse =  direct + "/" + thisLine[1].ToLower() + ".png";
          imgPathToUse = imgPathToUse.Replace("\"", "");
          /*
          DirectoryInfo persistent = new DirectoryInfo(direct);
          FileInfo[] fileInfo = persistent.GetFiles("*",SearchOption.AllDirectories);
          foreach(FileInfo file in fileInfo){
            print(file);
            print(imgPathToUse);
          }
          */
          //string imgPathToUse = Path.Combine(direct.FullName, thisLine[1] + ".png");
          if(File.Exists(imgPathToUse)){
            byte[] currImg = File.ReadAllBytes(imgPathToUse);
            Texture2D newImg = new Texture2D(2,2);
            newImg.LoadImage(currImg);
            termToAdd = new Term(thisLine[0], thisLine[1], newImg);
          }else{
            termToAdd = new Term(thisLine[0], thisLine[1]);//, newImg);
          }
        }else{
          termToAdd = new Term(thisLine[0], thisLine[1]);
        }
        listToReturn.Add(termToAdd);
      }
    }
    return listToReturn;
  }
}
