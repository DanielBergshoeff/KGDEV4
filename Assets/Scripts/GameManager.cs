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
            ClientBehaviour.SendInfo(SendType.Forward, myId);
        }
    }

    private void Receive(SendType sendType, object o) {
        if(isServer) { //ON SERVER
            switch (sendType) {
                case SendType.Forward:
                    if ((int)o == playerIdTurn)
                        carBehaviour.Accelerate();
                    break;
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
            }
        }
    }
}
