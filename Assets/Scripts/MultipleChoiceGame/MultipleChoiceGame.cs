using UnityEngine;

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
	List<string[]> matrixOfCSVData;
	
	public List<SequenceTerm> allTerms = new List<SequenceTerm>();
	//accumulatedMastery holds the value that the deleted terms would have since mastery is always counted by iterating through list of existing terms
	float accumulatedMastery;

	GameType gameType = GameType.Text;
	GameState gameState = GameState.Intro;
	bool areDistractorTerms;
	int xRandomRange, yRandomRange;
	string[] currentSequenceTerm;
	float scaleFactor, numberOfDraggablesSnapped=0;
	float startTime, exitTime = 5f;
	CSVParser thisCSVParser;
	PopUpGraphic greenCheck, redX, greenCheckmark;//todo
	
	public Text readytoPlay;
	
	bool userClickedStart = false;
	
	
	//STATE MACHINE
	void Update () {
		switch(gameState){
		case GameState.Intro :
			gameState = GameState.Idle;
			break;
		case GameState.Idle:
			if(readyToConfigure){ //readyToConfigure set from AppManager by calling configGame located on each GameManager go in each template
				gameState = GameState.Config;
			}
			break;
			
		case GameState.Config :
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
					gameState = GameState.SetRound;
				}
			}else{
				loadSlider.value = ((float)(Mathf.InverseLerp(timeSinceLoad,timeSinceLoad+loadDelay,Time.time)*1+(currentImageIterator))/(float)(allTerms.Count));
			}
			break;
		case GameState.SetRound :
			readytoPlay.gameObject.SetActive(true);
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
			GUIManager.s_instance.ActivateSurveyLink();
			if ((Time.time - startTime) > exitTime) {
				GUIManager.s_instance.DeactivateSurveyLink();
				LoadMainMenu();
			}
			break;
		}
		
	}
	
	public void configureGame(int thisInt){
		submitButton = GameObject.Find ("SubmitButton"); //TODO GET RID OF ALL .FINDS
		scaleFactor = GameObject.Find ("Canvas").GetComponent<Canvas> ().scaleFactor;
		greenCheck = GameObject.Find ("greenCheck").GetComponent<PopUpGraphic> ();
		draggableGUIHolder = GameObject.Find ("DraggableGUIHolder");
		redX = GameObject.Find ("redX").GetComponent<PopUpGraphic> ();
		Input.multiTouchEnabled = true;
		
		
		assignIndex = thisInt;
		Assignment assignToUse = AppManager.s_instance.currentAssignments[assignIndex];
		useImages = AppManager.s_instance.currentAssignments[assignIndex].hasImages;
		if(useImages){
			directoryForAssignment = AppManager.s_instance.currentAssignments[assignIndex].imgDir;
		}
		contentForAssign = AppManager.s_instance.currentAssignments[assignIndex].content;
		
		//parse CSV
		useImages = AppManager.s_instance.currentAssignments[assignIndex].hasImages;
		if(useImages){
			directoryForAssignment = AppManager.s_instance.currentAssignments[assignIndex].imgDir;
		}
		
		//list init
		allTerms = new List<SequenceTerm> (); //use this to store per SequenceTerm mastery values
		allTerms = convertCSV(parseContent(AppManager.s_instance.currentAssignments[assignIndex].content));
		
		//parse CSV
		useImages = AppManager.s_instance.currentAssignments[assignIndex].hasImages;
		if(useImages){
			directoryForAssignment = AppManager.s_instance.currentAssignments[assignIndex].imgDir;
		}
		for (int i = 0; i < allTerms.Count; i++) {
			totalMastery+=requiredMastery;
		}
		
		PropagateMastery(assignToUse);
		readyToConfigure = true;
	}
	
	
	void OnGUI () {
		Event e = Event.current;
		if (e.type == EventType.mouseDown && gameState == GameState.Playing) {
			userClickedStart = true;
			introSlide.SetActive(false);
			
			
		}
	}
	
	void PropagateMastery(Assignment assignToUse) {
		//Mastery Propagation
		int priorMasteryPercentage = AppManager.s_instance.pullAssignMastery(assignToUse);
		if (priorMasteryPercentage >97) {
			priorMasteryPercentage = 0;
		}
		int totalMastery = requiredMastery * allTerms.Count;
		int masteryAvailableForPropagation = Mathf.FloorToInt((float)(priorMasteryPercentage*totalMastery)/ 100f);
		for (int i = 0; i < allTerms.Count; i++) {
			if (masteryAvailableForPropagation>0){
				allTerms[i].mastery+=1;
				masteryAvailableForPropagation-=1;
				continue;
			}
			else {
				break;
			}
		}
		SetMastery();
	}
	
	void SetMastery() {
		currMastery = 0;
		foreach (SequenceTerm x in allTerms) {
			currMastery+=x.mastery;
		}
		print("total mast: " + totalMastery + "currmast: " + currMastery + "currindex: " + currIndex);
		AppManager.s_instance.currentAssignments[assignIndex].mastery = (int)((currMastery+accumulatedMastery)/totalMastery*100);

		masteryMeter.value = (float)(currMastery+accumulatedMastery)/totalMastery;
		timer.Reset(25f);
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
		if (allTerms.Count == 0) {
			WinRound();
			return;
		}
		else
			for (int i = 0; i < allTerms.Count; i++) {
				if (allTerms[i].mastery == requiredMastery) { //skip over completed 
					allTerms.Remove(allTerms[i]);
					accumulatedMastery+=requiredMastery;
				}
			}
			if (allTerms.Count > currIndex+1) {
				currIndex++;
			}
			else {
				currIndex = 0;
			}
		}

	IEnumerator LoadMain() {
		print ("LOAD MAIN");
		int masteryOutput = Mathf.CeilToInt(masteryMeter.value*100);
		AppManager.s_instance.currentAssignments[assignIndex].mastery = masteryOutput;
		yield return new WaitForSeconds (2f);
		Application.LoadLevel ("Login");
	}

	public void LoadMainMenu() {
		StartCoroutine("LoadMain");
		
	}
	void WinRound() {
		winningSlide.SetActive(true);
		startTime = Time.time;
	}
	
	public void InitiateSequenceTerm () { //displaces current SequenceTerm
		currentSequenceTerm = allTerms [currIndex].arrayOfStrings;
		picture.sprite = allTerms [currIndex].imgAssoc;
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
		//save local mastery per term
		if (didAnswerCorrect && !timer.timesUp) {
			allTerms[currIndex].mastery += 1;
		}
		
		else {
			if (allTerms[currIndex].mastery > 0) {
				allTerms[currIndex].mastery -= 1;
			}
		}
		
		SetMastery();
		
		//update server mastery as well as local per assignment mastery
		timer.Reset(25f);
		
	}
	
	void AnswerWrong(){
		AppManager.s_instance.saveTermMastery(
			AppManager.s_instance.currentAssignments[AppManager.s_instance.currIndex],
			allTerms[currIndex].arrayOfStrings[1],
			false
			);

		if (SoundManager.s_instance!=null) SoundManager.s_instance.PlaySound (SoundManager.s_instance.m_wrong);
		
		redX.StartFade (); //TODO change to drag this into inspector
		AdjustMasteryMeter (false);
		timer.timesUp = true;
		ResetDraggables();
		DisableSubmitButton ();
		
	}
	
	bool AnswerCorrect() {
		AppManager.s_instance.saveTermMastery(
			AppManager.s_instance.currentAssignments[AppManager.s_instance.currIndex],
			allTerms[currIndex].arrayOfStrings[1],
			true
			);
		if (SoundManager.s_instance!=null) SoundManager.s_instance.PlaySound (SoundManager.s_instance.m_correct);

		greenCheck.StartFade (); //TODO set in inspector
		foreach(GameObject go in draggables) {
			Destroy (go);
		}
		target.GetComponent<TargetGUI> ().Reset ();
		draggables.Clear();
		AdjustMasteryMeter (true);
		DisableSubmitButton ();
		
		if (masteryMeter.value > .97f) {
			WinRound();
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
	List<SequenceTerm> convertCSV (List<string[]> inputString){
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
