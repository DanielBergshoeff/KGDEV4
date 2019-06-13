using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.Events;
using System.Text;
using static MenuBehaviour;
using UnityEngine.Networking;

public static class Communication {
    public static UnityObjectEvent receivedObject;
    public static UnityObjectsEvent receivedObjects;
    //private static string server = "https://studenthome.hku.nl/~daniel.bergshoeff/KGDEV4/";
    private static string server = "http://localhost/KGDEV4/";


    /// <summary>
    /// Dictionary containing all the SendTypes with their respective VarTypes
    /// </summary>
    public static readonly Dictionary<SendType, VarType> SendToVar = new Dictionary<SendType, VarType> {
        //Server to client
        { SendType.CarPosition, VarType.Vector3 },
        { SendType.CarRotation, VarType.Quaternion },
        { SendType.TimeLeft, VarType.Float },
        { SendType.AssignId, VarType.Int },
        { SendType.StartGame, VarType.Float },
        { SendType.WonGame, VarType.Bool },
        { SendType.DriveTurn, VarType.Bool },
        { SendType.EggHit, VarType.Bool },

        //Client to Server
        { SendType.MoveForward, VarType.Bool },
        { SendType.MoveBack, VarType.Bool },
        { SendType.TurnLeft, VarType.Bool },
        { SendType.TurnRight, VarType.Bool },

        //String test
        { SendType.Text, VarType.String },
        { SendType.SessionId, VarType.String }
    };

    /// <summary>
    /// Dictionary containing the capacity cost per VarType
    /// </summary>
    public static readonly Dictionary<VarType, int> VarToCost = new Dictionary<VarType, int> {
        { VarType.Bool, 1 },
        { VarType.Float, 4 },
        { VarType.Int, 4 },
        { VarType.Vector3, 12 },
        { VarType.Quaternion, 16 }
    };

    /// <summary>
    /// Dictionary containing the VarTypes of SendTypes with multiple Vars
    /// </summary>
    public static readonly Dictionary<SendType, VarType[]> SendToVars = new Dictionary<SendType, VarType[]> {
        { SendType.EggThrow, new VarType[] {VarType.Vector3, VarType.Float} },
        { SendType.EggSpawn, new VarType[] {VarType.Vector3, VarType.Vector3} }
    };

    /// <summary>
    /// Returns a DataStreamWriter containing the value of the SendType
    /// </summary>
    /// <param name="sendType"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static DataStreamWriter Send(SendType sendType, object value) {
        DataStreamWriter writer = default(DataStreamWriter);
        int dataCost = 0;

        if (VarToCost.ContainsKey(SendToVar[sendType]))
            dataCost += 4 + VarToCost[SendToVar[sendType]];
        else {
            switch (SendToVar[sendType]) {
                case VarType.String:
                    var amtOfBytes = Encoding.ASCII.GetBytes((string)value).Length;
                    dataCost += 4 + 4 + amtOfBytes;
                    break;
            }
        }
        writer = new DataStreamWriter(dataCost, Allocator.Temp);
        writer.Write((uint)sendType);

