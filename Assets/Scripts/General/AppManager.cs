﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net;
using UnityEngine.UI;
using ICSharpCode.SharpZipLib.Zip;


public enum AppState {Login, Initialize, GetURLs, DownloadAssignments, MenuConfig, AssignmentMenu, PlayConfig, Playing, LoadContent};

public class Assignment {
	public bool isCompleted = false;

	public int mastery = 0;

	public string assignmentTitle = "";
  public string fullAssignTitle = "";
  public string displayTitle = "";
  public string type = "";

  public float timeAtLoad;

  public string sceneToLoad;
  public bool hasImages;

	public GameObject associatedGUIObject;
  public string[] content;
  public string imgDir;

  public Assignment(string assignTitle, string templateType, bool usesImg = false){
    hasImages = usesImg; 
    type = templateType;
    assignmentTitle = assignTitle;
    displayTitle = UppercaseFirst(assignmentTitle.Split('.')[0]).Replace("_", " ");
    fullAssignTitle = type + "_" + assignmentTitle.Split('.')[0];
  }
  static string UppercaseFirst(string s){
    if (string.IsNullOrEmpty(s)){
        return string.Empty;
    }
    return char.ToUpper(s[0]) + s.Substring(1);
  }
}

public class AppManager : MonoBehaviour {

  public bool development, userDebug;
	public AppState currentAppState;
	public static AppManager s_instance;
  public List<Assignment> currentAssignments = new List<Assignment>();
	public List<GameObject> userAssignments;
  public GameObject loginButton;
  public int currIndex;
  public string[] supportedTemplates;

	string[] assignmentURLs;
	string serverURL = "http://96.126.100.208:9999/client", folderName,
         username,
         password,
         masteryFilePath,
         loginFilePath,
         filePathToUse;


  int assignsLoaded = 0, assignmentsDownloaded = 0, totalAssigns, imagesRequired, imagesLoaded;

	List<string> assignmentURLsToDownload;

  bool urlsDownloaded, clicked, userExists;

	void Awake() {
    if(development){
      serverURL = "http://96.126.100.208:9999/client";
    }
    if(userDebug){
      username = "AGutierrez";
      password = "Password1357";
      userExists = true;
    }
    masteryFilePath = Application.persistentDataPath + "/mastery.info";
    loginFilePath = Application.persistentDataPath + "/studentLogin.info";
    if(File.Exists(loginFilePath)){
      string[] loginData = File.ReadAllLines(loginFilePath);
      loginData = loginData[0].Split(',');
			GUIManager.s_instance.SetErrorText("User Data Found! Logging in...");
      userExists = true;
      username = loginData[0];
      password = loginData[1];
      print(username);
      loginButton.SendMessage("updateFields", loginData);
    }
    DontDestroyOnLoad(transform.gameObject);
    if(s_instance == null){
      s_instance = this;
    }else{
      Destroy(gameObject);
    }
	}
	 
	void Update () {
		switch (currentAppState) {
      case AppState.Login :
        if(userExists){
          currentAppState = AppState.Initialize;
        }
        break;
      case AppState.Initialize :
        if(CheckForInternetConnection()){
          currentAssignments.Add(new Assignment("hotspots_periodic", "hotspots"));
          StartCoroutine (DownloadListOfURLs());
          currentAppState = AppState.GetURLs;
        }else{
          currentAppState = AppState.MenuConfig;
        }
        break;
      case AppState.GetURLs :
        if(urlsDownloaded){
          currentAppState = AppState.DownloadAssignments;
        }
        break;
      case AppState.DownloadAssignments :
        if(assignsLoaded == totalAssigns && imagesLoaded == imagesRequired){
          currentAppState = AppState.LoadContent;
        }
        if(imagesLoaded != imagesRequired){
          GUIManager.s_instance.SetErrorText(("Loading Images: " + imagesLoaded.ToString() + "/" + imagesRequired.ToString()));
        }else{
          GUIManager.s_instance.SetErrorText(("Loading Assignments: " + assignsLoaded.ToString() + "/" + totalAssigns.ToString()));
        }
        break;
      case AppState.LoadContent:
        loadInLocalAssignments();
        currentAppState = AppState.MenuConfig;
        break;
      case AppState.MenuConfig:
        List<int> indexesToRemove = new List<int>();
        for(int i = 0; i<currentAssignments.Count; i++){
          if(!(supportedTemplates.Contains(currentAssignments[i].type))){
            indexesToRemove.Add(i);
          }
        }
        for(int i = indexesToRemove.Count-1;i>-1;i--){
          currentAssignments.RemoveAt(indexesToRemove[i]);
        }
        GUIManager.s_instance.LoadAllAssignments(currentAssignments);
        GUIManager.s_instance.SlideFromLoginToMain();
        currentAppState = AppState.AssignmentMenu;
        break;
      case AppState.AssignmentMenu :
        if(clicked){
          Application.LoadLevel(currentAssignments[currIndex].type);
          currentAssignments[currIndex].timeAtLoad = Time.time;
          clicked = false;
          currentAppState = AppState.PlayConfig;
        }
        break;
      case AppState.PlayConfig:
        GameObject newMgr = GameObject.Find("GameManager");
        if(currentAssignments[currIndex].type != "hotspots"){
          newMgr.SendMessage("configureGame", currIndex);//currentAssignments[currIndex]);
        }
        currentAppState = AppState.Playing;
        break;
      case AppState.Playing:
        if(Application.loadedLevelName == "Login"){
          currentAppState = AppState.MenuConfig;
        }
        break;
    }
	}

