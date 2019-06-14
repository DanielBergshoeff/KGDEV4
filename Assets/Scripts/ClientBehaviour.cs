using Unity.Burst;
using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;
using Unity.Jobs;
using System.Net;

public class ClientBehaviour : MonoBehaviour {

    public static ClientBehaviour Instance;
    public UdpNetworkDriver ClientDriver;
    public bool ClientToServerConnectionMade = false;
    public bool Done;

    private NetworkConnection m_clientToServerConnection;
    private NetworkEndPoint serverEndPoint;

    void Start() {
        Instance = this;
        ClientDriver = new UdpNetworkDriver(new INetworkParameter[0]);
        m_clientToServerConnection = default(NetworkConnection);

        serverEndPoint = new NetworkEndPoint();
        serverEndPoint = NetworkEndPoint.Parse("192.168.1.16", 9000);
        m_clientToServerConnection = ClientDriver.Connect(serverEndPoint);
    }

    public void OnDestroy() {
        ClientDriver.Dispose();
    }

    // Update is called once per frame
    void Update() {
        ClientDriver.ScheduleUpdate().Complete();

        if(Done)
            return;

        if (serverEndPoint.IsValid && !m_clientToServerConnection.IsCreated) {
            m_clientToServerConnection = ClientDriver.Connect(serverEndPoint);
            Debug.Log("Try to connect");
        }

        // If the client ui indicates we should not be sending pings but we do have a connection we close that connection
        if (!serverEndPoint.IsValid && m_clientToServerConnection.IsCreated) {
            m_clientToServerConnection.Disconnect(ClientDriver);
            m_clientToServerConnection = default(NetworkConnection);
            Debug.Log("Close connection because it's not valid");
        }

        Debug.Log("Things seem fine");

        DataStreamReader stream;
        NetworkEvent.Type cmd;
        while ((cmd = m_clientToServerConnection.PopEvent(ClientDriver, out stream)) !=
            NetworkEvent.Type.Empty) {
            Debug.Log("Package received?");
            if (cmd == NetworkEvent.Type.Connect) {
                Debug.Log("We are now connected to the server");
                ClientToServerConnectionMade = true;
            }
            else if (cmd == NetworkEvent.Type.Data) {
                Communication.Receive(stream, m_clientToServerConnection);
            }
            else if (cmd == NetworkEvent.Type.Disconnect) {
                Debug.Log("Client got disconnected from server");
                ((GameManagerClient)GameManager.Instance).sentSessionId = false;
                m_clientToServerConnection = default(NetworkConnection);
                ClientToServerConnectionMade = false;
            }
        }
    }

    public static void SendInfo(SendType sendType, object value) {
        DataStreamWriter writer = Communication.Write(sendType, value);
        
        Instance.m_clientToServerConnection.Send(Instance.ClientDriver, writer);

        writer.Dispose();
    }

    public static void SendInfo(SendType sendType, params object[] values) {
        DataStreamWriter writer = Communication.Write(sendType, values);

        Instance.m_clientToServerConnection.Send(Instance.ClientDriver, writer);

        writer.Dispose();
    }


}
