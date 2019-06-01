using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarBehaviour : MonoBehaviour
{
    public float Speed = 100.0f;
    private Rigidbody myRigidBody;

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
        if (!GameManager.isServer)
            return;

        if (ServerBehaviour.HasClients()) {
            if (posLastFrame != transform.position) {
                ServerBehaviour.SendInfo(SendType.CarPosition, transform.position);
            }

            if (rotLastFrame != transform.rotation) {
                ServerBehaviour.SendInfo(SendType.CarRotation, transform.rotation);
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

    public void Accelerate() {
        if(myRigidBody.velocity.magnitude <= Speed * 2)
            myRigidBody.AddForce(transform.forward * Speed);
    }

    public void Decelerate() {
        if(myRigidBody.velocity.magnitude <= Speed * 2)
            myRigidBody.AddForce(-transform.forward * Speed);
    }

    public void TurnLeft() {
        transform.Rotate(new Vector3(0f, -1.0f, 0f));
    }

    public void TurnRight() {
        transform.Rotate(new Vector3(0f, 1.0f, 0f));
    }
}
