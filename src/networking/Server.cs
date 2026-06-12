using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using LiteNetLib;
using LiteNetLib.Utils;
using TT2026.NetworkedBoardGameEntitySystem;

namespace TT2026.networking;

public class Server : NetworkPeer, IServerNetworkInterface
{
    public ServerGameState GameState { get; private set; }
    private NetworkEventListener _listener;
    private NetManager _netManager;
    
    public Server()
    {
        GameState = new ServerGameState(networkInterface: this);
        _listener = new NetworkEventListener(this);
        _netManager = new NetManager(_listener);
        _netManager.Start(8080);
        GameController.NetworkPeers.Add(this);
        Logger.Log("Server started");
    }
    
    protected override void ReceiveJsonPacket(JsonPacket jsonPacket)
    {
        switch (jsonPacket.Type)
        {
            
#if DEBUG
            default: throw new NotImplementedException($"Unsupported packet type {jsonPacket.Type}");
#else
            default: break;
#endif
        }
    }

    // Game State Network Interface
    public void PushVariableChange(EntityVariableUpdatePacket entityVariableUpdatePacket)
    {
        foreach (NetPeer client in _netManager)
        {
            SendJsonPacket(client, entityVariableUpdatePacket);
        }
    }
    
    // Event Listener
    public override void OnPeerConnected(NetPeer peer)
    {
        foreach (var entity in GameState.EntitiesById.Values)
        {
            foreach (var data in entity.__SyncedData.Values)
            {
                foreach (var syncPacket in data.GenerateSyncPacketsForEntireHistory())
                {
                    SendJsonPacket(peer, syncPacket);
                }
            }
        }
    }

    public override void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
    }

    public override void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
    {
    }

    public override void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
    }

    public override void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {
    }

    public override void OnConnectionRequest(ConnectionRequest request)
    {
        request.Accept();
    }

    public override NetManager NetManager => _netManager;
}