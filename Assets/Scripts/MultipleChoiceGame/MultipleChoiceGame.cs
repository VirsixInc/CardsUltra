using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class MultipleChoiceGame : BRTemplate {

	public enum GameState {Idle, Config, ImageLoad, Intro, SetRound, Playing, CheckAnswer, WrongAnswer, CorrectAnswer, WinScreen};
	
	public GameObject draggableGUIPrefab, GUITargetPrefab, REDX, GREENCHECKMARK, submitButton;
	public GameObject draggableHolder;
	public GameObject prompt;
	public Image CircleMaterial;
	public GameObject target;
	public List<Image> pictures;
	public Image picture;
	public Timer1 timer;
	
	GameObject draggableGUIHolder;
	List<GameObject> draggables = new List<GameObject>();
	bool isButtonPressed = false;
	List<List<string>> matrixOfCSVData;
	List<SequenceTerm> listOfSequenceTerms; //listOfSequenceTerms exists during an instance of Sequencing game. Current row index accesses the current SequenceTerm

	public List<SequenceTerm> allTerms = new List<SequenceTerm>();
	public List<SequenceTerm> unmasteredTerms = new List<SequenceTerm>();

	GameType gameType = GameType.Text;
	GameState gameState = GameState.Intro;
	bool areDistractorTerms;
	int xRandomRange, yRandomRange;
	string[] currentSequenceTerm;
	float scaleFactor, numberOfDraggablesSnapped=0;
	float startTime, exitTime = 5f;
	CSVParser thisCSVParser;
	PopUpGraphic greenCheck, redX, greenCheckmark;//todo

	void ConfigureAssignment() {
		submitButton = GameObject.Find ("SubmitButton"); //TODO GET RID OF ALL .FINDS
		scaleFactor = GameObject.Find ("Canvas").GetComponent<Canvas> ().scaleFactor;
		greenCheck = GameObject.Find ("greenCheck").GetComponent<PopUpGraphic> ();
		draggableGUIHolder = GameObject.Find ("DraggableGUIHolder");
		redX = GameObject.Find ("redX").GetComponent<PopUpGraphic> ();
		Input.multiTouchEnabled = true;
		
		//parse CSV
		useImages = AppManager.s_instance.currentAssignments[assignIndex].hasImages;
		if(useImages){
			directoryForAssignment = AppManager.s_instance.currentAssignments[assignIndex].imgDir;
		}
		
		//list init
		listOfSequenceTerms = new List<SequenceTerm> (); //use this to store per SequenceTerm mastery values
		listOfSequenceTerms = convertCSV(parseContent(AppManager.s_instance.currentAssignments[assignIndex].content));
		
		timer.Reset(15f);
		
	}

	bool userClickedStart = false;


	//STATE MACHINE
	void Update () {
		switch(gameState){
		case GameState.Intro :
			if (userClickedStart){
				gameState = GameState.Idle;
			}
			break;
		case GameState.Idle:
			#if UNITY_EDITOR
			readyToConfigure = true;
			#endif
			if(readyToConfigure){ //readyToConfigure set from AppManager by calling configGame located on each GameManager go in each template
				gameState = GameState.Config;
			}
			break;
		
		case GameState.Config :
			ConfigureAssignment();
			//check JSON to see if it is ReqIMG or not, if is set GameType to GameType.Image
			if (useImages) {
				gameState = GameState.ImageLoad;
			}
			else gameState = GameState.SetRound;
			break;

		case GameState.ImageLoad:
			if(loadDelay + timeSinceLoad < Time.time){
				if(currentImageIterator < allTerms.Count){
					if(!allTerms[currentImageIterator].imageLoaded){
						allTerms[currentImageIterator].loadImage(allTerms[currentImageIterator].imgPath);
						timeSinceLoad = Time.time;
					}else{
						currentImageIterator++;
					}
				}else{
					unmasteredTerms = allTerms.ToList();
					gameState = GameState.SetRound;
				}
			}else{
				loadSlider.value = ((float)(Mathf.InverseLerp(timeSinceLoad,timeSinceLoad+loadDelay,Time.time)*1+(currentImageIterator))/(float)(allTerms.Count));
			}
			break;
		case GameState.SetRound :
			CheckForSequenceTermMastery(); //eliminate mastered SequenceTerms
			InitiateSequenceTerm();
			gameState = GameState.Playing;
			break;
			
		case GameState.Playing :
			CheckSequenceTerm(); //checks to see how many items have been placed
			if (numberOfDraggablesSnapped == 1){ //when all items have been placed
				gameState = GameState.CheckAnswer;
			}
			break;
			
		case GameState.CheckAnswer :
			if (isButtonPressed) {
				if (Checker()){
					gameState = GameState.CorrectAnswer;
				}
				else {
					gameState = GameState.WrongAnswer;
				}
			}
			break;
			
		case GameState.CorrectAnswer :
			if (AnswerCorrect()){
				gameState = GameState.WinScreen;
			}
			else {
				gameState = GameState.SetRound;
			}
			break;
			
		case GameState.WrongAnswer :
			AnswerWrong();
			gameState = GameState.Playing;
			break;
			
		case GameState.WinScreen :
			WinRound();
			if ((Time.time - startTime) > exitTime) {
				LoadMainMenu();
			}
			break;
		}

	}

	public void configureGame(int thisInt){
		assignIndex = thisInt;
		useImages = AppManager.s_instance.currentAssignments[assignIndex].hasImages;
		if(useImages){
			directoryForAssignment = AppManager.s_instance.currentAssignments[assignIndex].imgDir;
		}
		contentForAssign = AppManager.s_instance.currentAssignments[assignIndex].content;
		currMastery = AppManager.s_instance.pullAssignMastery(AppManager.s_instance.currentAssignments[assignIndex]);
		readyToConfigure = true;
	}
	

	void OnGUI () {
		Event e = Event.current;
		if (e.type == EventType.mouseDown && gameState == GameState.Intro) {
			userClickedStart = true;
			introSlide.SetActive(false);
		}
	}


	
	void CheckSequenceTerm(){
		//checks to see how many items are currently snapped into place, keeps track of the number.
		if (draggables != null) {
			numberOfDraggablesSnapped = 0;
			foreach (GameObject x in draggables) {
				if (x.GetComponent<DraggableGUI> ().isSnapped) {
					numberOfDraggablesSnapped++; //how many items are currently snapped +1
				}
				
			}
			if (numberOfDraggablesSnapped == 1)
				submitButton.GetComponent<Image> ().color = new Color (1, 1, 1, 1); //show button 
		}
	}
	public void CheckForSequenceTermMastery() {
		if (currIndex >= listOfSequenceTerms.Count)
			currIndex = 0; //loop around to beginning of list
		while (listOfSequenceTerms[currIndex].mastery==1f && listOfSequenceTerms.Count != 0) { //skip over completed 
			listOfSequenceTerms.Remove(listOfSequenceTerms[currIndex]);
			if (listOfSequenceTerms.Count > currIndex+1) {
				currIndex++;
			}
			else 
				currIndex = 0;
		}
	}
	
	public void LoadMainMenu() {
		Application.LoadLevel("Login");
		
	}
	void WinRound() {
		winningSlide.SetActive(true);
		gameState = GameState.WinScreen; //i know that this is the wrong way to change gamestate but I have to do it until a major refactor
		startTime = Time.time;
	}
	
	public void InitiateSequenceTerm () { //displaces current SequenceTerm
		currentSequenceTerm = listOfSequenceTerms [currIndex].arrayOfStrings;
		picture.sprite = listOfSequenceTerms [currIndex].imgAssoc;
		//instantiate all of the targets and draggables in the correct positions
		for (int i = 1; i < currentSequenceTerm.Length; i++) {
			//calculate position of target based on i and sS.Count
			//generate currentSequenceTerm.Count number dragable GUI objects
			GameObject tempDraggable = (GameObject)Instantiate(draggableGUIPrefab);
			tempDraggable.transform.SetParent(draggableGUIHolder.transform);
			tempDraggable.transform.localScale *= scaleFactor;
			tempDraggable.GetComponent<RectTransform>().localPosition = new Vector3(0,0,0);
			tempDraggable.GetComponent<DraggableGUI>().SetValues(currentSequenceTerm[i], gameType);
			draggables.Add (tempDraggable);
		}
		//use mastery to determine how many answers will be filled in
		//GameObject targetHolder = GameObject.Find ("TargetGUIHolder");
		string tempPrompt = currentSequenceTerm [0];
		string replaceComma = tempPrompt.Replace ('/', ',');
		prompt.GetComponent<Text> ().text = replaceComma;
		target.GetComponent<TargetGUI> ().correctAnswer = currentSequenceTerm [1];
	}
	
	public bool Checker (){
		if (target.GetComponent<TargetGUI>().correctAnswer == target.GetComponent<TargetGUI>().occupier.GetComponent<DraggableGUI>().stringValue) {
			return true;
		}
		else {
			return false;
		}
	}
	
	void AdjustMasteryMeter(bool didAnswerCorrect) {
		if (didAnswerCorrect && !timer.timesUp) {
//			listOfSequenceTerms[currIndex].mastery += .5f;
		}
		
		else {
			if (listOfSequenceTerms[currIndex].mastery > 0) {
//				listOfSequenceTerms[currIndex].mastery -= .5f;
			}
		}
		
		float totalMastery = 0f;
		foreach (SequenceTerm x in listOfSequenceTerms) {
			totalMastery+=x.mastery;
		}
		totalMastery = totalMastery / listOfSequenceTerms.Count;
		masteryMeter.value = totalMastery;
		AppManager.s_instance.currentAssignments[assignIndex].mastery = (int)totalMastery*100;
		timer.Reset(15f);
		
	}
	
	void AnswerWrong(){
		if (SoundManager.s_instance!=null) SoundManager.s_instance.PlaySound (SoundManager.s_instance.m_wrong);
		
		redX.StartFade (); //TODO change to drag this into inspector
		AdjustMasteryMeter (false);
		timer.timesUp = true;
		ResetDraggables();
		DisableSubmitButton ();
		
	}
	
	bool AnswerCorrect() {
		if (SoundManager.s_instance!=null) SoundManager.s_instance.PlaySound (SoundManager.s_instance.m_correct);
		
		greenCheck.StartFade (); //TODO set in inspector
		foreach(GameObject go in draggables) {
			Destroy (go);
		}
		target.GetComponent<TargetGUI> ().Reset ();
		draggables.Clear();
		currIndex++;
		CheckForSequenceTermMastery ();
		AdjustMasteryMeter (true);
		DisableSubmitButton ();
		
		if (masteryMeter.value > .97f) {
			return true;
		} else { 
			return false;
		}
	}
	
	void ResetDraggables () {
		target.GetComponent<TargetGUI> ().occupier.GetComponent<DraggableGUI> ().isSnapped = false;
		target.GetComponent<TargetGUI>().occupier.transform.position = new Vector3 (Screen.width/2, Screen.height/4, 0);

	}
	void DisableSubmitButton(){
		submitButton.GetComponent<Image> ().color = new Color (1, 1, 1, .3f); //allow
		isButtonPressed = false;
	}
	public void PressSubmitButton () {
		if (submitButton.GetComponent<Image> ().color.a == 1f) {
			isButtonPressed = true;
		}
	}

	private List<string[]> parseContent(string[] contentToParse){
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

	//Put content everything into SequenceTerm classes 	
	List<SequenceTerm> convertCSV(List<string[]> inputString){
		List<SequenceTerm> listToReturn = new List<SequenceTerm>();
		foreach(string[] thisLine in inputString){
			if(thisLine.Length > 1){
				SequenceTerm termToAdd;
				if(useImages){
					if(thisLine[1][0] == ' '){
						thisLine[1] = thisLine[1].Substring(1,thisLine[1].Length-1);
					}
					string imgPathToUse =  directoryForAssignment + "/" + thisLine[1].ToLower() + ".png";
					imgPathToUse = imgPathToUse.Replace("\"", "");
					termToAdd = new SequenceTerm(thisLine, imgPathToUse);//, newImg);
				}else{
					termToAdd = new SequenceTerm(thisLine);
				}
				termToAdd.mastery = ((int)Mathf.Ceil(((float)(currMastery/100f))*requiredMastery));
				listToReturn.Add(termToAdd);
			}
		}
		return listToReturn;
	}
}