using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net;
using UnityEngine.UI;
using ICSharpCode.SharpZipLib.Zip;


public enum AppState
{
	Login,
	Initialize,
	GetURLs,
	DownloadAssignments,
	MenuConfig,
	AssignmentMenu,
	LoadFromAssign,
	Uploading,
	PlayConfig,
	Playing,
	LoadContent}
;

public class AppManager : MonoBehaviour
{
	
	public bool userDebug;
	public AppState currentAppState;
	public static AppManager s_instance;
	public List<Assignment> currentAssignments = new List<Assignment> ();
	public List<GameObject> userAssignments;
	public GameObject loginButton;
	public int currIndex;
	private float timeAtAssignLoad;
	public string[] supportedTemplates;
	string[] assignmentURLs;
	string serverURL = "http://45.33.37.122:9990/client", folderName,
	username,
	password,
	masteryFilePath,
	loginFilePath,
	filePathToUse;
	
	int assignsLoaded = 0, totalAssigns, imagesRequired, imagesLoaded;
	
	List<string> assignmentURLsToDownload;
	
	bool urlsDownloaded, clicked, userExists, uploadingDone;
	
	void Awake (){
		DontDestroyOnLoad (transform.gameObject);
		if (s_instance == null) {
			s_instance = this;
		} else {
			Destroy (gameObject);
      return;
		}
		GUIManager.s_instance = transform.GetChild(0).GetComponent<GUIManager>();
		if (userDebug) {
			username = "AGutierrez";
			password = "Password1357";
			userExists = true;
		}
		masteryFilePath = Application.persistentDataPath + "/mastery.info";
		loginFilePath = Application.persistentDataPath + "/studentLogin.info";
		if (File.Exists (loginFilePath)) {
			string[] loginData = File.ReadAllLines (loginFilePath);
			loginData = loginData [0].Split (',');
			GUIManager.s_instance.SetErrorText ("User Data Found! Logging in...");
			userExists = true;
			username = loginData [0];
			password = loginData [1];
			loginButton.SendMessage ("updateFields", loginData); //sets text info on input fields
		}
	}
	
	void Update (){
		switch (currentAppState) {
		case AppState.Login:
			if (userExists) {
				currentAssignments.Add (new Assignment ("hotspots_periodic", "hotspots", "NA", false, 9999, "NA", "Periodic Table"));
				currentAppState = AppState.Initialize;
			}
			break;
		case AppState.Initialize:
			if (CheckForInternetConnection ()) {
				StartCoroutine (DownloadListOfURLs ());
				currentAppState = AppState.GetURLs;
			} else {
				currentAppState = AppState.MenuConfig;
			}
			break;
		case AppState.GetURLs:
			//only triggers once DownloadListOfURLs finishes
			if (urlsDownloaded) {
				currentAppState = AppState.DownloadAssignments;
			}
			break;
		case AppState.DownloadAssignments:
			if (assignsLoaded == totalAssigns && imagesLoaded == imagesRequired) {
				currentAppState = AppState.LoadContent;
			}
			
			//lets user know how many images and or assignments are still being loaded
			if (imagesLoaded != imagesRequired) {
				GUIManager.s_instance.SetErrorText (("Loading Images: " + imagesLoaded.ToString () + "/" + imagesRequired.ToString ()));
			} else {
				GUIManager.s_instance.SetErrorText (("Loading Assignments: " + assignsLoaded.ToString () + "/" + totalAssigns.ToString ()));
			}
			break;
		case AppState.LoadContent:
			loadInLocalAssignments ();
			currentAppState = AppState.MenuConfig;
			break;
		case AppState.LoadFromAssign:
			saveAssignmentMastery(currentAssignments[currIndex]);
			currentAppState = AppState.Uploading;
			break;
    case AppState.Uploading:
      if(uploadingDone){
        currentAppState = AppState.MenuConfig;
      }
      break;
		case AppState.MenuConfig:
			List<int> indexesToRemove = new List<int> ();
			for (int i = 0; i<currentAssignments.Count; i++) {
				if (!(supportedTemplates.Contains (currentAssignments [i].type))) {
					indexesToRemove.Add (i);
				}
			}
			for (int i = indexesToRemove.Count-1; i>-1; i--) {
				currentAssignments.RemoveAt (indexesToRemove [i]);
			}
			GUIManager.s_instance.LoadAllAssignments (currentAssignments);
			GUIManager.s_instance.SlideFromLoginToMain ();
			currentAppState = AppState.AssignmentMenu;
			break;
		case AppState.AssignmentMenu:
			if (clicked) {
        timeAtAssignLoad = Time.time;
				currentAssignments [currIndex].timeAtLoad = Time.time;
				clicked = false;
				currentAppState = AppState.PlayConfig;
				Application.LoadLevel (currentAssignments [currIndex].type);
			}
			break;
		case AppState.PlayConfig:
      timeAtAssignLoad = Time.time - timeAtAssignLoad;
			GameObject newMgr = GameObject.Find ("GameManager");
			
			if (currentAssignments [currIndex].type != "hotspots") {
				newMgr.SendMessage ("configureGame", currIndex);//currentAssignments[currIndex]);
			}
			currentAppState = AppState.Playing;
			break;
		case AppState.Playing:
			if (Application.loadedLevelName == "Login") {
				currentAppState = AppState.LoadFromAssign;
			}
			break;
		}
	}
	
