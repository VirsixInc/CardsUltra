using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Net;

public class cardManager : BRTemplate
{

	public enum GameState
	{
		Idle,
		ConfigGame,
		ConfigCards,
		ImageLoad,
		PlayingCards,
		ResetCards,
		ConfigKeyboard,
		ResetKeyboard,
		PlayingKeyboard,
		End}
	;

	public GameState currentState; //public for debug purposes 
	public GameObject circGraphic;
	public GameObject background;

	public Text questDisplay;
	public InputField keyboardText;
	public Text keyboardDispText;
	public GameObject cardsView;
	public GameObject keyboardView;

	public List<Card> allCards = new List<Card> ();
	public List<CardsTerm> allCardsTerms = new List<CardsTerm> ();
	public List<CardsTerm> unmasteredCardsTerms = new List<CardsTerm> ();

	private bool handleCardPress, firstPress, handleKeyboardSubmit, firstSubmit;

  private float timerVal = 25f;
	private int currentDifficulty;

	private int amtOfCards;
	private int correctCardsTermIndex;
	private int currentPhase;
	private int levenThresh = 2;

	bool soundHasPlayed = false;

	private Vector3 questDispStart, questDispEnd;

	public AppManager manager;
	public GameObject loadingBar;
  
	void Update ()
	{
		switch (currentState) {
		case GameState.Idle:
			if (readyToConfigure) {
				currentState = GameState.ConfigGame;
			}
			break;
		case GameState.ConfigGame:
			currentState = GameState.ConfigCards;
			break;
		case GameState.ConfigCards:
			keyboardView.SetActive (false);
			cardsView.SetActive (true);
			currentDifficulty = 1;
			GameObject[] cardObjs = GameObject.FindGameObjectsWithTag ("card");
			cardObjs = cardObjs.OrderBy (c => c.name).ToArray ();
			questDispStart = circGraphic.transform.localPosition;
			questDispEnd = circGraphic.transform.localPosition;
			questDispEnd.y = questDispEnd.y * -1;
			foreach (GameObject card in cardObjs) {
				Card newCard = new Card (card, card.transform.Find ("cardText").GetComponent<Text> (), card.transform.Find ("Image").GetComponent<Image> ());
				newCard.thisIndiCard = card.GetComponent<indiCard> ();
				allCards.Add (newCard);
			}
			allCardsTerms = convertCSV (parseContent (contentForAssign));
			totalMastery = allCardsTerms.Count * requiredMastery;
			currentState = GameState.ImageLoad;
			break;
		case GameState.ImageLoad:
			if (loadDelay + timeSinceLoad < Time.time) {
				if (currentImageIterator < allCardsTerms.Count) {
					if (!allCardsTerms [currentImageIterator].imageLoaded) {
						allCardsTerms [currentImageIterator].loadImage (allCardsTerms [currentImageIterator].imgPath);
						timeSinceLoad = Time.time;
					} else {
						currentImageIterator++;
					}
				} else {
					unmasteredCardsTerms = allCardsTerms.ToList ();
					loadingBar.SetActive (false);
					currentState = GameState.ResetCards;
				}
			} else {
				loadSlider.value = ((float)(Mathf.InverseLerp (timeSinceLoad, timeSinceLoad + loadDelay, Time.time) * 1 + (currentImageIterator)) / (float)(allCardsTerms.Count));
			}
			break;
		case GameState.ResetCards:
			masteryMeter.value = getMastery ();
			Timer1.s_instance.Reset (timerVal);
			foreach (Card currCard in allCards) {
				currCard.objAssoc.SetActive (false);
			}
			correctCardsTermIndex = Random.Range (0, unmasteredCardsTerms.Count);
			currentDifficulty = Mathf.Clamp (currentDifficulty, unmasteredCardsTerms [correctCardsTermIndex].mastery, 3); 
			amtOfCards = (int)(4.5 * currentDifficulty);
			List<int> uniqueIndexes = generateUniqueRandomNum (amtOfCards, unmasteredCardsTerms.Count, correctCardsTermIndex);
			for (int i = 0; i<uniqueIndexes.Count; i++) {
				if (!useImages) {
					allCards [i].setCard (unmasteredCardsTerms [uniqueIndexes [i]], false);
				} else {
					allCards [i].setCard (unmasteredCardsTerms [uniqueIndexes [i]], true);
				}
			}
			questDisplay.text = unmasteredCardsTerms [correctCardsTermIndex].question;
			firstPress = true;
			currentState = GameState.PlayingCards;
			break;
		case GameState.PlayingCards:
			if (circleDrag.c_instance.tapped) {
			} else if (!circleDrag.c_instance.tapped && circleDrag.c_instance.lastCardHit != null) {
				cardHandler (int.Parse (circleDrag.c_instance.lastCardHit.gameObject.name));
				circleDrag.c_instance.reset ();
			} else {
				circGraphic.transform.localPosition = Vector3.Lerp (
              questDispStart,
              questDispEnd,
              Timer1.s_instance.normTime
				);
			}
			if (handleCardPress) {
				if (firstPress && allCards [currIndex].answer == unmasteredCardsTerms [correctCardsTermIndex].answer) {
					background.SendMessage ("correct");
					if (SoundManager.s_instance != null)
						SoundManager.s_instance.PlaySound (SoundManager.s_instance.m_correct);
					unmasteredCardsTerms [correctCardsTermIndex].mastery++;
          AppManager.s_instance.saveTermMastery(
              AppManager.s_instance.currentAssignments[AppManager.s_instance.currIndex],
              unmasteredCardsTerms[correctCardsTermIndex].answer,
              true
              );
					currentState = GameState.ResetCards;
					if (unmasteredCardsTerms [correctCardsTermIndex].mastery == requiredMastery * .75f) {
						unmasteredCardsTerms.RemoveAt (correctCardsTermIndex);
					}
				} else if (allCards [currIndex].answer == unmasteredCardsTerms [correctCardsTermIndex].answer) {
					background.SendMessage ("correct");
					if (unmasteredCardsTerms [correctCardsTermIndex].mastery > 0) {
						unmasteredCardsTerms [correctCardsTermIndex].mastery--;
					}
          AppManager.s_instance.saveTermMastery(
              AppManager.s_instance.currentAssignments[AppManager.s_instance.currIndex],
              unmasteredCardsTerms[correctCardsTermIndex].answer,
              false
              );
					currentState = GameState.ResetCards;
				} else {
					allCards [currIndex].objAssoc.SendMessage ("incorrectAnswer");
					if (SoundManager.s_instance != null)
						SoundManager.s_instance.PlaySound (SoundManager.s_instance.m_wrong);

				}
				background.SendMessage ("incorrect");
				Timer1.s_instance.Pause ();
				firstPress = false;
				handleCardPress = false;
				//masteryMeter.value = getMastery();
				if (getMastery () >= 1f) {
					currentState = GameState.ConfigKeyboard;
				}


			}
			if (Timer1.s_instance.timesUp && !Timer1.s_instance.pause) {
				Timer1.s_instance.Pause ();
				if (unmasteredCardsTerms [correctCardsTermIndex].mastery > 0) {
					unmasteredCardsTerms [correctCardsTermIndex].mastery--;
				}
			}
			break;
		case GameState.ConfigKeyboard:
			keyboardView.SetActive (true);
			cardsView.SetActive (false);
			circGraphic.transform.localPosition = questDispStart;

			unmasteredCardsTerms = allCardsTerms.ToList ();
			currentState = GameState.ResetKeyboard;
			masteryMeter.value = 0f;
			break;
		case GameState.ResetKeyboard:
			Timer1.s_instance.Reset (timerVal);
			firstSubmit = true;
			correctCardsTermIndex = Random.Range (0, unmasteredCardsTerms.Count);
			questDisplay.text = unmasteredCardsTerms [correctCardsTermIndex].question;
			keyboardDispText.text = "Enter text...";

			masteryMeter.value = getMastery ();
			currentState = GameState.PlayingKeyboard;
			break;
		case GameState.PlayingKeyboard:
			if (handleKeyboardSubmit) {
				if (levenThresh > levenDist (unmasteredCardsTerms [correctCardsTermIndex].answer.ToLower(), keyboardText.text.ToLower ()) || keyboardText.text.ToLower() == unmasteredCardsTerms[correctCardsTermIndex].answer.ToLower()) {
					if (firstSubmit) {
						unmasteredCardsTerms [correctCardsTermIndex].mastery++;
					}
					currentState = GameState.ResetKeyboard;
					if (unmasteredCardsTerms [correctCardsTermIndex].mastery == requiredMastery * .5f) {
						unmasteredCardsTerms.RemoveAt (correctCardsTermIndex);
					}
				} else if (firstSubmit) {
					if (unmasteredCardsTerms [correctCardsTermIndex].mastery > 0) {
						unmasteredCardsTerms [correctCardsTermIndex].mastery--;
					}
				}
				Timer1.s_instance.Pause ();
				firstSubmit = false;
				handleKeyboardSubmit = false;
				keyboardDispText.text = unmasteredCardsTerms [correctCardsTermIndex].answer;
				keyboardText.text = "";
				if (getMastery () >= 1f) {
					currentState = GameState.End;
					timeAtEnd = Time.time;
				}
			}
			if (Timer1.s_instance.timesUp && !Timer1.s_instance.pause) {
				Timer1.s_instance.Pause ();
				if (unmasteredCardsTerms [correctCardsTermIndex].mastery > 0) {
					unmasteredCardsTerms [correctCardsTermIndex].mastery--;
				}
			}
			break;
		case GameState.End:
			winningSlide.SetActive (true);
			GUIManager.s_instance.ActivateSurveyLink();
			if (soundHasPlayed == false) {
				if (SoundManager.s_instance != null)
					SoundManager.s_instance.PlaySound (SoundManager.s_instance.m_correct);
				soundHasPlayed = true;
			}

			if (timeAtEnd + 5f < Time.time) {
				Application.LoadLevel ("Login");
				GUIManager.s_instance.DeactivateSurveyLink();

				AppManager.s_instance.uploadAssignMastery (
              AppManager.s_instance.currentAssignments [currIndex],
              100);
				AppManager.s_instance.currentAssignments [currIndex].mastery = 100;
			}
        
			break;
		}
	}

