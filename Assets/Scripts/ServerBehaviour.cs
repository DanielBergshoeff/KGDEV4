using Unity.Burst;
using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;
using Unity.Jobs;

public class ServerBehaviour : MonoBehaviour
{
    public static ServerBehaviour Instance;

    public UdpNetworkDriver m_ServerDriver;
    private NativeList<NetworkConnection> m_Connections;

    // Start by creating a driver for the client and an address for the server.
    void Start() {
        Instance = this;

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

        SQPDriver.ServerPort = serverPort;
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
                if (cmd == NetworkEvent.Type.Data) {
                    Communication.Receive(stream, i);

                    /*
                    using (var writer = new DataStreamWriter(4, Allocator.Temp)) {
                        writer.Write(number);
                        m_ServerDriver.Send(NetworkPipeline.Null, m_Connections[i], writer);
                    }*/
                }
                else if (cmd == NetworkEvent.Type.Disconnect) {
                    Debug.Log("Client disconnected from server");
                    m_Connections[i] = default(NetworkConnection);
                }
            }
        }
    }

    public static void SendInfo(SendType sendType, object value) {
        DataStreamWriter writer = Communication.Send(sendType, value);

        for (int i = 0; i < Instance.m_Connections.Length; i++) {
            if (!Instance.m_Connections[i].IsCreated)
                continue;
            Instance.m_Connections[i].Send(Instance.m_ServerDriver, writer);
        }

        writer.Dispose();
    }

    public void OnDestroy() {
        m_ServerDriver.Dispose();
        m_Connections.Dispose();
    }
}