	public IEnumerator loginAcct (string name, string wrd)
	{
		WWW www = new WWW (serverURL + "/logStudentIn?username=" + name + "&password=" + wrd);
		yield return www;
		if (www.text == "true") {
			userExists = true;
			username = name;
			password = wrd;
			File.WriteAllText (loginFilePath, (name + "," + wrd));
		} else if (www.text == "false") {
			GUIManager.s_instance.SetErrorText ("User Data Not Found");
			userExists = false;
		} else {
			GUIManager.s_instance.SetErrorText ("Check Internet Connection");
			
		}
	}
	
	
	public int countStringOccurrences (string text, string pattern)
	{
		int count = 0;
		int i = 0;
		while ((i = text.IndexOf(pattern, i)) != -1) {
			i += pattern.Length;
			count++;
		}
		return count;
	}
	
	IEnumerator DownloadListOfURLs ()
	{
		WWW www = new WWW (serverURL + "/pullData?username=" + username + "&password=" + password);
		urlsDownloaded = false;
		yield return www;
		JSONObject allAssignments = ParseToJSON (www.text);
		totalAssigns = allAssignments.Count;
		//delete old data files which are very small, keeps up to date
		string[] filesToDelete = Directory.GetFiles ((Application.persistentDataPath + "/"), "*.data");
		foreach (string file in filesToDelete) {
			File.Delete (file);
		}
		string directoryPath = Application.persistentDataPath + "/images/";
		if (!Directory.Exists (directoryPath)) {
			Directory.CreateDirectory (directoryPath);
		}
		imagesLoaded = 0;
		imagesRequired = 0;
		for (int i = 0; i<totalAssigns; i++) {
			//getting string values from  JSON obj 
			string thisAssign = (string)(allAssignments [i].GetField ("assignmentName").ToString ());
      int thisAssignOrder = -1;
      if(allAssignments[i].GetField("order") != null){
        thisAssignOrder = int.Parse(allAssignments[i].GetField("order").ToString().Replace("\"", ""));
      }
      string thisAssignSurvey = "NA";
      if(allAssignments[i].GetField("survey") != null){
        thisAssignSurvey = allAssignments[i].GetField("survey").ToString().Replace("\"", "");
      }
      string thisAssignName = "NA";
      if(allAssignments[i].GetField("dispName") != null){
        thisAssignName = allAssignments[i].GetField("dispName").ToString().Replace("\"", "");
      }
			//			string hasImages = (string)(allAssignments [i].GetField ("hasImages").ToString ());
			string imgDirPath = directoryPath + thisAssign.Replace ("\"", "") + "-images";
			if (imgDirPath.Contains ("cards") || imgDirPath.Contains ("multiples")) {
				if (!Directory.Exists (imgDirPath)) {
					Directory.CreateDirectory (imgDirPath);
					imagesRequired++;
					StartCoroutine (pullImgs (thisAssign));
					
					//checksum for if there was a corrupted zip which would result in only one file existing
				} else if (Directory.GetFiles (imgDirPath).Length < 1) {
					Directory.Delete (imgDirPath, true);
					Directory.CreateDirectory (imgDirPath);
					imagesRequired++;
					StartCoroutine (pullImgs (thisAssign));
				}
			}
			//currently filePath is not used
			//			string filePath = (Application.persistentDataPath + "/" + thisAssign).Replace ("\"", "");
			StartCoroutine (saveAssignment (thisAssign, thisAssignOrder, thisAssignSurvey, thisAssignName));
		}
		urlsDownloaded = true;
	}
	