	public void configureGame (int index)
	{
		assignIndex = index;
		Assignment assignToUse = AppManager.s_instance.currentAssignments [assignIndex];
		useImages = assignToUse.hasImages;
		if (useImages) {
			directoryForAssignment = assignToUse.imgDir;
		}
		contentForAssign = assignToUse.content;
		currMastery = AppManager.s_instance.pullAssignMastery (assignToUse);
		readyToConfigure = true;
	}

	public void cardHandler (int cardIndex)
	{
		handleCardPress = true;
		currIndex = cardIndex;

	}

	public void keyboardHandler ()
	{
		handleKeyboardSubmit = true;
	}

	public void switchState (int newState)
	{
		currentState = (GameState)newState;
	}

	public int levenDist (string s, string t)
	{
		int n = s.Length;
		int m = t.Length;
		int[,] d = new int[n + 1, m + 1];

		// Step 1
		if (n == 0) {
			return m;
		}

		if (m == 0) {
			return n;
		}
    	// Step 2
    for (int i = 0; i <= n; d[i, 0] = i++)
    {
    }

    for (int j = 0; j <= m; d[0, j] = j++)
    {
    }
		for (int i = 1; i <= n; i++) {
			for (int j = 1; j <= m; j++) {
				int cost = (t [j - 1] == s [i - 1]) ? 0 : 1;

				d [i, j] = Mathf.Min (
            Mathf.Min (d [i - 1, j] + 1, d [i, j - 1] + 1),
            d [i - 1, j - 1] + cost);
			}
		}
		return d [n, m];
	}