        SendValue(SendToVar[sendType], value, ref writer);
        return writer;
    }

    /// <summary>
    /// Returns a DataStreamWriter containing all the values of the SendType
    /// </summary>
    /// <param name="sendType"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    public static DataStreamWriter Send(SendType sendType, params object[] values) {
        DataStreamWriter writer = default(DataStreamWriter);

        if (values.Length != SendToVars[sendType].Length)
            return writer;

        int dataCost = 4;

        for (int i = 0; i < values.Length; i++) {
            dataCost += VarToCost[SendToVars[sendType][i]];
        }

        writer = new DataStreamWriter(dataCost, Allocator.Temp);

        writer.Write((uint)sendType);

        for (int i = 0; i < values.Length; i++) {
            SendValue(SendToVars[sendType][i], values[i], ref writer);
        }

        return writer;
    }


    /// <summary>
    /// Adds the value of value to writer
    /// </summary>
    /// <param name="varType"></param>
    /// <param name="value"></param>
    /// <param name="writer"></param>
    private static void SendValue(VarType varType, object value, ref DataStreamWriter writer) {
        switch (varType) {
            case VarType.Float:
                SendFloat((float)value, ref writer);
                break;

            case VarType.Vector3:
                SendVector3((Vector3)value, ref writer);
                break;

            case VarType.Int:
                SendInt((int)value, ref writer);
                break;

            case VarType.Bool:
                SendBool((bool)value, ref writer);
                break;

            case VarType.Quaternion:
                SendQuaternion((Quaternion)value, ref writer);
                break;

            case VarType.String:
                SendString((string)value, ref writer);
                break;
        }
    }

    /// <summary>
    /// Adds a string to the writer
    /// </summary>
    /// <param name="value"></param>
    /// <param name="writer"></param>
    private static void SendString(string value, ref DataStreamWriter writer) {
        byte[] bytes = Encoding.ASCII.GetBytes(value);
        writer.Write(bytes.Length);
        writer.Write(bytes);
    }

    /// <summary>
    /// Adds a Vector3 to the writer
    /// </summary>
    /// <param name="vector"></param>
    /// <param name="writer"></param>
    private static void SendVector3( Vector3 vector, ref DataStreamWriter writer) {
        writer.Write(vector.x);
        writer.Write(vector.y);
        writer.Write(vector.z);
    }

    /// <summary>
    /// Adds a Quaternion to the writer
    /// </summary>
    /// <param name="quaternion"></param>
    /// <param name="writer"></param>
    private static void SendQuaternion(Quaternion quaternion, ref DataStreamWriter writer) {
        writer.Write(quaternion.x);
        writer.Write(quaternion.y);
        writer.Write(quaternion.z);
        writer.Write(quaternion.w);
    }

    /// <summary>
    /// Adds a float to the writer
    /// </summary>
    /// <param name="f"></param>
    /// <param name="writer"></param>
    private static void SendFloat (float f, ref DataStreamWriter writer) {
        writer.Write(f);
    }

    /// <summary>
    /// Adds an integer to the writer
    /// </summary>
    /// <param name="i"></param>
    /// <param name="writer"></param>
    private static void SendInt(int i, ref DataStreamWriter writer) {
        writer.Write(i);
    }

    /// <summary>
    /// Adds a boolean to the writer
    /// </summary>
    /// <param name="b"></param>
    /// <param name="writer"></param>
    private static void SendBool(bool b, ref DataStreamWriter writer) {
        byte x = b ? (byte)1 : (byte)0;
        writer.Write(x);
    }

    /// <summary>
    /// Reads the received stream and converts it to the original values
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="connection"></param>
    public static void Receive(DataStreamReader stream, NetworkConnection connection) {
        var readerCtx = default(DataStreamReader.Context);
        SendType sendType = (SendType)stream.ReadUInt(ref readerCtx);
        if (!SendToVars.ContainsKey(sendType)) {
            object o = ReadObject(ref stream, SendToVar[sendType], ref readerCtx);

            if (o != null)
                receivedObject.Invoke(sendType, o, connection);
        }
        else {
            object[] objects = new object[SendToVars[sendType].Length];
            for (int i = 0; i < objects.Length; i++) {
                objects[i] = ReadObject(ref stream, SendToVars[sendType][i], ref readerCtx);
            }

            if (objects.Length > 0)
                receivedObjects.Invoke(sendType, objects, connection);
        }
    }

    /// <summary>
    /// Reads the stream and returns the correct object
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="varType"></param>
    /// <param name="readerCtx"></param>
    /// <returns></returns>
    private static object ReadObject(ref DataStreamReader stream, VarType varType, ref DataStreamReader.Context readerCtx) {
        object o = null;
        switch (varType) {
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
            case VarType.String:
                int amtOfBytes = stream.ReadInt(ref readerCtx);
                o = System.Text.Encoding.ASCII.GetString(stream.ReadBytesAsArray(ref readerCtx, amtOfBytes));
                break;
        }

        return o;
    }

    public static IEnumerator GetRequest(string url, System.Action<string> callBack = null) {
        url = server + url;
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url)) {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = url.Split('/');
            int page = pages.Length - 1;

            if (webRequest.isNetworkError) {
                Debug.Log(pages[page] + ": Error: " + webRequest.error);
            }
            else {
                Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                callBack?.Invoke(webRequest.downloadHandler.text);
            }
        }
    }
}

public class UnityObjectEvent : UnityEvent<SendType, object, NetworkConnection> { }

public class UnityObjectsEvent : UnityEvent<SendType, object[], NetworkConnection> { }


public class VarTypeValue {
    public VarType varType;
    public object value;
}
