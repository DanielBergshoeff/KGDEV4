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
        { SendType.TimeLeft, VarType.Float },
        { SendType.AssignId, VarType.Int },

        //Client to Server
        { SendType.Forward, VarType.Int }
    };

    public static DataStreamWriter Send(SendType sendType, object value) {
        switch (SendToVar[sendType]) {
            case VarType.Float:
                return SendFloat(sendType, (float)value);
            case VarType.Vector3:
                return SendVector3(sendType, (Vector3)value);
            case VarType.Int:
                return SendInt(sendType, (int)value);

        }
        return default(DataStreamWriter);
    }

    public static DataStreamWriter SendVector3(SendType sendType, Vector3 vector) {

        var writer = new DataStreamWriter(16, Allocator.Temp);
        writer.Write((uint)sendType);
        writer.Write(vector.x);
        writer.Write(vector.y);
        writer.Write(vector.z);

        return writer;
    }

    public static DataStreamWriter SendFloat (SendType sendType, float f) {

        var writer = new DataStreamWriter(8, Allocator.Temp);
        writer.Write((uint)sendType);
        writer.Write(f);

        return writer;
    }

    public static DataStreamWriter SendInt(SendType sendType, int i) {

        var writer = new DataStreamWriter(8, Allocator.Temp);
        writer.Write((uint)sendType);
        writer.Write(i);

        return writer;
    }

    public static void Receive(DataStreamReader stream) {
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
        }

        if(o != null)
            receivedObject.Invoke(sendType, o);
    }
}

public class UnityObjectEvent : UnityEvent<SendType, object> { }