	IEnumerator pullImgs (string assignmentName)
	{
		string fileName = assignmentName + "-images.zip";
		fileName = fileName.Replace ("\"", "");
		string url = (serverURL + "/images?assignment=" + fileName);
		WWW www = new WWW (url);
		yield return www;
		string directoryPath = Application.persistentDataPath + "/images/";
		string fileToUnzip = directoryPath + (fileName);
		string pathToWrite = fileToUnzip.Substring (0, fileToUnzip.Length - 4) + "/";
		if (www.isDone) { 
			if (Directory.Exists (pathToWrite)) {
				File.WriteAllBytes (fileToUnzip, www.bytes);
				using (ZipInputStream s = new ZipInputStream(File.OpenRead(fileToUnzip))) {
					ZipEntry theEntry;
					//iterate through all of the items that are zipped up
					while ((theEntry = s.GetNextEntry()) != null) {
						string directoryName = Path.GetDirectoryName (theEntry.Name.ToLower ());
						string fileNameZip = Path.GetFileName (theEntry.Name.ToLower ());
						
						if (directoryName.Length > 0) {
							Directory.CreateDirectory (pathToWrite + directoryName);
						}
						if (fileNameZip != String.Empty) {
							string filename = pathToWrite;//.Substring(0, pathToWrite.Length - 8);
							filename += theEntry.Name.ToLower ();
							using (FileStream streamWriter = File.Create(filename)) {
								int size = 2048;
								byte[] fdata = new byte[2048];
								while (true) {
									//ZipInputStream.Read returns bytes read or -1 if end of entry is reached
									size = s.Read (fdata, 0, fdata.Length);
									if (size > 0) {
										streamWriter.Write (fdata, 0, size);
									} else {
										break;
									}
								}
							}
						}
					}
				}
			}
		}
		imagesLoaded++;
	}
	
	IEnumerator saveAssignment (string assignmentName, int order, string survey, string assignDispName)
	{
		//takes the assignment name from list of URLs and downloads the assignment content
		assignmentName = assignmentName.Replace ("\"", "");
		WWW www = new WWW (serverURL + "/pullAssignment?assign=" + assignmentName);
		yield return www;
		JSONObject thisAssignmentInfo = ParseToJSON (www.text);
		string filePath = Application.persistentDataPath + "/" + assignmentName + ".data";
		List<string> assignmentContent = new List<string> ();
		foreach (JSONObject allIndArgs in thisAssignmentInfo.list) {
			foreach (JSONObject indArg in allIndArgs.list) {
				if (indArg.Count > 0) {
					string[] argToAdd = new string[indArg.Count];
					int iterator = 0;
					//convert JSON content into
					foreach (JSONObject arg in indArg.list) {
						argToAdd [iterator] = arg.ToString ();
						iterator++;
					}
					string concatString = String.Join (",", argToAdd);//does this line add commas into the content?
					concatString = concatString.Replace ("\"", "");//what does this do
					assignmentContent.Add (concatString);
				}
			}
		}
		if(!File.Exists(masteryFilePath) || !(File.ReadAllText(masteryFilePath).Contains(assignmentName))){
			File.AppendAllText (masteryFilePath, assignmentName + ",0\n");
		}
    assignmentContent.Insert(0, order.ToString() + "," + survey + "," + assignDispName);
		File.WriteAllLines (filePath, assignmentContent.ToArray ());
		assignsLoaded++;
	}
	
	public static bool CheckForInternetConnection ()
	{
		try {
			using (var client = new WebClient())
			using (var stream = client.OpenRead("http://www.google.com")) {
				return true;
			}
		} catch {
			return false;
		}
	}
	
