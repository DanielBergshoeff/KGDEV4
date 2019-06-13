using Unity.Burst;
using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UdpCNetworkDriver = Unity.Networking.Transport.UdpNetworkDriver;


public class ServerBehaviour : MonoBehaviour
{
    public static ServerBehaviour Instance;

    public UdpCNetworkDriver m_ServerDriver;
    private NativeList<NetworkConnection> m_Connections;

    private int amtOfPlayers;
    public Dictionary<UserConnection, NetworkConnection> connectionToUserInfo;

    // Start by creating a driver for the client and an address for the server.
    void Start() {
        Instance = this;

        m_ServerDriver = new UdpCNetworkDriver(new INetworkParameter[0]);
        if (m_ServerDriver.Bind(NetworkEndPoint.Parse("0.0.0.0", 9000)) != 0)
            Debug.Log("Failed to bind to port ...");
        else
            m_ServerDriver.Listen();

        m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);

        /*
        ushort serverPort = 9000;
        ushort newPort = 0;
        if (CommandLine.TryGetCommandLineArgValue("-port", out newPort))
            serverPort = newPort;
        // Create the server driver, bind it to a port and start listening for incoming connections
        m_ServerDriver = new UdpNetworkDriver(new INetworkParameter[0]);
        var addr = NetworkEndPoint.AnyIpv4;
        addr.Port = serverPort;
        if (m_ServerDriver.Bind(addr) != 0)
            Debug.Log($"Failed to bind to port {serverPort}");
        else
            m_ServerDriver.Listen();
            

        m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);
        connectionToUserInfo = new Dictionary<UserConnection, NetworkConnection>();

        SQPDriver.ServerPort = serverPort;  */

        amtOfPlayers = 0;
    }

    void Update() {

        m_ServerDriver.ScheduleUpdate().Complete();

        // Clean up connections
        for (int i = 0; i < m_Connections.Length; i++) {
            if (!m_Connections[i].IsCreated) {
                m_Connections.RemoveAtSwapBack(i);
                --i;
            }
        }

        // Accept new connections
        NetworkConnection c;
        while ((c = m_ServerDriver.Accept()) != default(NetworkConnection)) {
            m_Connections.Add(c);

            GameManager.myId++;
            DataStreamWriter writer = Communication.Send(SendType.AssignId, GameManager.myId);
            c.Send(m_ServerDriver, writer);

            Debug.Log("Accepted a connection");
        }

        DataStreamReader stream;
        for (int i = 0; i < m_Connections.Length; i++) {
            if (!m_Connections[i].IsCreated)
                continue;
            NetworkEvent.Type cmd;
            while ((cmd = m_ServerDriver.PopEventForConnection(m_Connections[i], out stream)) !=
                NetworkEvent.Type.Empty) {
                if(cmd == NetworkEvent.Type.Connect) {

                }
                if (cmd == NetworkEvent.Type.Data) {
                    Communication.Receive(stream, m_Connections[i]);
                }
                else if (cmd == NetworkEvent.Type.Disconnect) {
                    Debug.Log("Client disconnected from server");
                    m_Connections[i] = default(NetworkConnection);
                }
            }
        }
    }

    public static void SetSessionId(string sessid, NetworkConnection connection) {
        UserConnection receivedSession = null;
        foreach(UserConnection ui in Instance.connectionToUserInfo.Keys){
            if (ui.sessionid == sessid)
                receivedSession = ui;
        }

        if (receivedSession != null) {
            Instance.connectionToUserInfo[receivedSession] = connection;
        }
        else {
            UserConnection ui = new UserConnection();
            ui.sessionid = sessid;
            ui.connection = Instance.amtOfPlayers;

            Instance.connectionToUserInfo.Add(ui, connection);
            SendInfo(WriteInfo(SendType.AssignId, Instance.amtOfPlayers), connection);
            Instance.amtOfPlayers++;
            if(Instance.amtOfPlayers == 1) {
                //GameManager.playerTurn = connection;
            }
            else if(Instance.amtOfPlayers == 2) {
                GameManagerServer.playerTurn = connection;
                ((GameManagerServer)GameManager.Instance).SwitchRoles();

                SendInfo(WriteInfo(SendType.StartGame, 5.0f));
                Instance.Invoke("StartGame", 5.0f);
            }
        }
    }

    public void StartGame() {
        GameManager.Instance.gameStarted = true;
    }

    public static DataStreamWriter WriteInfo(SendType sendType, object value) {
        DataStreamWriter writer = Communication.Send(sendType, value);
        return writer;
    }

    public static DataStreamWriter WriteInfo(SendType sendType, params object[] values) {
        DataStreamWriter writer = Communication.Send(sendType, values);
        return writer;
    }

    public static void SendInfo(DataStreamWriter writer, params NetworkConnection[] conn) {
        if (Instance != null) {
            if (conn.Length > 0) {
                for (int i = 0; i < conn.Length; i++) {
                    if (!conn[i].IsCreated)
                        continue;
                    conn[i].Send(Instance.m_ServerDriver, writer);
                }
            }
            else {
                for (int i = 0; i < Instance.m_Connections.Length; i++) {
                    if (!Instance.m_Connections[i].IsCreated)
                        continue;
                    Instance.m_Connections[i].Send(Instance.m_ServerDriver, writer);
                }
            }
        }

        writer.Dispose();
    }

    public static NetworkConnection GetConnectionByPlayerNr(int playerNr) {
        foreach(UserConnection uc in Instance.connectionToUserInfo.Keys) {
            if (uc.connection == playerNr)
                return Instance.connectionToUserInfo[uc];
        }

        return default(NetworkConnection);
    }

    public static UserConnection GetUserConnectionByPlayerNr(int playerNr) {
        foreach (UserConnection uc in Instance.connectionToUserInfo.Keys) {
            if (uc.connection == playerNr)
                return uc;
        }

        return null;
    }

    public static int GetPlayerNrByConnection(NetworkConnection connection) {
        UserConnection uc = KeyByValue(Instance.connectionToUserInfo, connection);
        return uc.connection;
    }

    public static bool HasClients() {
        return Instance.m_Connections.Length > 0;
    }

    public void OnDestroy() {
        m_ServerDriver.Dispose();
        m_Connections.Dispose();
    }

    public static UserConnection KeyByValue(Dictionary<UserConnection, NetworkConnection> dict, NetworkConnection val) {
        UserConnection key = null;
        foreach (KeyValuePair<UserConnection, NetworkConnection> pair in dict) {
            if (pair.Value == val) {
                key = pair.Key;
                break;
            }
        }
        return key;
    }
}

public class UserConnection {
    public string sessionid;
    public int connection;
}
