using System;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using TT2026.libraries.LiteNetLib_2._1._4.LiteNetLib;
using TT2026.Libraries.NetworkedBoardGameEntitySystem.Networking;

namespace TT2026.libraries.NetworkedBoardGameEntitySystem.Networking;

public class Server : NetworkPeer
{
    public ServerGameState GameState { get; private set; }
    private NetworkEventListener _listener;
    private NetManager _netManager;
    
    public Server(int port)
    {
        GameState = new ServerGameState(server: this);
        _listener = new NetworkEventListener(this);
        _netManager = new NetManager(_listener);
        _netManager.Start(port);
        GlobalSingletons.NetworkController.RegisterNetworkPeer(this);
        Logger.Log("Server started");
    }
    
    protected override NetworkResponse? ReceiveJsonPacket(JsonPacket jsonPacket)
    {
        switch (jsonPacket.Type)
        {
            case nameof(EditorPacket):
                // TODO EditorPackets are meant for scenario editing, so allow EditorPackets from approved clients
                var editorPacket =  JsonSerializer.Deserialize<EditorPacket>(jsonPacket.Payload);
                Logger.Log($"Server received editor packet: {jsonPacket.Payload}");
                try
                {
                    EditorPacketResponse editorResponse = GameState.HandleEditorPacket(editorPacket);
                    return new NetworkResponse(jsonPacket.CallbackId, editorResponse);
                }
                catch (Exception e)
                {
                    return new NetworkResponse(jsonPacket.CallbackId, $"Error applying edit on server: {e}", NetworkResponseError.Error);
                }
                break;
#if DEBUG
            default: throw new NotImplementedException($"Unsupported packet type {jsonPacket.Type}");
#else
            default: break;
#endif
        }
    }

    protected override void ReceiveCallback(INetworkRequest originalRequest, NetworkResponse response)
    {
        return; // Currently the server is fire-and-forget only
    }

    // Game State Network Interface
    public void PushUpdate(INetworkRequest networkUpdate)
    {
        foreach (NetPeer client in _netManager)
        {
            SendJsonPacket(client, networkUpdate, 0);
        }
    }
    
    // Event Listener
    public override void OnPeerConnected(NetPeer peer)
    {
        lock (Mutex)
        {
            SendJsonPacket(peer, new SetStepPacket(GameState.GameStepID), 0);
            foreach (var entity in GameState.EntitiesById.Values)
            {
                foreach (var data in entity.__SyncedData.Values)
                {
                    foreach (var syncPacket in data.GenerateSyncPacketsForEntireHistory())
                    {
                        SendJsonPacket(peer, syncPacket, 0);
                    }
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