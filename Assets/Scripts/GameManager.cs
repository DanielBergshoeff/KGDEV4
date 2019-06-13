using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.Networking;


public class GameManager : MonoBehaviour
{
    //Both
    public static GameManager Instance;
    public static NetworkConnection playerTurn;
    public static bool isServer;

    public static int myId;

    [Header("Both")]
    public GameObject Car;

    public bool localIsServer;
    public UnityEngine.UI.Text GameTimerText;

    private CarBehaviour carBehaviour;
    public bool gameStarted = false;
    private float gameTimer = 0.0f;
    private float blindTime = 3.0f;
    public GameObject blindText;

    //Client
    [Header("Client")]
    public GameObject WinCanvas;
    public GameObject LossCanvas;
    public GameObject BlindPanel;

    public GameObject triggerPlayerOne;
    public GameObject triggerPlayerTwo;
    public bool sentSessionId;

    public GameObject PreGameCamera;
    private bool driveTurn = false;

    //Server
    [Header("Server")]
    public GameObject EggPrefab;
    public GameObject RespawnParent;
    private List<GameObject> RespawnPositions;
    private int currentRespawnPosition;
    private float switchTimer = 10f;

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
        isServer = localIsServer;

        if (!isServer)
            return;

        RespawnPositions = new List<GameObject>();
        for (int i = 0; i < RespawnParent.transform.childCount; i++) {
            RespawnPositions.Add(RespawnParent.transform.GetChild(i).gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (gameTimer < 0.0f || gameStarted) {
            if (isServer) {
                if (gameStarted) {
                    switchTimer -= Time.deltaTime;
                    if(switchTimer <= 0f) {
                        SwitchRoles();
                        switchTimer = 10f;
                    }
                }
            }

            gameTimer += Time.deltaTime;
            GameTimerText.text = gameTimer.ToString("F2");
        }

        if(isServer) {
            ServerBehaviourMethod();
        }
        else {
            ClientBehaviourMethod();
        }

        if (Input.GetKeyDown(KeyCode.P)) {
            Vector3 posToThrowFrom = Vector3.zero;
            Vector3 eggDirection = (-transform.forward + transform.up).normalized;
            float eggForce = 500.0f;
            posToThrowFrom = carBehaviour.camPlayerOneBackwards.transform.position;
            GameObject egg = Instantiate(EggPrefab, posToThrowFrom, Quaternion.identity);
            egg.GetComponent<Rigidbody>().AddForce(eggDirection * eggForce);
        }
    }

    public void RespawnCar() {
        Car.transform.position = RespawnPositions[currentRespawnPosition].transform.position;
        Car.transform.rotation = Quaternion.identity;
        carBehaviour.myRigidBody.velocity = Vector3.zero;
        carBehaviour.myRigidBody.angularVelocity = Vector3.zero;
        switchTimer = 0f;
    }

    public void SwitchRoles() {
        int tempPlayerTurn = playerTurn.InternalId;
        foreach (NetworkConnection nc in ServerBehaviour.Instance.connectionToUserInfo.Values) {
            if (nc.InternalId == tempPlayerTurn) {
                ServerBehaviour.SendInfo(ServerBehaviour.WriteInfo(SendType.DriveTurn, false), nc);
            }
            else {
                playerTurn = nc;
                ServerBehaviour.SendInfo(ServerBehaviour.WriteInfo(SendType.DriveTurn, true), nc);
            }
        }
    }

    public void StartGame() {
        gameStarted = true;
    }
    

    public void SetScore(string sessionId, float time) {
        string setscore = "https://studenthome.hku.nl/~daniel.bergshoeff/KGDEV4/insertscore.php?sessid=" + sessionId + "&score=" + time.ToString("F2").Replace(',', '.');
        StartCoroutine(Communication.GetRequest(setscore));
    }

    

    private void ServerBehaviourMethod() {
        
    }

    private void BackToMenu() {
        MenuBehaviour.BackToMenu();
    }

    private void ClientBehaviourMethod() {
        if (MenuBehaviour.userInfo == default(MenuBehaviour.UserInfo)) //If the player is not logged in, return to menu
            BackToMenu();

        if (!ClientBehaviour.Instance.clientToServerConnectionMade) //If there's no connection, return
            return;

        if (!sentSessionId) {
            ClientBehaviour.SendInfo(SendType.SessionId, MenuBehaviour.userInfo.sessid);
            sentSessionId = true;
        }

        if (!gameStarted)
            return;

        if (driveTurn) {
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
        }
        else {
            //Player Egging
            if (Input.GetMouseButtonDown(0)) {
                if(myId == 0)
                    ClientBehaviour.SendInfo(SendType.EggThrow, carBehaviour.camPlayerOneBackwards.transform.forward, 500.0f);
                else
                    ClientBehaviour.SendInfo(SendType.EggThrow, carBehaviour.camPlayerTwoBackwards.transform.forward, 500.0f);
            }
            if (Input.GetKeyDown(KeyCode.T)) {
                ClientBehaviour.SendInfo(SendType.Text, "This is a text test!");
            }
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
                        if(ServerBehaviour.GetPlayerNrByConnection(connection) == 0)
                            carBehaviour.Accelerate();
                        else
                            carBehaviour.Decelerate();
                        break;
                    case SendType.MoveBack:
                        if (ServerBehaviour.GetPlayerNrByConnection(connection) == 0)
                            carBehaviour.Decelerate();
                        else
                            carBehaviour.Accelerate();
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
                case SendType.CarPosition: //If the Car Position has been received, set the Car position to value
                    Car.transform.position = (Vector3)o;
                    break;
                case SendType.AssignId: //If an Id has been assigned by the server, set Id to value and enable one of the cameras, depending on Id
                    myId = (int)o;
                    break;
                case SendType.CarRotation: //If the Car Rotation has been received, set the Car rotation to value
                    Car.transform.rotation = (Quaternion)o;
                    break;
                case SendType.StartGame: //If the Start Game float has been received, set a timer for value amount of seconds until game start
                    Invoke("StartGame", (float)o);
                    gameTimer = -(float)o;
                    Destroy(PreGameCamera);
                    if (myId == 0) {
                        carBehaviour.camPlayerOne.gameObject.SetActive(true);
                    }
                    else if (myId == 1) {
                        carBehaviour.camPlayerTwoBackwards.gameObject.SetActive(true);
                    }
                    break;
                case SendType.WonGame: //If the Won Game bool has been received, activate either the win or loss canvas depending on value
                    if ((bool)o)
                        WinCanvas.SetActive(true);
                    else
                        LossCanvas.SetActive(true);
                    gameStarted = false;
                    Invoke("BackToMenu", 5.0f);
                    break;
                case SendType.DriveTurn: //If the Drive Turn bool has been received, activate either the forward or backward camera depending on value and id
                    driveTurn = (bool)o;
                    if (myId == 0) {
                        if (driveTurn) {
                            carBehaviour.camPlayerOne.gameObject.SetActive(true);
                            carBehaviour.camPlayerOneBackwards.gameObject.SetActive(false);
                        }
                        else {
                            carBehaviour.camPlayerOneBackwards.gameObject.SetActive(true);
                            carBehaviour.camPlayerOne.gameObject.SetActive(false);
                        }
                    }
                    else {
                        if (driveTurn) {
                            carBehaviour.camPlayerTwo.gameObject.SetActive(true);
                            carBehaviour.camPlayerTwoBackwards.gameObject.SetActive(false);
                        }
                        else {
                            carBehaviour.camPlayerTwoBackwards.gameObject.SetActive(true);
                            carBehaviour.camPlayerTwo.gameObject.SetActive(false);
                        }
                    }
                    break;
                case SendType.EggHit: //If the Egg Hit bool has been received, either blind player or visually show the other player has been blinded depending on value
                    if ((bool)o) {
                        BlindPanel.SetActive(true);
                        Invoke("Unblind", blindTime);
                    }
                    else {
                        blindText.SetActive(true);
                        Invoke("RemoveBlindText", blindTime);
                    }
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
                        Vector3 posToThrowFrom = Vector3.zero;
                        Vector3 eggDirection = (Vector3)values[0];
                        float eggForce = (float)values[1];
                        if(ServerBehaviour.KeyByValue(ServerBehaviour.Instance.connectionToUserInfo, connection).connection == 0) {
                            posToThrowFrom = carBehaviour.camPlayerOneBackwards.transform.position;
                        }
                        else {
                            posToThrowFrom = carBehaviour.camPlayerTwoBackwards.transform.position;
                        }
                        GameObject egg = Instantiate(EggPrefab, posToThrowFrom, Quaternion.identity);
                        egg.GetComponent<Rigidbody>().AddForce(eggDirection * eggForce);
                        break;
                }
            }
        }
        else { //ON CLIENT

        }
    }

    public void UnBlind() {
        BlindPanel.SetActive(false);
    }

    public void RemoveBlindText() {
        blindText.SetActive(false);
    }

    public void PlayerWin(UserConnection conn) {
        gameStarted = false;
        SetScore(conn.sessionid, gameTimer);
    }

    public void TouchRespawnPosition(GameObject go) {
        if (RespawnPositions.Contains(go)) {
            currentRespawnPosition = RespawnPositions.IndexOf(go);
        }
    }
}


