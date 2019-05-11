using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarBehaviour : MonoBehaviour
{
    private Rigidbody myRigidBody;

    private Vector3 posLastFrame;

    // Start is called before the first frame update
    void Start()
    {
        myRigidBody = GetComponent<Rigidbody>();
        posLastFrame = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        if(posLastFrame != transform.position) {
            ServerBehaviour.SendInfo(SendType.CarPosition, transform.position);
        }

        posLastFrame = transform.position;
    }

    public void Accelerate() {
        myRigidBody.AddForce(transform.forward * 20.0f);
    }
}
