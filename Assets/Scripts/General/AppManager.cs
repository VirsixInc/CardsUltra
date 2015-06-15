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

  public bool localDebug, pabloDebug;
	private AppState currentAppState;
	public static AppManager s_instance;
  public List<Assignment> currentAssignments = new List<Assignment>();
	public List<GameObject> userAssignments;
  public int currIndex;

	string[] assignmentURLs;
	string serverURL = "http://96.126.100.208:8000/client", folderName,
         username,
         password,
         masteryFilePath,
         filePathToUse;


  int assignsLoaded = 0, assignmentsDownloaded = 0, totalAssigns;

	List<string> assignmentURLsToDownload;

  bool urlsDownloaded, clicked, userExists;

	void Awake() {
    if(localDebug){
			if(pabloDebug){
				serverURL = "http://192.168.1.16:8080/client";

			}else{
        serverURL = "http://localhost:8080/client";
			}
      username = "AGutierrez";
      password = "Password1357";
      userExists = true;
    }
    s_instance = this;
    masteryFilePath = Application.persistentDataPath + "/mastery.info";
    DontDestroyOnLoad(transform.gameObject);
	}

	void Update () {
		switch (currentAppState) {
      case AppState.Login :
        if(userExists){
          currentAppState = AppState.Initialize;
          Application.LoadLevel("AssignmentMenu");
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
        if(assignsLoaded == totalAssigns){
          currentAppState = AppState.LoadContent;
        }
        
        break;
      case AppState.LoadContent:
        loadInLocalAssignments();
        currentAppState = AppState.MenuConfig;
        break;
      case AppState.MenuConfig:
        GUIManager.s_instance.LoadAllAssignments(currentAssignments);
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
          newMgr.SendMessage("configureGame", currentAssignments[currIndex]);
        }
        currentAppState = AppState.Playing;
        break;
      case AppState.Playing:
        if(Application.loadedLevelName == "AssignmentMenu"){
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
    }else if(www.text == "false"){
      userExists = false;
    }else{
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
    for(int i = 0; i<totalAssigns;i++){
      string thisAssign = (string)(allAssignments[i].GetField("assignmentName").ToString());
      string hasImages = (string)(allAssignments[i].GetField("hasImages").ToString());
      string directoryPath = Application.persistentDataPath + "/images/";
      string imgDirPath = directoryPath + thisAssign.Replace("\"", "") + "-images";
      if(!Directory.Exists(imgDirPath)){
        if(!Directory.Exists(directoryPath)){
          Directory.CreateDirectory(directoryPath);
        }
        Directory.CreateDirectory(imgDirPath);
        StartCoroutine(pullImgs(thisAssign));
      }
      string filePath = (Application.persistentDataPath + "/" + thisAssign).Replace("\"", "");
      if(!File.Exists(filePath + ".data")){
        StartCoroutine(saveAssignment(thisAssign));
      }else{
        assignsLoaded++;
      }
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

    }
    File.WriteAllText(masteryFilePath, String.Empty);
    File.WriteAllLines(masteryFilePath, masteryFile);
  }

  public IEnumerator uploadAssignMastery(string assignmentName, int mastery){
    assignmentName = assignmentName.Replace("\"", "").ToLower();
		WWW www = new WWW(serverURL + "/setAssignmentMastery?assignmentName=" + assignmentName + "&student=" + username + "&mastery=" + mastery.ToString());
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