	void loadInLocalAssignments ()
	{
		DirectoryInfo localFolder = new DirectoryInfo (Application.persistentDataPath + "/");
		//		string[] masteryFile;
		//load in mastery data regarding each assignment
		if (File.Exists (masteryFilePath)) {
			//			masteryFile = File.ReadAllLines (masteryFilePath);
		} else {
			//creates file, opens files, writes "", closes file
			File.WriteAllText (masteryFilePath, "");
		}
		foreach (FileInfo currFile in localFolder.GetFiles()) {
			string[] path = currFile.ToString ().Split ('/');
			string assignName = path [path.Length - 1];
			string check = assignName.Split ('.') [1];
			if (check == "data") {
        string[] splitFirstLine = (File.ReadAllLines(currFile.FullName)[0]).Split(','); 
        int order = int.Parse(splitFirstLine[0]);
        string surveyLink = splitFirstLine[1];
        string displayName = splitFirstLine[2];
				Assignment currAssign = generateAssignment (assignName, order, surveyLink, displayName);
				currAssign.mastery = pullAssignMastery (currAssign);
				currentAssignments.Add (currAssign);
			}
		}
	}
	
	Assignment generateAssignment (string assignName, int order, string survey, string displayName)
	{
		Assignment assignToReturn;
		string[] assign = assignName.Split ('_');
		bool assignImages = Directory.Exists (Application.persistentDataPath + "/images/" + assignName.Split ('.') [0] + "-images");
		assignToReturn = new Assignment (assign [1], assign [0], (Application.persistentDataPath + "/" + assignName), assignImages, order, survey, displayName);
    print(assignToReturn.fileName);
		assignToReturn.imgDir = Application.persistentDataPath + "/images/" + assignName.Split ('.') [0] + "-images";
    List<string> cont = (File.ReadAllLines((Application.persistentDataPath + "/" + assignName).Replace ("\"", "")).ToList());
    cont.RemoveAt(0);
		assignToReturn.content = cont.ToArray();
		return assignToReturn;
	}
	
	public int pullAssignMastery (Assignment currAssign)
	{
		//grab locally saved mastery for an individual template that is currently being played
		int mastery = 0;
		string[] masteryFile = File.ReadAllLines (masteryFilePath);
		bool foundFile = false;
		if (masteryFile.Length > 0) {
			foreach (string currLine in masteryFile) {
				if (currLine.Contains (currAssign.fullAssignTitle)) {
					foundFile = true;
					string[] operateLine = currLine.Split (',');
					//Mastery is stored as a string value
					mastery = int.Parse (operateLine [1]);
				}
			}
		}
		if (!foundFile) {
			File.AppendAllText (masteryFilePath, currAssign.fullAssignTitle + ",0\n");
		}
		return mastery;
	}
	
	public void saveAssignmentMastery (Assignment assignToSave){
		//the mastery file contains an array of assignments by fullAssignTitle
		int mastery = assignToSave.mastery;
		string[] masteryFile = File.ReadAllLines (masteryFilePath);
		for (int i = 0; i<masteryFile.Length; i++) {
			if (masteryFile [i].Contains (assignToSave.fullAssignTitle)) {
				masteryFile [i] = assignToSave.fullAssignTitle + "," + mastery.ToString ();
				break;
			}
		}
		File.WriteAllText (masteryFilePath, String.Empty);
		File.WriteAllLines (masteryFilePath, masteryFile);
		if (CheckForInternetConnection ()) {
			StartCoroutine (uploadAssignMastery (assignToSave, mastery));
		}
	}
	
