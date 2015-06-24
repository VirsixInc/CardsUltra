using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Net;

public class Bucket{

  public GameObject objAssoc;
  public Text objText;
  public Image objImg;
  public string answer;
  public string question;
  public indiCard thisIndiCard;
  public Bucket(GameObject objRef, Text objTxtRef, Image objImageRef){
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

public class bktTerm{
  public string answer;
  public string question;
  public Sprite imgAssoc;
  public int mastery = 0;
  public bool mastered = false;
  public string imgPath;
  public bool imageLoaded;
  public bktTerm(string newQuestion, string newAnswer, string imgPathToUse = null){
    if(imgPathToUse != null){
      imgPath = imgPathToUse;
    }
    question = newQuestion;
    answer = newAnswer;

  }
  public void loadImage(string path){
    byte[] currImg = File.ReadAllBytes(path);
    Texture2D newImg = new Texture2D(256,256);
    newImg.LoadImage(currImg);
    imgAssoc = Sprite.Create(newImg,new Rect(0,0,newImg.width, newImg.height),new Vector2(0.5f, 0.5f));
    imageLoaded = true;
  }
}

public class bucketManager : MonoBehaviour {

  public enum GameState{
    Idle,
    ConfigGame,
    ConfigCards,
    ImageLoad,
    PlayingCards,
    ResetCards,
    End};
  public GameState currentState; //public for debug purposes 
  public GameObject bucketFab;
  public Transform bucketHolder;

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
  private int currMastery = 0;
  private int requiredMastery = 4;
  private int currentPhase;
  private int levenThresh = 3;
  private int currentImageIt;

  private string[] contentForAssign;
  public string baseImagePath;
	public GameObject winningSlide;
	
  public Slider masteryMeter;
  public Slider loadSlider;
  public float loadDelay = 0.5f;
  public float timeSinceLoad;

	bool soundHasPlayed = false;
  bool readyToConfigure;

  private Vector3 questDispStart, questDispEnd;

  public AppManager manager;
  public GameObject loadingBar;

  public void configureGame(Assignment assignToUse){
    useImages = assignToUse.hasImages;
    if(useImages){
      direct = assignToUse.imgDir;
    }
    contentForAssign = assignToUse.content;
    currMastery = AppManager.s_instance.pullAssignMastery(assignToUse);
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
        /*
        keyboardView.SetActive(false);
        cardsView.SetActive(true);
        currentDifficulty = 1;
        GameObject[] cardObjs = GameObject.FindGameObjectsWithTag("Bucket");
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

        totalMastery = allTerms.Count*requiredMastery;
        currentState = GameState.ImageLoad;
        */
        break;
      case GameState.ImageLoad:
        if(loadDelay + timeSinceLoad < Time.time){
          if(currentImageIt < allTerms.Count){
            if(!allTerms[currentImageIt].imageLoaded){
              allTerms[currentImageIt].loadImage(allTerms[currentImageIt].imgPath);
              timeSinceLoad = Time.time;
            }else{
              currentImageIt++;
            }
          }else{
            unmasteredTerms = allTerms.ToList();
            currentState = GameState.ResetCards;
          }
        }else{
          loadSlider.value = ((float)(Mathf.InverseLerp(timeSinceLoad,timeSinceLoad+loadDelay,Time.time)*1+(currentImageIt))/(float)(allTerms.Count));
        }
        break;
      case GameState.ResetCards:
        /*
        loadingBar.SetActive(false);
        masteryMeter.value = getMastery();
        Timer1.s_instance.Reset(15f);
        foreach(Card currCard in allCards){
          currCard.objAssoc.SetActive(false);
        }
        correctTermIndex = Random.Range(0,unmasteredTerms.Count);
        currentDifficulty = Mathf.Clamp(currentDifficulty, unmasteredTerms[correctTermIndex].mastery,  3); 
        amtOfCards = (int)(4.5*currentDifficulty);
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
        */
        break;
      case GameState.PlayingCards:
        /*
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
          //masteryMeter.value = getMastery();
          if(getMastery() >= 1f){
            //currentState = GameState.ConfigKeyboard;
          }


        }
        if(Timer1.s_instance.timesUp && !Timer1.s_instance.pause){
          Timer1.s_instance.Pause();
          unmasteredTerms[correctTermIndex].mastery -=2;
        }
        */
        break;
      case GameState.End:
        /*
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
        
        */
        break;
    }
  }

  public void cardHandler (int cardIndex) {
    handleCardPress = true;
    currIndex = cardIndex;

  }

  public void switchState(int newState){
    currentState = (GameState)newState;
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
          termToAdd = new Term(thisLine[0], thisLine[1], imgPathToUse);//, newImg);
        }else{
          termToAdd = new Term(thisLine[0], thisLine[1]);
        }
        termToAdd.mastery = ((int)Mathf.Ceil(((float)(currMastery/100f))*requiredMastery));
        listToReturn.Add(termToAdd);
      }
    }
    return listToReturn;
  }

  void generateBuckets(string[] categories){
    int maxBuckets = 6;
    float screenChunk = Screen.width/categories.Length;
    for(int i = 0; i<categories.Length;i++){
      GameObject currentBucket = Instantiate(bucketFab) as GameObject;
      currentBucket.transform.parent = bucketHolder;
      float xPos = ((-1*categories.Length)*screenChunk)/2 + i*screenChunk + screenChunk/2;
      print(xPos);
      //currBktPos = new Vector3(,0f,0f);
    }
  }
}
