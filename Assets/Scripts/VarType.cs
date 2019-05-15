using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum VarType 
{
    Float,
    Vector3,
    Int,
    Bool,
    Quaternion,
    String
}

public enum SendType {
    //Server to client
    AssignId,
    CarPosition,
    CarRotation,
    TimeLeft,

    //Client 1 to Server
    MoveForward,
    MoveBack,
    TurnLeft,
    TurnRight,

    //Client 2 to server
    EggThrow,

    //String test
    Text
}
