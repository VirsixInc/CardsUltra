using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum GameType {Text, Image};

public class SequencingGame : BRTemplate {

	public enum GameState {Idle, Config, ImageLoad, Intro, SetRound, Playing, CheckAnswer, WrongAnswer, CorrectAnswer, WinScreen};

	/*

	When sequencing game starts, it receives a string for text_file or image_file name and a version number that directs it to the TextAsset which will be a either text or image based depending on first part of string
	check to see if our directory has that name and version number, if not, calls ContentDownloader

	*/

	public GameObject draggableGUIPrefab, GUITargetPrefab, REDX, GREENCHECKMARK, submitButton, targetHolder;
	public GameObject draggableHolder;
	public GameObject prompt;
	GameObject parentCanvas, draggableGUIHolder;
	List<GameObject> draggables = new List<GameObject>();
	List<GameObject> targets = new List<GameObject>();
	bool isButtonPressed = false;
	List<string[]> matrixOfCSVData;
	public TextAsset shortNoticeCSV;
	List<SequenceTerm> allTerms, randomizedListSequences; //allTerms exists during an instance of Sequencing game. Current row index accesses the current sequence

	GameType gameType = GameType.Text;
	GameState gameState = GameState.Config;
	bool areDistractorTerms;
	int xRandomRange, yRandomRange;
	List<string> currentSequence;
	List<float> masteryValues; //all start at 0 on first playthrough.
	float scaleFactor, numberOfDraggablesSnapped=0;
	float startTime, exitTime = 5f;
	PopUpGraphic greenCheck, redX, greenCheckmark;//todo
	public Timer1 timer; //TODO refactor to a generic timer
	public Image CircleMaterial;
	bool hasReceivedServerData = false;

	//UI Meters etc...
	[SerializeField]
	Color start;
	[SerializeField]
	Color end;

	Canvas myCanvas;
	float screenWidth;

	bool userClickedStart = false;

	void OnGUI () {
		Event e = Event.current;
		if (e.type == EventType.mouseDown && gameState == GameState.Intro) {
			userClickedStart = true;
			introSlide.SetActive(false);
		}
	}
		
