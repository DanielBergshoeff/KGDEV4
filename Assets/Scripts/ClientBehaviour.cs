using Unity.Burst;
using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;
using Unity.Jobs;
using System.Net;

public class ClientBehaviour : MonoBehaviour {

    public static ClientBehaviour Instance;

    public UdpNetworkDriver m_ClientDriver;
    private NetworkConnection m_clientToServerConnection;
    public bool clientToServerConnectionMade = false;
    private NetworkEndPoint ServerEndPoint;

    public bool Done;

    void Start() {
        Instance = this;

        /*
        m_ClientDriver = new UdpNetworkDriver(new INetworkParameter[0]);
        m_clientToServerConnection = new NativeArray<NetworkConnection>(1, Allocator.Persistent);
        ServerEndPoint = default(NetworkEndPoint);

        ushort port = 9000;

        var endpoint = NetworkEndPoint.LoopbackIpv4;
        endpoint.Port = port;
        ServerEndPoint = endpoint;*/

        m_ClientDriver = new UdpNetworkDriver(new INetworkParameter[0]);
        m_clientToServerConnection = default(NetworkConnection);

        ServerEndPoint = NetworkEndPoint.Parse("192.168.1.16", 9000);
        m_clientToServerConnection = m_ClientDriver.Connect(ServerEndPoint);
    }

    public void OnDestroy() {
        m_ClientDriver.Dispose();
        //m_clientToServerConnection.Dispose();

    }

    // Update is called once per frame
    void Update() {
        m_ClientDriver.ScheduleUpdate().Complete();

        if(Done)
            return;

        if (ServerEndPoint.IsValid && !m_clientToServerConnection.IsCreated) {
            m_clientToServerConnection = m_ClientDriver.Connect(ServerEndPoint);
        }

        // If the client ui indicates we should not be sending pings but we do have a connection we close that connection
        if (!ServerEndPoint.IsValid && m_clientToServerConnection.IsCreated) {
            m_clientToServerConnection.Disconnect(m_ClientDriver);
            m_clientToServerConnection = default(NetworkConnection);
        }

        DataStreamReader stream;
        NetworkEvent.Type cmd;
        while ((cmd = m_clientToServerConnection.PopEvent(m_ClientDriver, out stream)) !=
            NetworkEvent.Type.Empty) {
            if (cmd == NetworkEvent.Type.Connect) {
                Debug.Log("We are now connected to the server");
                clientToServerConnectionMade = true;
            }
            else if (cmd == NetworkEvent.Type.Data) {
                Communication.Receive(stream, m_clientToServerConnection);
            }
            else if (cmd == NetworkEvent.Type.Disconnect) {
                Debug.Log("Client got disconnected from server");
                ((GameManagerClient)GameManager.Instance).sentSessionId = false;
                m_clientToServerConnection = default(NetworkConnection);
                clientToServerConnectionMade = false;
            }
        }
    }

    public static void SendInfo(SendType sendType, object value) {
        DataStreamWriter writer = Communication.Send(sendType, value);
        
        Instance.m_clientToServerConnection.Send(Instance.m_ClientDriver, writer);

        writer.Dispose();
    }

    public static void SendInfo(SendType sendType, params object[] values) {
        DataStreamWriter writer = Communication.Send(sendType, values);

        Instance.m_clientToServerConnection.Send(Instance.m_ClientDriver, writer);

        writer.Dispose();
    }


}
