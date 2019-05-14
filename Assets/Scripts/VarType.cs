using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum VarType 
{
    Float,
    Vector3,
    Int,
    Bool,
    Quaternion
}

public enum SendType {
    //Server to client
    AssignId,
    CarPosition,
    CarRotation,
    TimeLeft,

    //Client to Server
    Forward,
    TurnLeft,
    TurnRight
}