  public IEnumerator loginAcct(string name, string wrd){
    WWW www = new WWW(serverURL + "/logStudentIn?username=" + name + "&password=" + wrd);
    yield return www;
    if(www.text == "true"){
      userExists = true;
      username = name;
      password = wrd;
      File.WriteAllText(loginFilePath, (name+","+wrd));
    }else if(www.text == "false"){
			GUIManager.s_instance.SetErrorText("User Data Not Found");
      userExists = false;
    }else{
			GUIManager.s_instance.SetErrorText("Check Internet Connection");

    }
  }


  public int countStringOccurrences(string text, string pattern){
    int count = 0;
    int i = 0;
    while ((i = text.IndexOf(pattern, i)) != -1){
        i += pattern.Length;
        count++;
    }
    return count;
  }

	IEnumerator DownloadListOfURLs(){
		WWW www = new WWW(serverURL + "/pullData?username=" + username + "&password=" + password);
    urlsDownloaded = false;
		yield return www;
    JSONObject allAssignments = ParseToJSON(www.text);
    totalAssigns = allAssignments.Count;
    string[] filesToDelete = Directory.GetFiles((Application.persistentDataPath + "/"), "*.data");
    foreach(string file in filesToDelete){
      File.Delete(file);
    }
    string directoryPath = Application.persistentDataPath + "/images/";
    if(!Directory.Exists(directoryPath)){
      Directory.CreateDirectory(directoryPath);
    }
    imagesLoaded = 0;
    imagesRequired = 0;
    for(int i = 0; i<totalAssigns;i++){
      string thisAssign = (string)(allAssignments[i].GetField("assignmentName").ToString());
      string hasImages = (string)(allAssignments[i].GetField("hasImages").ToString());
      string imgDirPath = directoryPath + thisAssign.Replace("\"", "") + "-images";
      if(imgDirPath.Contains("cards") || imgDirPath.Contains("multiples")){
        if(!Directory.Exists(imgDirPath)){
          Directory.CreateDirectory(imgDirPath);
          imagesRequired++;
          StartCoroutine(pullImgs(thisAssign));
        }else if(Directory.GetFiles(imgDirPath).Length < 1){
          Directory.Delete(imgDirPath, true);
          Directory.CreateDirectory(imgDirPath);
          imagesRequired++;
          StartCoroutine(pullImgs(thisAssign));
        }
      }
      string filePath = (Application.persistentDataPath + "/" + thisAssign).Replace("\"", "");
      StartCoroutine(saveAssignment(thisAssign));
    }
    urlsDownloaded = true;
	}

