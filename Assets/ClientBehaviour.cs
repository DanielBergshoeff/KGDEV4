﻿using Unity.Burst;
using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;
using Unity.Jobs;
using System.Net;

public class ClientBehaviour : MonoBehaviour {

    public UdpNetworkDriver m_ClientDriver;
    private NativeArray<NetworkConnection> m_clientToServerConnection;
    private NetworkEndPoint ServerEndPoint;

    public bool Done;

    void Start() {
        m_ClientDriver = new UdpNetworkDriver(new INetworkParameter[0]);
        m_clientToServerConnection = new NativeArray<NetworkConnection>(1, Allocator.Persistent);
        ServerEndPoint = default(NetworkEndPoint);

        ushort port = 9000;

        var endpoint = NetworkEndPoint.LoopbackIpv4;
        endpoint.Port = port;
        ServerEndPoint = endpoint;
    }

    public void OnDestroy() {
        m_ClientDriver.Dispose();
        m_clientToServerConnection.Dispose();

    }

    // Update is called once per frame
    void Update() {
        m_ClientDriver.ScheduleUpdate().Complete();

        if(Done)
            return;

        if (ServerEndPoint.IsValid && !m_clientToServerConnection[0].IsCreated) {
            m_clientToServerConnection[0] = m_ClientDriver.Connect(ServerEndPoint);
        }

        // If the client ui indicates we should not be sending pings but we do have a connection we close that connection
        if (!ServerEndPoint.IsValid && m_clientToServerConnection[0].IsCreated) {
            m_clientToServerConnection[0].Disconnect(m_ClientDriver);
            m_clientToServerConnection[0] = default(NetworkConnection);
        }

        DataStreamReader stream;
        NetworkEvent.Type cmd;
        while ((cmd = m_clientToServerConnection[0].PopEvent(m_ClientDriver, out stream)) !=
            NetworkEvent.Type.Empty) {
            if (cmd == NetworkEvent.Type.Connect) {
                Debug.Log("We are now connected to the server");

                var value = 1;
                using (var writer = new DataStreamWriter(4, Allocator.Temp)) {
                    writer.Write(value);
                    m_clientToServerConnection[0].Send(m_ClientDriver, writer);
                }
            }
            else if (cmd == NetworkEvent.Type.Data) {
                var readerCtx = default(DataStreamReader.Context);
                uint value = stream.ReadUInt(ref readerCtx);
                Debug.Log("Got the value = " + value + " back from the server");
                Done = true;
                m_clientToServerConnection[0].Disconnect(m_ClientDriver);
                m_clientToServerConnection[0] = default(NetworkConnection);
            }
            else if (cmd == NetworkEvent.Type.Disconnect) {
                Debug.Log("Client got disconnected from server");
                m_clientToServerConnection[0] = default(NetworkConnection);
            }
        }
    }
}
