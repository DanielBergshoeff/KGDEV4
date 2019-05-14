using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.Events;

public static class Communication {
    public static UnityObjectEvent receivedObject;

    public static readonly Dictionary<SendType, VarType> SendToVar = new Dictionary<SendType, VarType> {
        //Server to client
        { SendType.CarPosition, VarType.Vector3 },
        { SendType.CarRotation, VarType.Quaternion },
        { SendType.TimeLeft, VarType.Float },
        { SendType.AssignId, VarType.Int },

        //Client to Server
        { SendType.Forward, VarType.Bool },
        { SendType.TurnLeft, VarType.Bool },
        { SendType.TurnRight, VarType.Bool }
    };

    public static DataStreamWriter Send(SendType sendType, object value) {
        switch (SendToVar[sendType]) {
            case VarType.Float:
                return SendFloat(sendType, (float)value);
            case VarType.Vector3:
                return SendVector3(sendType, (Vector3)value);
            case VarType.Int:
                return SendInt(sendType, (int)value);
            case VarType.Bool:
                return SendBool(sendType, (bool)value);
            case VarType.Quaternion:
                return SendQuaternion(sendType, (Quaternion)value);

        }
        return default(DataStreamWriter);
    }

    private static DataStreamWriter SendVector3(SendType sendType, Vector3 vector) {

        var writer = new DataStreamWriter(16, Allocator.Temp);
        writer.Write((uint)sendType);
        writer.Write(vector.x);
        writer.Write(vector.y);
        writer.Write(vector.z);

        return writer;
    }

    private static DataStreamWriter SendQuaternion(SendType sendType, Quaternion quaternion) {
        var writer = new DataStreamWriter(20, Allocator.Temp);
        writer.Write((uint)sendType);
        writer.Write(quaternion.x);
        writer.Write(quaternion.y);
        writer.Write(quaternion.z);
        writer.Write(quaternion.w);

        return writer;
    }

    private static DataStreamWriter SendFloat (SendType sendType, float f) {

        var writer = new DataStreamWriter(8, Allocator.Temp);
        writer.Write((uint)sendType);
        writer.Write(f);

        return writer;
    }

    private static DataStreamWriter SendInt(SendType sendType, int i) {

        var writer = new DataStreamWriter(8, Allocator.Temp);
        writer.Write((uint)sendType);
        writer.Write(i);

        return writer;
    }

    private static DataStreamWriter SendBool(SendType sendType, bool b) {
        var writer = new DataStreamWriter(1, Allocator.Temp);
        writer.Write((uint)sendType);
        byte x = b ? (byte)1 : (byte)0;
        writer.Write(x);

        return writer;
    }

    public static void Receive(DataStreamReader stream, int connection) {
        var readerCtx = default(DataStreamReader.Context);
        SendType sendType = (SendType)stream.ReadUInt(ref readerCtx);
        object o = null;
        switch (SendToVar[sendType]) {
            case VarType.Float:
                o = stream.ReadFloat(ref readerCtx);
                break;
            case VarType.Vector3:
                o = new Vector3(stream.ReadFloat(ref readerCtx), stream.ReadFloat(ref readerCtx), stream.ReadFloat(ref readerCtx));
                break;
            case VarType.Int:
                o = stream.ReadInt(ref readerCtx);
                break;
            case VarType.Bool:
                byte b = stream.ReadByte(ref readerCtx);
                o = false;
                if (b == 1)
                    o = true;
                break;
            case VarType.Quaternion:
                o = new Quaternion(stream.ReadFloat(ref readerCtx), stream.ReadFloat(ref readerCtx), stream.ReadFloat(ref readerCtx), stream.ReadFloat(ref readerCtx));
                break;
        }

        if(o != null)
            receivedObject.Invoke(sendType, o, connection);
    }
}

public class UnityObjectEvent : UnityEvent<SendType, object, int> { }
