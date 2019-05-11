using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum VarType 
{
    Float,
    Vector3,
    Int
}

public enum SendType {
    //Server to client
    AssignId,
    CarPosition,
    TimeLeft,

    //Client to Server
    Forward,
}