	bool checkForNewPhase ()
	{
		bool newPhase = false;
		int amtOfMasteredCardsTerms = allCardsTerms.Count - unmasteredCardsTerms.Count;
		int currentMastery = amtOfMasteredCardsTerms * requiredMastery; 
		foreach (CardsTerm currCardsTerm in unmasteredCardsTerms) {
			currentMastery += currCardsTerm.mastery;
		}
	
		if (currentMastery >= totalMastery / 2) {
			newPhase = true;
			print ("NEW PHASE IS TRUE!");
		}
		return newPhase;
	}

	float getMastery ()
	{
		float floatToReturn;
		float amtOfMasteredCardsTerms = allCardsTerms.Count - unmasteredCardsTerms.Count;
		float currentMastery = amtOfMasteredCardsTerms * requiredMastery; 
		foreach (CardsTerm currCardsTerm in unmasteredCardsTerms) {
			currentMastery += currCardsTerm.mastery;
		}
		floatToReturn = currentMastery / (allCardsTerms.Count * requiredMastery);
		return floatToReturn;
	}
	List<int> generateUniqueRandomNum (int amt, int randRange, int noThisNum = -1)
	{
		List<int> listToReturn = new List<int> ();
		for (int i = 0; i<amt; i++) {
			int x = Random.Range (0, randRange);
			if (!(listToReturn.Contains (x))) {
				listToReturn.Add (x);
			}
		}

		if (noThisNum != -1 && !listToReturn.Contains (noThisNum)) {
			listToReturn [Random.Range (0, listToReturn.Count)] = noThisNum;
		}
		return listToReturn;
	}

