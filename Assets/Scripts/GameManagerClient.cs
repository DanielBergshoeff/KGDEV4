using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;

public class GameManagerClient : GameManager
{
    public GameObject WinCanvas;
    public GameObject LossCanvas;
    public GameObject BlindPanel;
    public GameObject blindText;

    public bool sentSessionId;

    public GameObject PreGameCamera;
    private bool driveTurn = false;

    // Update is called once per frame
    protected new void Update()
    {
        base.Update();
        ClientBehaviourMethod();
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
                if (myId == 0)
                    ClientBehaviour.SendInfo(SendType.EggThrow, carBehaviour.camPlayerOneBackwards.transform.forward, 500.0f);
                else
                    ClientBehaviour.SendInfo(SendType.EggThrow, carBehaviour.camPlayerTwoBackwards.transform.forward, 500.0f);
            }
            if (Input.GetKeyDown(KeyCode.T)) {
                ClientBehaviour.SendInfo(SendType.Text, "This is a text test!");
            }
        }
    }

    private void Unblind() {
        BlindPanel.SetActive(false);
    }

    private void RemoveBlindText() {
        blindText.SetActive(false);
    }

    protected override void Receive(SendType sendType, object o, NetworkConnection connection) {
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

    protected override void Receive(SendType sendType, object[] values, NetworkConnection connection) {
        switch (sendType) {
            //Add sendtypes here
            case SendType.EggSpawn:
                GameObject egg = Instantiate(EggPrefab, (Vector3)values[0], Quaternion.identity);
                egg.GetComponent<Rigidbody>().AddForce((Vector3)values[1]);
                break;
        }
    }
}
