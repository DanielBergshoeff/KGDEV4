using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static int playerIdTurn;
    public static bool isServer;

    public static int myId;

    public GameObject Car;
    private CarBehaviour carBehaviour;
    public bool localIsServer;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        Communication.receivedObject = new UnityObjectEvent();
        Communication.receivedObject.AddListener(Receive);

        Communication.receivedObjects = new UnityObjectsEvent();
        Communication.receivedObjects.AddListener(Receive);

        carBehaviour = Car.GetComponent<CarBehaviour>();

        playerIdTurn = 1;

        isServer = localIsServer;
    }

    // Update is called once per frame
    void Update()
    {
        if(isServer) {
            ServerBehaviourMethod();
        }
        else {
            ClientBehaviourMethod();
        }
    }

    private void ServerBehaviourMethod() {

    }

    private void ClientBehaviourMethod() {
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
        if (Input.GetMouseButtonDown(0)) {
            ClientBehaviour.SendInfo(SendType.EggThrow, new Vector3(1f, 0.2f, 0f), 10.0f);
        }
        if (Input.GetKeyDown(KeyCode.T)) {
            ClientBehaviour.SendInfo(SendType.Text, "This is a text test!");
        }
    }

    private void Receive(SendType sendType, object o, int connection) {
        if(isServer) { //ON SERVER
            if (connection == playerIdTurn) { //If the input is coming from the player whose turn it is to drive
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
                    break;
                case SendType.CarRotation:
                    Car.transform.rotation = (Quaternion)o;
                    break;
            }
        }
    }

    private void Receive(SendType sendType, object[] values, int connection) {
        if(isServer) { //ON SERVER
            if (connection == playerIdTurn) { //If the input is coming from the player whose turn it is to drive
                
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
}
