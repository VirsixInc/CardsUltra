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
  public string category;
  public indiCard thisIndiCard;
  public Bucket(GameObject objRef, Text objTxtRef, string categoryToUse){
    objAssoc = objRef;
    objText = objTxtRef;
    category = categoryToUse;
    objText.text = category;

  }
};

public class dispTerm{
  public GameObject disp;
  public Text txtDisp;
  public string currArg;
  public string reqArg;

  public Vector3 start;
  public Vector3 end;
  public float current;

  public dispTerm(GameObject dispToSet, Text txtToSet){
    disp = dispToSet;
    txtDisp = txtToSet;
    start = disp.transform.localPosition;
    end = start;
    end.x = end.x*-1f;
    current = 0f;
  }
  
  public void setDisp(bktTerm termToUse){
    currArg = termToUse.answer;
    reqArg = termToUse.category;
    txtDisp.text = currArg;
  }

}

public class bktTerm{
  public string answer;
  public string category;
  public Sprite imgAssoc;
  public int mastery = 0;
  public bool mastered = false;
  public string imgPath;
  public bool imageLoaded;
  public bktTerm(string newAnswer, string newCategory, string imgPathToUse = null){
    if(imgPathToUse != null){
      imgPath = imgPathToUse;
    }
    answer = newAnswer;
    category = newCategory;

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
  public GameObject bucketFab, questionPanel;
  public Transform bucketHolder;

  private List<Bucket> allBuckets = new List<Bucket>();
  private List<bktTerm> allTerms = new List<bktTerm>();
  private List<bktTerm> unmasteredTerms = new List<bktTerm>();

  private string direct;

  private bool useImages, handleBucketPress, firstPress, handleKeyboardSubmit, firstSubmit;


  private float timeBetweenCorrAnswers;
  private float timeAtEnd;

  private int currIndex;
  private int amtOfCards;
  private int correctTermIndex;
  private int currMastery = 0;
  private int requiredMastery = 4;
  private int currentPhase;
  private int currentImageIt;

  public string[] contentForAssign;
  public string baseImagePath;
	public GameObject winningSlide;
	public GameObject background;
	
  public Slider masteryMeter;
  public Slider loadSlider;
  public float loadDelay = 0.5f;
  public float timeSinceLoad;

  bool readyToConfigure;

  private dispTerm disp;

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
        readyToConfigure = true;
        if(readyToConfigure){
          currentState = GameState.ConfigGame;
        }
        break;
      case GameState.ConfigGame:
        currentState = GameState.ConfigCards;
        break;
      case GameState.ConfigCards:
        Text dispTxt = GameObject.Find("questText").gameObject.GetComponent<Text>();
        disp = new dispTerm(questionPanel, dispTxt);
        disp.txtDisp.text = "here";

        allTerms = convertCSV(parseContent(contentForAssign));
        generateBuckets(pullCategories(allTerms));
        unmasteredTerms = allTerms.ToList();

        currentState = GameState.ResetCards;
        break;
      case GameState.ResetCards:
        masteryMeter.value = getMastery();
        Timer1.s_instance.Reset(15f);
        correctTermIndex = Random.Range(0,unmasteredTerms.Count);
        disp.setDisp(unmasteredTerms[correctTermIndex]);
        firstPress = true;
        currentState = GameState.PlayingCards;
        break;
      case GameState.PlayingCards:
        if(handleBucketPress){
          if(allBuckets[currIndex].category == disp.reqArg){
            if(firstPress){
              if(SoundManager.s_instance!=null){
                SoundManager.s_instance.PlaySound(SoundManager.s_instance.m_correct);
              }
              background.SendMessage("correct");
              unmasteredTerms[correctTermIndex].mastery++;
              currentState = GameState.ResetCards;
              if(unmasteredTerms[correctTermIndex].mastery == requiredMastery*.75f){
                unmasteredTerms.RemoveAt(correctTermIndex);
              }
            }else{
              background.SendMessage("correct");
              unmasteredTerms[correctTermIndex].mastery--;
              currentState = GameState.ResetCards;
            }
          }else{
            background.SendMessage("incorrect");
            if(SoundManager.s_instance!=null)SoundManager.s_instance.PlaySound(SoundManager.s_instance.m_wrong);
            Timer1.s_instance.Pause();
            firstPress = false;
            handleBucketPress = false;
          }
          masteryMeter.value = getMastery();
          if(getMastery() >= 1f){
            currentState = GameState.End;
          }
        }
        disp.disp.transform.localPosition = Vector3.Lerp(
            disp.start,
            disp.end,
            Timer1.s_instance.normTime
            );
        if(Timer1.s_instance.timesUp && !Timer1.s_instance.pause){
          Timer1.s_instance.Pause();
          unmasteredTerms[correctTermIndex].mastery -=2;
        }
        handleBucketPress = false;
        break;
      case GameState.End:
        break;
    }
  }

  public void bucketHandler (int cardIndex) {
    handleBucketPress = true;
    currIndex = cardIndex;
  }

  public void switchState(int newState){
    currentState = (GameState)newState;
  }

	float getMastery(){
		float floatToReturn;
		float amtOfMasteredTerms = allTerms.Count-unmasteredTerms.Count;
		float currentMastery = amtOfMasteredTerms*requiredMastery; 
		foreach(bktTerm currTerm in unmasteredTerms){
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

	List<string[]> parseContent(string[] lines){
    if(lines != null){
      List<string[]> listToReturn = new List<string[]>();
      if(lines.Length > 0){
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
      }

      for(int i = 0; i < listToReturn.Count; i++){
        for(int j = 0; j < listToReturn[i].Length; j++){
          string temp = listToReturn[i][j].Replace('|',',');
          listToReturn[i][j] = temp;
        }
      }

      return listToReturn;
    }else{
      return null;
    }
	}

  List<bktTerm> convertCSV(List<string[]> inputString){
    List<bktTerm> listToReturn = new List<bktTerm>();
    foreach(string[] thisLine in inputString){
      if(thisLine.Length > 1){
        bktTerm termToAdd;
        termToAdd = new bktTerm(thisLine[0], thisLine[1]);
        termToAdd.mastery = ((int)Mathf.Ceil(((float)(currMastery/100f))*requiredMastery));
        listToReturn.Add(termToAdd);
      }
    }
    return listToReturn;
  }

  void generateBuckets(string[] categories){
    int maxBuckets = 6;
    float screenChunk = 150f+(Screen.width/maxBuckets);
    for(int i = 0; i<categories.Length;i++){
      GameObject currentBucket = Instantiate(bucketFab) as GameObject;
      currentBucket.transform.SetParent(bucketHolder);
      currentBucket.transform.localScale = Vector3.one;
      RectTransform currTrans = currentBucket.GetComponent<RectTransform>();
      currentBucket.GetComponent<BoxCollider2D>().size = new Vector2(currTrans.rect.width,currTrans.rect.height);
      float xPos = ((-1*categories.Length)*screenChunk)/2 + i*screenChunk + screenChunk/2;
      currentBucket.transform.localPosition = new Vector3(xPos,0f,0f);
      Bucket currBucket = new Bucket(currentBucket, currentBucket.transform.GetChild(0).GetComponent<Text>(),categories[i]);
      currentBucket.SendMessage("configBucket", i);
      allBuckets.Add(currBucket);
    }
  }

  string[] pullCategories(List<bktTerm> currList){
    List<string> bktCategories = new List<string>();
    foreach(bktTerm x in currList){
      if(!bktCategories.Contains(x.category)){
        bktCategories.Add(x.category);
      }
    }
    string[] arrToReturn = bktCategories.ToArray();
    return arrToReturn;
  }
}
