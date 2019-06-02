using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.Networking;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static NetworkConnection playerTurn;
    public static bool isServer;

    public static int myId;

    public GameObject Car;
    private CarBehaviour carBehaviour;
    public bool localIsServer;
    public GameObject EggPrefab;

    public UnityEngine.UI.Text TextUsername;
    public UnityEngine.UI.Text TextPassword;
    public UnityEngine.UI.Text GameTimerText;
    public GameObject loginCanvas;

    public GameObject WinCanvas;
    public GameObject LossCanvas;

    public UserInfo userInfo;
    public bool sentSessionId;

    public bool gameStarted = false;
    private float gameTimer = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        Communication.receivedObject = new UnityObjectEvent();
        Communication.receivedObject.AddListener(Receive);

        Communication.receivedObjects = new UnityObjectsEvent();
        Communication.receivedObjects.AddListener(Receive);

        carBehaviour = Car.GetComponent<CarBehaviour>();

        playerTurn = default(NetworkConnection);

        userInfo = new UserInfo();
        userInfo.sessid = "0";
        userInfo.username = "0";

        isServer = localIsServer;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameTimer < 0.0f || gameStarted) {
            gameTimer += Time.deltaTime;
            GameTimerText.text = gameTimer.ToString("F2");
        }

        if(isServer) {
            ServerBehaviourMethod();
        }
        else {
            ClientBehaviourMethod();
        }
    }

    public void StartGame() {
        gameStarted = true;
    }

    public void Login() {
        string request = "https://studenthome.hku.nl/~daniel.bergshoeff/KGDEV4/login.php?username=" + TextUsername.text + "&password=" + TextPassword.text;
        StartCoroutine(GetRequest(request));
    }

    public void Register() {
        string request = "https://studenthome.hku.nl/~daniel.bergshoeff/KGDEV4/register.php?username=" + TextUsername.text + "&password=" + TextPassword.text;
        StartCoroutine(GetRequest(request));
    }

    public void SetScore(string sessionId, float time) {
        string setscore = "https://studenthome.hku.nl/~daniel.bergshoeff/KGDEV4/insertscore.php?sessid=" + sessionId + "&score=" + time.ToString("F2").Replace(',', '.');
        StartCoroutine(SetScore(setscore));
    }

    IEnumerator GetRequest(string url) {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url)) {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = url.Split('/');
            int page = pages.Length - 1;

            if (webRequest.isNetworkError) {
                Debug.Log(pages[page] + ": Error: " + webRequest.error);
            }
            else {
                Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                Uncode(webRequest.downloadHandler.text);
            }
        }
    }

    IEnumerator SetScore(string url) {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url)) {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = url.Split('/');
            int page = pages.Length - 1;

            if (webRequest.isNetworkError) {
                Debug.Log(pages[page] + ": Error: " + webRequest.error);
            }
            else {
                Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                UncodeSet(webRequest.downloadHandler.text);
            }
        }
    }

    private void Uncode(string json) {
        UserInfo ui = JsonUtility.FromJson<UserInfo>(json);
        if (ui != null) {
            if (ui.sessid != "0") {
                userInfo = ui;
            }
        }
    }

    private void UncodeSet(string json) {
        int i = JsonUtility.FromJson<int>(json);
    }

    private void ServerBehaviourMethod() {

    }

    private void ClientBehaviourMethod() {
        if (ClientBehaviour.Instance.m_clientToServerConnection[0] == default(NetworkConnection)) //If there's no connection, return
            return;

        if (userInfo.sessid == "0") //If the player is not logged in, return
            return;

        if(!sentSessionId) {
            ClientBehaviour.SendInfo(SendType.SessionId, userInfo.sessid);
            loginCanvas.SetActive(false);
            sentSessionId = true;
        }

        if (!gameStarted)
            return;

        //Player Driving
        if (Input.GetKey(KeyCode.W)) {
            ClientBehaviour.SendInfo(SendType.MoveForward, true);
        }
        if (Input.GetKey(KeyCode.S)) {
            ClientBehaviour.SendInfo(SendType.MoveBack, true);
        }
        if (Input.GetKey(KeyCode.A)) {
            ClientBehaviour.SendInfo(SendType.TurnLeft, true);
        }
        if (Input.GetKey(KeyCode.D)) {
            ClientBehaviour.SendInfo(SendType.TurnRight, true);
        }

        //Player Egging
        if (Input.GetMouseButtonDown(0)) {
            ClientBehaviour.SendInfo(SendType.EggThrow, Camera.main.transform.forward, 10.0f);
        }
        if (Input.GetKeyDown(KeyCode.T)) {
            ClientBehaviour.SendInfo(SendType.Text, "This is a text test!");
        }
    }

    private void Receive(SendType sendType, object o, NetworkConnection connection) {
        if(isServer) { //ON SERVER
            //If the input is coming from either player
            switch (sendType) {
                case SendType.SessionId:
                    ServerBehaviour.SetSessionId((string)o, connection);
                    break;
            }

            if (connection == playerTurn) { //If the input is coming from the player whose turn it is to drive
                switch (sendType) {
                    case SendType.MoveForward:
                        carBehaviour.Accelerate();
                        break;
                    case SendType.MoveBack:
                        carBehaviour.Decelerate();
                        break;
                    case SendType.TurnLeft:
                        carBehaviour.TurnLeft();
                        break;
                    case SendType.TurnRight:
                        carBehaviour.TurnRight();
                        break;
                }
            }
            else { //If the input is coming from the player whose turn it is to throw eggs
                switch (sendType) {
                    case SendType.Text:
                        Debug.Log((string)o);
                        break;
                }
            }
        }
        else { //ON CLIENT
            switch (sendType) {
                case SendType.CarPosition:
                    Car.transform.position = (Vector3)o;
                    break;
                case SendType.AssignId:
                    myId = (int)o;
                    if(myId == 0) {
                        carBehaviour.camPlayerOne.gameObject.SetActive(true);
                    }
                    else if(myId == 1) {
                        carBehaviour.camPlayerTwo.gameObject.SetActive(true);
                    }
                    break;
                case SendType.CarRotation:
                    Car.transform.rotation = (Quaternion)o;
                    break;
                case SendType.StartGame:
                    Invoke("StartGame", (float)o);
                    gameTimer = -(float)o;
                    break;
                case SendType.WonGame:
                    if ((bool)o)
                        WinCanvas.SetActive(true);
                    else
                        LossCanvas.SetActive(true);
                    gameStarted = false;
                    break;
            }
        }
    }

    private void Receive(SendType sendType, object[] values, NetworkConnection connection) {
        if(isServer) { //ON SERVER
            if (connection == playerTurn) { //If the input is coming from the player whose turn it is to drive
                
            }
            else {//If the input is coming from the player whose turn it is to throw eggs
                switch (sendType) {
                    case SendType.EggThrow:
                        Vector3 eggDirection = (Vector3)values[0];
                        float eggForce = (float)values[1];
                        Debug.Log(eggDirection);
                        Debug.Log(eggForce);
                        break;
                }
            }
        }
        else { //ON CLIENT

        }
    }

    public void PlayerWin(UserConnection conn) {
        gameStarted = false;
        SetScore(conn.sessionid, gameTimer);
    }
}

public class UserInfo {
    public string sessid;
    public string username;
}