	public void saveTermMastery(Assignment assignToSave, string term, bool correct){
    if(assignToSave.fileName == "NA"){
      return;
    }
		string dataFilePath = assignToSave.fileName;
		string[] dataFile = File.ReadAllLines(dataFilePath);
		for (int i = 0; i<dataFile.Length; i++) {
			if (dataFile [i].Contains (term)) {
				string newMastLine = dataFile[i];
				if(dataFile[i].Contains("/masteryBreak")){
					string[] masteryString = dataFile[i].Split(new string[]{ "/masteryBreak" }, StringSplitOptions.None);
					string[] corrAndIncorr = masteryString[1].Split(',');
					int[] newVals = new int[corrAndIncorr.Length];
					for(int j =0;j<corrAndIncorr.Length;j++){
						string valToParse = corrAndIncorr[j];
						newVals[j] = int.Parse (valToParse);
					}
					if(correct){
						newVals[0]++;
					}else{
						newVals[1]++;
					}
					newMastLine = masteryString[0] + ",/masteryBreak"+newVals[0].ToString() + "," + newVals[1].ToString() + '\n';
				}else{
					string corrAndIncorr = "";
					if(correct){
						corrAndIncorr = ",/masteryBreak1,0\n";
					}else{
						corrAndIncorr = ",/masteryBreak0,1\n";
					}
					newMastLine = newMastLine.Replace("\n", "");
					newMastLine = newMastLine + corrAndIncorr;
				}
				dataFile[i] = newMastLine;
				break;
			}
		}
		File.WriteAllText (dataFilePath, String.Empty);
		File.WriteAllLines (dataFilePath, dataFile);
	}
	public void uploadAllTerms(Assignment assignToUpload){
    if(assignToUpload.fileName == "NA"){
      return;
    }
		string[] dataFile = File.ReadAllLines(assignToUpload.fileName);
    List<string> swpData = new List<string>();
    string dataToPush = "";

		for(int i = 0;i<dataFile.Length;i++){
			if(dataFile[i].Contains("/masteryBreak")){
				string[] masteryString = dataFile[i].Split(new string[]{ "/masteryBreak" }, StringSplitOptions.None);
				string[] corrAndIncorr = masteryString[1].Split(',');
				string[] termToPush = masteryString[0].Split(',');
				int[] valsToPush = new int[corrAndIncorr.Length];
				for(int j =0;j<corrAndIncorr.Length;j++){
					string valToParse = corrAndIncorr[j];
					valsToPush[j] = int.Parse (valToParse);
				}
        string newValToAdd = (termToPush[0] + "," + valsToPush[0].ToString() + "," + valsToPush[1].ToString()); 
        swpData.Add(newValToAdd);
			}
		}
    for(int i = 0;i<swpData.Count;i++){
      dataToPush = dataToPush + "|" + swpData[i];
    }
    /*
    print(dataToPush);
    dataToPush = String.Join("|", dataSwp2);
    */
    if(dataToPush.Length > 1){
      dataToPush = dataToPush.Remove(0,1);
      Debug.Log("UPLOADING TERM MASTERY: " + dataToPush + "     " + dataFile.Length + "    " + assignToUpload.fileName);
      StartCoroutine(uploadTermMastery(assignToUpload, dataToPush));
    }else{
      StartCoroutine(uploadAssignTime(currentAssignments[currIndex], (int)(Time.time-timeAtAssignLoad)));
    }
	}
	
	public IEnumerator uploadTermMastery(Assignment assignToUpload, string dataToPush){
		string assignmentName = assignToUpload.fullAssignTitle.Replace ("\"", "").ToLower ();
		//dataToPush = dataToPush.Replace(" ", "%20");
    
    string urlToUse = serverURL + "/setTermMastery?assignmentName=" + assignmentName + "&student=" + username + "&content=" + dataToPush;
    urlToUse = System.Uri.EscapeUriString(urlToUse);
    WWW www = new WWW (urlToUse);
		yield return www;
    StartCoroutine(uploadAssignTime(currentAssignments[currIndex], (int)timeAtAssignLoad));//(int)(Time.time-timeAtAssignLoad)));
	}
	
	public IEnumerator uploadAssignMastery (Assignment assignToUpload, int mastery)
	{
		string assignmentName = assignToUpload.fullAssignTitle.Replace ("\"", "").ToLower ();
    string urlToUse =serverURL + "/setAssignmentMastery?assignmentName=" + assignmentName + "&student=" + username + "&mastery=" + mastery.ToString ();
    urlToUse = System.Uri.EscapeUriString(urlToUse);
		WWW www = new WWW (urlToUse);
		yield return www;
    uploadAllTerms(currentAssignments[currIndex]);
	}
	
	public IEnumerator uploadAssignTime (Assignment assignToUpload, int seconds)
	{
		string assignmentName = assignToUpload.fullAssignTitle.Replace ("\"", "").ToLower ();

		assignmentName = assignmentName.Replace ("\"", "").ToLower ();
    string urlToUse =serverURL + "/setAssignmentTime?assignmentName=" + assignmentName + "&student=" + username + "&time=" + seconds.ToString ();
    urlToUse = System.Uri.EscapeUriString(urlToUse);
		WWW www = new WWW (urlToUse);
		yield return www;
    uploadingDone = true;
	}
	
	JSONObject ParseToJSON (string txt)
	{
		JSONObject newJSONObject = JSONObject.Create (txt);
		return newJSONObject;
	}
	
	public void ClickHandler (int index)
	{
		clicked = true;
		currIndex = index;
	}
	
}