	void Update () 
	{
		print (gameState);
		switch (gameState) {
		case GameState.Idle :
			if (readyToConfigure){
				gameState = GameState.Config;
			}
			break;
		case GameState.Config :
			if (hasReceivedServerData) {
				//check JSON to see if it is ReqIMG or not, if is set GameType to GameType.Image
				gameState = GameState.Intro;
			}
			break;
		
		case GameState.Intro : 
			if (userClickedStart) {
				gameState = GameState.SetRound;
			}
			break;

		case GameState.SetRound :
			CheckForSequenceMastery(); //eliminate mastered sequences
			InitiateSequence();
			gameState = GameState.Playing;
			break;
		case GameState.Playing :
			CheckSequence(); //checks to see how many items have been placed
			if (numberOfDraggablesSnapped == draggables.Count){ //when all items have been placed
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
				WinRound();
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
			if ((Time.time - startTime) > exitTime) {
				LoadMainMenu();
				GUIManager.s_instance.DeactivateSurveyLink();
			}
			break;
		}
	}


	public void configureGame (int thisInt) {
		assignIndex = thisInt;
		Assignment assignToUse = AppManager.s_instance.currentAssignments[assignIndex];
		matrixOfCSVData = parseContent(AppManager.s_instance.currentAssignments[assignIndex].content);
		useImages = AppManager.s_instance.currentAssignments[assignIndex].hasImages;
		if(useImages){
			directoryForAssignment = AppManager.s_instance.currentAssignments[assignIndex].imgDir;
		}
		myCanvas = GameObject.Find ("Canvas").GetComponent<Canvas>();
		screenWidth = myCanvas.GetComponent<RectTransform> ().rect.width;
		submitButton = GameObject.Find ("SubmitButton"); //TODO GET RID OF ALL .FINDS
		scaleFactor = GameObject.Find ("Canvas").GetComponent<Canvas> ().scaleFactor;
		greenCheck = GameObject.Find ("greenCheck").GetComponent<PopUpGraphic> ();
		parentCanvas = GameObject.FindGameObjectWithTag("shaker");
		draggableGUIHolder = GameObject.Find ("DraggableGUIHolder");
		redX = GameObject.Find ("redX").GetComponent<PopUpGraphic> ();
		Input.multiTouchEnabled = true;
		
		//list init
		allTerms = new List<SequenceTerm> (); //use this to store per sequence mastery values
		randomizedListSequences = new List<SequenceTerm> (); //can remove from this list once mastered
		
		//parsing
		
		for (int i = 0; i < matrixOfCSVData.Count; i++) { //fill out list of Sequence class instances
			SequenceTerm tempSequence = new SequenceTerm();
			tempSequence.initIndex = i;
			tempSequence.arrayOfStrings = matrixOfCSVData[i];
			allTerms.Add(tempSequence);
		}
		for (int i = 0; i < allTerms.Count; i++) {
			totalMastery+=requiredMastery;
		}

		PropagateMastery(assignToUse);
	
		List<SequenceTerm> tempListSequences = new List<SequenceTerm>(allTerms); //copy list
		
		while (tempListSequences.Count > 0) //shuffle list
		{
			int randomIndex = Mathf.FloorToInt(Random.Range(0, tempListSequences.Count));//r.Next(0, inputList.Count); //Choose a random object in the list
			randomizedListSequences.Add(tempListSequences[randomIndex]); //add it to the new, random list
			tempListSequences.RemoveAt(randomIndex); //remove to avoid duplicates
		}
		CheckForSequenceMastery();

		readyToConfigure = true;

	}

	void CheckSequence(){
		//checks to see how many items are currently snapped into place, keeps track of the number.
		if (draggables != null) {
			numberOfDraggablesSnapped = 0;
			foreach (GameObject x in draggables) {
				if (x.GetComponent<DraggableGUI> ().isSnapped) {
					numberOfDraggablesSnapped++; //how many items are currently snapped +1
				}
			}
			if (numberOfDraggablesSnapped == draggables.Count){
				submitButton.GetComponent<Image> ().color = new Color (1, 1, 1, 1); //show button 
				submitButton.transform.GetChild(0).GetComponent<Text>().color = new Color (0, 0, 0, 1f);
			}

		}
	}
	
	public void CheckForSequenceMastery() {
		if (randomizedListSequences.Count == 0) {
			print ("DELETED EVERYTHING");
			WinRound();
			return;
		}
		else {
			for (int i = 0; i < randomizedListSequences.Count; i++) {
				if (randomizedListSequences[i].mastery == requiredMastery) { //skip over completed 
					randomizedListSequences.Remove(randomizedListSequences[i]);
				}
			}
			if (randomizedListSequences.Count > currIndex+1) {
				currIndex++;
			}
			else {
				currIndex = 0;
			}
		}
	}

	public void LoadMainMenu() {
		//gets called on quit and on menu click
		StartCoroutine ("LoadMain");
		//update Mastery Values

	}
	IEnumerator LoadMain() {
		print ("LOAD MAIN");
		int masteryOutput = Mathf.CeilToInt(masteryMeter.value*100);
		AppManager.s_instance.currentAssignments[assignIndex].mastery = masteryOutput;
		yield return new WaitForSeconds (2f);
		Application.LoadLevel ("Login");
	}
	void WinRound() {
		GUIManager.s_instance.ActivateMenuButtons();
		winningSlide.SetActive(true);
		gameState = GameState.WinScreen; //i know that this is the wrong way to change gamestate but I have to do it until a major refactor
		startTime = Time.time;
	}

	public void InitiateSequence () { //displaces current sequence
		currentSequence = new List<string> (randomizedListSequences[currIndex].arrayOfStrings);
		int currentSequenceMastery = randomizedListSequences [currIndex].mastery;
	
		for (int i = 0; i < currentSequence.Count; i++) { //NOTE I HAD TO DO A SECOND LOOP FOR LAYERING ISSUES
			//calculate position of target based on i and sS.Count

			float spaceBetweenTargets = screenWidth/7;
			float totalNumberOfTargets = currentSequence.Count;
			float xPositionOfTarget =  (-totalNumberOfTargets * spaceBetweenTargets)/2 + i*spaceBetweenTargets + spaceBetweenTargets/2; //makes targets centered
			GameObject tempTarget = (GameObject)Instantiate(GUITargetPrefab);
			tempTarget.transform.SetParent(targetHolder.transform, false);
			tempTarget.transform.localPosition = new Vector3(xPositionOfTarget,tempTarget.transform.localPosition.y,0);
			tempTarget.GetComponent<TargetGUI>().correctAnswer = randomizedListSequences[currIndex].arrayOfStrings[i];
			targets.Add (tempTarget);
			
		}
		//instantiate all of the targets and draggables in the correct positions
		for (int i = 0; i < currentSequence.Count; i++) {
			//calculate position of target based on i and sS.Count
			//generate currentSequence.Count number dragable GUI objects
			GameObject tempDraggable = (GameObject)Instantiate(draggableGUIPrefab);
			tempDraggable.transform.SetParent(draggableGUIHolder.transform);
			tempDraggable.transform.localScale *= scaleFactor;
			tempDraggable.GetComponent<RectTransform>().localPosition = new Vector3(0,0,0);
			tempDraggable.GetComponent<RectTransform>().localScale = new Vector3(1,1,1);

			tempDraggable.GetComponent<DraggableGUI>().SetValues(currentSequence[i], gameType);
			draggables.Add (tempDraggable);
		}
		//use mastery to determine how many answers will be filled in
		//GameObject targetHolder = GameObject.Find ("TargetGUIHolder");

//		prompt.GetComponent<Text> ().text = replaceComma;
		int totalSpotsFilled = 0;
		for (int i = 0; i < (2 - 2*currentSequenceMastery); i++){ //currentSequenceMastery increments in .5 f
			//make sure it does not fill out all answers
			if (totalSpotsFilled >= currentSequence.Count - 1){
				break;
			}

			int randValue = Random.Range(1, currentSequence.Count)-1;
			if (!targets[randValue].GetComponent<TargetGUI>().isOccupied){
				draggables[randValue].GetComponent<DraggableGUI>().AutoFillToTarget(targets[randValue].gameObject);
				totalSpotsFilled++;
			}
		}

	}

	public bool Checker (){
		int misMatches = 0;
		for (int i = 0; i < draggables.Count; i++) {
			if (draggables[i].GetComponent<DraggableGUI>().stringValue != draggables[i].GetComponent<DraggableGUI>().currentTarget.GetComponent<TargetGUI>().correctAnswer) {
				draggables[i].GetComponent<DraggableGUI>().isMismatched = true;
				misMatches ++;
			}
			else
				draggables[i].GetComponent<DraggableGUI>().isMismatched = false;
		}

		if (misMatches!=0) {
			return false;
		}
		else {
			return true;
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
	void AdjustMasteryMeter(bool didAnswerCorrect) {
//		print ("ALL TERMS COUNT: " + allTerms.Count + "RANDLIST: " + randomizedListSequences.Count
//		       + "INIT INDEX:  " + randomizedListSequences[currIndex].initIndex + " CURRINDEX " + currIndex);
		if (didAnswerCorrect && !timer.timesUp) {

			allTerms[randomizedListSequences[currIndex].initIndex].mastery += 1;
		}

		else if (!didAnswerCorrect) {
			if (allTerms[randomizedListSequences[currIndex].initIndex].mastery > 0) {

				allTerms[randomizedListSequences[currIndex].initIndex].mastery -= 1;
			}
		}
		SetMastery();

	}

	void SetMastery() {
		currMastery = 0;
		foreach (SequenceTerm x in allTerms) {
			currMastery+=x.mastery;
		}
		print ("CURR MASTERY AT SET " + currMastery);
		AppManager.s_instance.currentAssignments[assignIndex].mastery = (int)((currMastery)/totalMastery*100);
		masteryMeter.value = (float)(currMastery)/totalMastery;
		timer.Reset(25f);
	}

	void AnswerWrong(){
		if (SoundManager.s_instance!=null) SoundManager.s_instance.PlaySound (SoundManager.s_instance.m_wrong);
		GameObject.FindGameObjectWithTag ("shaker").GetComponent<Shake>().StartShake();
		redX.StartFade (); //TODO change to drag this into inspector
		AdjustMasteryMeter (false);
		foreach(GameObject go in draggables){
			if (go.GetComponent<DraggableGUI>().isMismatched == true) { //showing what you got right and wrong with red and green GUIs
				GameObject gr = Instantiate(REDX) as GameObject;
				gr.transform.SetParent(parentCanvas.transform);//Set in inspector
				gr.transform.position = go.transform.position;
				gr.transform.localScale = new Vector3(1f,1f,1f);
			}
			else {
				GameObject gc = Instantiate(GREENCHECKMARK) as GameObject;
				gc.transform.SetParent(parentCanvas.transform);
				gc.transform.position = go.transform.position;		
				gc.transform.localScale = new Vector3(1f,1f,1f);
			}
		}
		ResetDraggables();
		timer.timesUp = true;
		DisableSubmitButton ();
		AppManager.s_instance.saveTermMastery(
			AppManager.s_instance.currentAssignments[AppManager.s_instance.currIndex],
			allTerms[currIndex].arrayOfStrings[0],
			false
			);

	}

	bool AnswerCorrect() {

		AppManager.s_instance.saveTermMastery(
			AppManager.s_instance.currentAssignments[AppManager.s_instance.currIndex],
			allTerms[currIndex].arrayOfStrings[0],
			true
			);
	

		if (SoundManager.s_instance!=null) SoundManager.s_instance.PlaySound (SoundManager.s_instance.m_correct);

		greenCheck.StartFade (); //TODO set in inspector
		foreach(GameObject go in draggables) {
			Destroy (go);
		}
		foreach (GameObject fo in targets) {
			Destroy (fo);
		}
		targets.Clear();
		draggables.Clear();
		AdjustMasteryMeter (true);
		DisableSubmitButton ();

		if (masteryMeter.value > .97f ) {
			return true;
		} else { 
			return false;
		}
	}

	void ResetDraggables () {
		for (int i = 0; i < currentSequence.Count; i++) {
			if (draggables[i].GetComponent<DraggableGUI>().isMismatched){
				draggables[i].GetComponent<DraggableGUI>().isSnapped = false;
				draggables[i].transform.localPosition = new Vector3 (0, 0, 0);
				draggables[i].GetComponent<DraggableGUI>().SetToStartColor();
			}
		}
	}
	void DisableSubmitButton(){
		submitButton.GetComponent<Image> ().color = new Color (1, 1, 1, .3f); //allow
		submitButton.transform.GetChild(0).GetComponent<Text>().color = new Color (0, 0, 0, .3f);
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
		hasReceivedServerData = true;
		return listToReturn;
	}
}
