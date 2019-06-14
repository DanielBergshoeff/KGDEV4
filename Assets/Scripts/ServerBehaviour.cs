using Unity.Burst;
using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;
using System.Collections.Generic;
using Unity.Jobs;


public class ServerBehaviour : MonoBehaviour
{
    public static ServerBehaviour Instance;
    public UdpNetworkDriver ServerDriver;
    public Dictionary<UserConnection, NetworkConnection> ConnectionToUserInfo;

    private NativeList<NetworkConnection> m_Connections;
    private int amtOfPlayers;

    // Start by creating a driver for the client and an address for the server.
    void Start() {
        Instance = this;

        ServerDriver = new UdpNetworkDriver(new INetworkParameter[0]);
        var addr = NetworkEndPoint.AnyIpv4;
        addr.Port = 9000;
        if (ServerDriver.Bind(addr) != 0)
            Debug.Log("Failed to bind to port ...");
        else {
            ServerDriver.Listen();
            Debug.Log("Server created!");

        }
        m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);

        SQPDriver.ServerPort = 9000;

        ConnectionToUserInfo = new Dictionary<UserConnection, NetworkConnection>();
        amtOfPlayers = 0;
    }

    void Update() {

        ServerDriver.ScheduleUpdate().Complete();

        // Clean up connections
        for (int i = 0; i < m_Connections.Length; i++) {
            if (!m_Connections[i].IsCreated) {
                m_Connections.RemoveAtSwapBack(i);
                --i;
            }
        }

        // Accept new connections
        NetworkConnection c;
        while (true) {
            var con = ServerDriver.Accept();
            if (!con.IsCreated)
                break;
            m_Connections.Add(con);

            GameManager.myId++;
            DataStreamWriter writer = Communication.Write(SendType.AssignId, GameManager.myId);
            con.Send(ServerDriver, writer);

            Debug.Log("Accepted a connection");
        }

        DataStreamReader stream;
        for (int i = 0; i < m_Connections.Length; i++) {
            if (!m_Connections[i].IsCreated)
                continue;
            NetworkEvent.Type cmd;
            while ((cmd = ServerDriver.PopEventForConnection(m_Connections[i], out stream)) !=
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
        foreach(UserConnection ui in Instance.ConnectionToUserInfo.Keys){
            if (ui.sessionid == sessid)
                receivedSession = ui;
        }

        if (receivedSession != null) {
            Instance.ConnectionToUserInfo[receivedSession] = connection;
        }
        else {
            UserConnection ui = new UserConnection();
            ui.sessionid = sessid;
            ui.connection = Instance.amtOfPlayers;

            Instance.ConnectionToUserInfo.Add(ui, connection);
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
        DataStreamWriter writer = Communication.Write(sendType, value);
        return writer;
    }

    public static DataStreamWriter WriteInfo(SendType sendType, params object[] values) {
        DataStreamWriter writer = Communication.Write(sendType, values);
        return writer;
    }

    public static void SendInfo(DataStreamWriter writer, params NetworkConnection[] conn) {
        if (Instance != null) {
            if (conn.Length > 0) {
                for (int i = 0; i < conn.Length; i++) {
                    if (!conn[i].IsCreated)
                        continue;
                    conn[i].Send(Instance.ServerDriver, writer);
                }
            }
            else {
                for (int i = 0; i < Instance.m_Connections.Length; i++) {
                    if (!Instance.m_Connections[i].IsCreated)
                        continue;
                    Instance.m_Connections[i].Send(Instance.ServerDriver, writer);
                }
            }
        }

        writer.Dispose();
    }

    public static NetworkConnection GetConnectionByPlayerNr(int playerNr) {
        foreach(UserConnection uc in Instance.ConnectionToUserInfo.Keys) {
            if (uc.connection == playerNr)
                return Instance.ConnectionToUserInfo[uc];
        }

        return default(NetworkConnection);
    }

    public static UserConnection GetUserConnectionByPlayerNr(int playerNr) {
        foreach (UserConnection uc in Instance.ConnectionToUserInfo.Keys) {
            if (uc.connection == playerNr)
                return uc;
        }

        return null;
    }

    public static int GetPlayerNrByConnection(NetworkConnection connection) {
        UserConnection uc = KeyByValue(Instance.ConnectionToUserInfo, connection);
        return uc.connection;
    }

    public static bool HasClients() {
        return Instance.m_Connections.Length > 0;
    }

    public void OnDestroy() {
        ServerDriver.Dispose();
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
