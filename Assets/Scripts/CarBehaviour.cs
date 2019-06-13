using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarBehaviour : MonoBehaviour
{
    public float Speed = 100.0f;
    public Camera camPlayerOne;
    public Camera camPlayerTwo;
    public Camera camPlayerOneBackwards;
    public Camera camPlayerTwoBackwards;

    public GameObject targetPlayerOne;
    public GameObject targetPlayerTwo;

    public Rigidbody myRigidBody;

    private Vector3 posLastFrame;
    private Quaternion rotLastFrame;

    // Start is called before the first frame update
    void Start()
    {
        myRigidBody = GetComponent<Rigidbody>();
        posLastFrame = Vector3.zero;
        rotLastFrame = Quaternion.identity;
    }

    // Update is called once per frame
    void Update()
    {
        if (!(GameManager.Instance is GameManagerServer))
            return;

        if (ServerBehaviour.HasClients()) {
            if (posLastFrame != transform.position) {
                ServerBehaviour.SendInfo(ServerBehaviour.WriteInfo(SendType.CarPosition, transform.position));
            }

            if (rotLastFrame != transform.rotation) {
                ServerBehaviour.SendInfo(ServerBehaviour.WriteInfo(SendType.CarRotation, transform.rotation));
            }

            posLastFrame = transform.position;
            rotLastFrame = transform.rotation;
        }
        else {
            if (Input.GetKey(KeyCode.W)) {
                Accelerate();
            }
            if (Input.GetKey(KeyCode.S)) {
                Decelerate();
            }
            if (Input.GetKey(KeyCode.A)) {
                TurnLeft();
            }
            if (Input.GetKey(KeyCode.D)) {
                TurnRight();
            }
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (!(GameManager.Instance is GameManagerServer))
            return;

        if (!GameManager.Instance.gameStarted)
            return;

        if(other.gameObject == targetPlayerOne) {
            ((GameManagerServer)GameManager.Instance).PlayerWin(ServerBehaviour.GetUserConnectionByPlayerNr(0));
            ServerBehaviour.SendInfo(ServerBehaviour.WriteInfo(SendType.WonGame, true), ServerBehaviour.GetConnectionByPlayerNr(0));
            ServerBehaviour.SendInfo(ServerBehaviour.WriteInfo(SendType.WonGame, false), ServerBehaviour.GetConnectionByPlayerNr(1));
        }
        else if(other.gameObject == targetPlayerTwo) {
            ((GameManagerServer)GameManager.Instance).PlayerWin(ServerBehaviour.GetUserConnectionByPlayerNr(1));
            ServerBehaviour.SendInfo(ServerBehaviour.WriteInfo(SendType.WonGame, true), ServerBehaviour.GetConnectionByPlayerNr(1));
            ServerBehaviour.SendInfo(ServerBehaviour.WriteInfo(SendType.WonGame, false), ServerBehaviour.GetConnectionByPlayerNr(0));
        }
        else if (other.CompareTag("RespawnPosition")) {
            ((GameManagerServer)GameManager.Instance).TouchRespawnPosition(other.gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision) {
        if (!(GameManager.Instance is GameManagerServer))
            return;

        if (collision.gameObject.CompareTag("RespawnTag")) {
            myRigidBody.velocity = Vector3.zero;
            ((GameManagerServer)GameManager.Instance).RespawnCar();
        }
    }

    public void Accelerate() {
        myRigidBody.AddForce(transform.forward * Speed);
        if (myRigidBody.velocity.magnitude > Speed)
            myRigidBody.velocity = Vector3.ClampMagnitude(myRigidBody.velocity, Speed);
    }

    public void Decelerate() {
        myRigidBody.AddForce(-transform.forward * Speed);
        if (myRigidBody.velocity.magnitude > Speed)
            myRigidBody.velocity = Vector3.ClampMagnitude(myRigidBody.velocity, Speed);
    }

    public void TurnLeft() {
        transform.Rotate(new Vector3(0f, -1.0f, 0f));
    }

    public void TurnRight() {
        transform.Rotate(new Vector3(0f, 1.0f, 0f));
    }
}