	List<string[]> parseContent (string[] contentToParse)
	{
		List<string[]> listToReturn = new List<string[]> ();
		string[] lines = contentToParse;
		for (int i = 0; i<lines.Length; i++) {
			string[] currLine = lines [i].Split (',');
			if (currLine.Length > 0) {
				for (int j = 0; j<currLine.Length; j++) {
					currLine [j] = currLine [j].Replace ('\\', ',');
					currLine [j] = currLine [j].ToLower ();
				}
				listToReturn.Add (currLine);
			}
		}

		for (int i = 0; i < listToReturn.Count; i++) {
			for (int j = 0; j < listToReturn[i].Length; j++) {
				string temp = listToReturn [i] [j].Replace ('|', ',');
				listToReturn [i] [j] = temp;
			}
		}

		return listToReturn;
	}

	List<CardsTerm> convertCSV (List<string[]> inputString)
	{
		List<CardsTerm> listToReturn = new List<CardsTerm> ();
    int masteryIterator = 0;
		foreach (string[] thisLine in inputString) {
			if (thisLine.Length > 1) {
				CardsTerm termToAdd;
				if (useImages) {
					if (thisLine [1] [0] == ' ') {
						thisLine [1] = thisLine [1].Substring (1, thisLine [1].Length - 1);
					}
					string imgPathToUse = directoryForAssignment + "/" + thisLine [1].ToLower () + ".png";
					imgPathToUse = imgPathToUse.Replace ("\"", "");
					termToAdd = new CardsTerm (thisLine [0], thisLine [1], imgPathToUse);//, newImg);
				} else {
					termToAdd = new CardsTerm (thisLine [0], thisLine [1]);
				}
        if(currMastery > 0 && masteryIterator < 2*((currMastery*inputString.Count)/(inputString.Count*requiredMastery))){
          termToAdd.mastery++;
          masteryIterator++;
        }
				listToReturn.Add (termToAdd);
			}
		}
		return listToReturn;
	}
}
