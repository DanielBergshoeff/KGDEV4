using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;

public class GameManagerServer : GameManager {
    public static NetworkConnection playerTurn;

    public GameObject TriggerPlayerOne;
    public GameObject TriggerPlayerTwo;
    public GameObject RespawnParent;

    private List<GameObject> respawnPositions;
    private int currentRespawnPosition;
    private float switchTimer = 10f;

    // Start is called before the first frame update
    new void Start() {
        base.Start();
        respawnPositions = new List<GameObject>();
        for (int i = 0; i < RespawnParent.transform.childCount; i++) {
            respawnPositions.Add(RespawnParent.transform.GetChild(i).gameObject);
        }
        playerTurn = default(NetworkConnection);
    }

    // Update is called once per frame
    protected new void Update() {
        base.Update();
        if (gameStarted) {
            switchTimer -= Time.deltaTime;
            if (switchTimer <= 0f) {
                SwitchRoles();
                switchTimer = 10f;
            }
        }
    }

    public void RespawnCar() {
        Car.transform.position = respawnPositions[currentRespawnPosition].transform.position;
        Car.transform.rotation = Quaternion.identity;
        carBehaviour.myRigidBody.velocity = Vector3.zero;
        carBehaviour.myRigidBody.angularVelocity = Vector3.zero;
        switchTimer = 0f;
    }

    public void SwitchRoles() {
        int tempPlayerTurn = playerTurn.InternalId;
        foreach (NetworkConnection nc in ServerBehaviour.Instance.ConnectionToUserInfo.Values) {
            if (nc.InternalId == tempPlayerTurn) {
                ServerBehaviour.SendInfo(ServerBehaviour.WriteInfo(SendType.DriveTurn, false), nc);
            }
            else {
                playerTurn = nc;
                ServerBehaviour.SendInfo(ServerBehaviour.WriteInfo(SendType.DriveTurn, true), nc);
            }
        }
    }

    public void SetScore(string sessionId, float time) {
        string setscore = "insertscore.php?sessid=" + sessionId + "&score=" + time.ToString("F2").Replace(',', '.');
        StartCoroutine(Communication.GetRequest(setscore));
    }

    public void PlayerWin(UserConnection conn) {
        gameStarted = false;
        SetScore(conn.sessionid, gameTimer);
    }

    public void TouchRespawnPosition(GameObject go) {
        if (respawnPositions.Contains(go)) {
            currentRespawnPosition = respawnPositions.IndexOf(go);
        }
    }

    protected override void Receive(SendType sendType, object o, NetworkConnection connection) {
        //If the input is coming from either player
        switch (sendType) {
            case SendType.SessionId:
                ServerBehaviour.SetSessionId((string)o, connection);
                break;
        }

        if (connection == playerTurn) { //If the input is coming from the player whose turn it is to drive
            switch (sendType) {
                case SendType.MoveForward:
                    if (ServerBehaviour.GetPlayerNrByConnection(connection) == 0)
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

    protected override void Receive(SendType sendType, object[] values, NetworkConnection connection) {
        if (connection == playerTurn) { //If the input is coming from the player whose turn it is to drive

        }
        else {//If the input is coming from the player whose turn it is to throw eggs
            switch (sendType) {
                case SendType.EggThrow:
                    Vector3 posToThrowFrom = Vector3.zero;
                    Vector3 eggDirection = (Vector3)values[0];
                    float eggForce = (float)values[1];
                    if (ServerBehaviour.KeyByValue(ServerBehaviour.Instance.ConnectionToUserInfo, connection).connection == 0) {
                        posToThrowFrom = carBehaviour.camPlayerOneBackwards.transform.position;
                    }
                    else {
                        posToThrowFrom = carBehaviour.camPlayerTwoBackwards.transform.position;
                    }
                    GameObject egg = Instantiate(EggPrefab, posToThrowFrom, Quaternion.identity);
                    egg.GetComponent<Rigidbody>().AddForce(eggDirection * eggForce);
                    ServerBehaviour.SendInfo(ServerBehaviour.WriteInfo(SendType.EggSpawn, posToThrowFrom, eggDirection * eggForce));
                    break;
            }
        }
    }
}