  IEnumerator pullImgs(string assignmentName){
    string fileName = assignmentName + "-images.zip";
    fileName = fileName.Replace("\"", "");
    string url = (serverURL + "/images?assignment=" + fileName);
		WWW www = new WWW(url);
		yield return www;
    string directoryPath = Application.persistentDataPath + "/images/";
    string fileToUnzip = directoryPath + (fileName);
    string pathToWrite = fileToUnzip.Substring(0, fileToUnzip.Length - 4) + "/";
    if(www.isDone){
      if(Directory.Exists(pathToWrite)){
        File.WriteAllBytes(fileToUnzip, www.bytes);
        using (ZipInputStream s = new ZipInputStream(File.OpenRead(fileToUnzip))){
          ZipEntry theEntry;
          while ((theEntry = s.GetNextEntry()) != null){
            string directoryName = Path.GetDirectoryName(theEntry.Name.ToLower());
            string fileNameZip = Path.GetFileName(theEntry.Name.ToLower());

            if (directoryName.Length > 0 ){
              Directory.CreateDirectory(pathToWrite + directoryName);
            }
            if (fileNameZip != String.Empty){
              string filename = pathToWrite;//.Substring(0, pathToWrite.Length - 8);
              filename += theEntry.Name.ToLower();
              using (FileStream streamWriter = File.Create(filename)){
                int size = 2048;
                byte[] fdata = new byte[2048];
                while (true){
                  size = s.Read(fdata, 0, fdata.Length);
                  if (size > 0){
                    streamWriter.Write(fdata, 0, size);
                  }
                  else{
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

  IEnumerator saveAssignment(string assignmentName){
    assignmentName = assignmentName.Replace("\"", "");
		WWW www = new WWW(serverURL + "/pullAssignment?assign=" + assignmentName);
    yield return www;
    JSONObject thisAssignmentInfo = ParseToJSON(www.text);
    string filePath = Application.persistentDataPath + "/" + assignmentName + ".data";
    List<string> assignmentContent = new List<string>();
    foreach(JSONObject allIndArgs in thisAssignmentInfo.list){
      foreach(JSONObject indArg in allIndArgs.list){
        if(indArg.Count>0){
          string[] argToAdd = new string[indArg.Count];
          int iterator = 0;
          foreach(JSONObject arg in indArg.list){
            argToAdd[iterator] = arg.ToString();
            iterator++;
          }
          string concatString = String.Join(",",argToAdd);
          concatString = concatString.Replace("\"", "");
          assignmentContent.Add(concatString);
        }
      }
    }
    File.AppendAllText(masteryFilePath, assignmentName + ",0\n");
    File.WriteAllLines(filePath, assignmentContent.ToArray());
    assignsLoaded++;
  }

  public static bool CheckForInternetConnection(){
    try{
      using (var client = new WebClient())
      using (var stream = client.OpenRead("http://www.google.com")){
        return true;
      }
    }catch{
      return false;
    }
  }

  void loadInLocalAssignments(){
    DirectoryInfo localFolder = new DirectoryInfo(Application.persistentDataPath + "/");
    string[] masteryFile;
    if(File.Exists(masteryFilePath)){
      masteryFile = File.ReadAllLines(masteryFilePath);
    }else{
      File.WriteAllText(masteryFilePath, "");
    }
    foreach(FileInfo currFile in localFolder.GetFiles()){
      string[] path = currFile.ToString().Split('/');
      string assignName = path[path.Length-1];
      string check = assignName.Split('.')[1];
      if(check == "data"){
        Assignment currAssign = generateAssignment(assignName);
        currAssign.mastery = pullAssignMastery(currAssign);
        currentAssignments.Add(currAssign);
      }
    }
  }

  Assignment generateAssignment(string assignName){
    Assignment assignToReturn;
    string[] assign = assignName.Split('_');
    bool assignImages = Directory.Exists(Application.persistentDataPath + "/images/" + assignName.Split('.')[0] + "-images");
    assignToReturn = new Assignment(assign[1],assign[0],assignImages);
    assignToReturn.imgDir = Application.persistentDataPath + "/images/" + assignName.Split('.')[0] + "-images";
    assignToReturn.content = File.ReadAllLines((Application.persistentDataPath + "/" + assignName).Replace("\"", ""));
    return assignToReturn;
  }

  public int pullAssignMastery(Assignment currAssign){
    int mastery = 0;
    string[] masteryFile = File.ReadAllLines(masteryFilePath);
    bool foundFile = false;
    if(masteryFile.Length > 0){
      foreach(string currLine in masteryFile){
        if(currLine.Contains(currAssign.fullAssignTitle)){
          foundFile = true;
          string[] operateLine = currLine.Split(',');
          mastery = int.Parse(operateLine[1]);
        }
      }
    }
    if(!foundFile){
      File.AppendAllText(masteryFilePath, currAssign.fullAssignTitle + ",0\n");
    }
    return mastery;
  }

  public void saveAssignmentMastery(Assignment assignToSave, int mastery){
    string[] masteryFile = File.ReadAllLines(masteryFilePath);
    bool foundFile = false;

    for(int i = 0; i<masteryFile.Length; i++){
      if(masteryFile[i].Contains(assignToSave.fullAssignTitle)){
        foundFile = true;
        masteryFile[i] = assignToSave.fullAssignTitle + "," + mastery.ToString();
        break;
      }
    }
    if(CheckForInternetConnection()){
      StartCoroutine(uploadAssignMastery(assignToSave.fullAssignTitle, mastery));
    }
    File.WriteAllText(masteryFilePath, String.Empty);
    File.WriteAllLines(masteryFilePath, masteryFile);
  }

  public IEnumerator uploadAssignMastery(string assignmentName, int mastery){
    assignmentName = assignmentName.Replace("\"", "").ToLower();
		WWW www = new WWW(serverURL + "/setAssignmentMastery?assignmentName=" + assignmentName + "&student=" + username + "&mastery=" + mastery.ToString());
    print(www.url);
    yield return www;
  }

  public IEnumerator uploadAssignTime(string assignmentName, int seconds){
    seconds = (int)Time.time - seconds;
    assignmentName = assignmentName.Replace("\"", "").ToLower();
		WWW www = new WWW(serverURL + "/setAssignmentTime?assignmentName=" + assignmentName + "&student=" + username + "&time=" + seconds.ToString());
    yield return www;
  }

	JSONObject ParseToJSON (string txt) {
		JSONObject newJSONObject = JSONObject.Create (txt);
		return newJSONObject;
	}

	public void ClickHandler (int index) {
    clicked = true;
    currIndex = index;
	}

}
