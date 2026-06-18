using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using TT2026.libraries.LiteNetLib_2._1._4.LiteNetLib;
using TT2026.Libraries.NetworkedBoardGameEntitySystem.Networking;
using TT2026.Libraries.NetworkedBoardGameEntitySystem.Saving;

namespace TT2026.libraries.NetworkedBoardGameEntitySystem.Networking;

public class Server : NetworkPeer
{
    public ServerGameState GameState { get; private set; }
    private NetworkEventListener _listener;
    private NetManager _netManager;
    private NetPeer _incomingSaveFileClient = null;
    private SortedList<int, ImportGameStatePacket> _incomingSaveFileChunks = null;
    
    public Server(int port)
    {
        GameState = new ServerGameState(server: this);
        _listener = new NetworkEventListener(this);
        _netManager = new NetManager(_listener);
        _netManager.Start(port);
        GlobalSingletons.NetworkController.RegisterNetworkPeer(this);
        Logger.Log("Server started");
    }

    private List<NetPeer> _peersAloc = new();
    protected override NetworkResponse? ReceiveJsonPacket(NetPeer sender, JsonPacket jsonPacket)
    {
        switch (jsonPacket.Type)
        {
            case nameof(EditorPacket):
                // TODO Authenticate Editor Packets
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
                
            case nameof(ImportGameStatePacket):
                // TODO Authenticate Editor Packets
                // TODO The chunking system may be unstable, ex if there's a dropped packet or the client disconnects mid-send
                if (_incomingSaveFileClient is not null && !_incomingSaveFileClient.Equals(sender)) return new NetworkResponse(jsonPacket.CallbackId, "Server currently importing save file from another client", NetworkResponseError.Error);

                if (_incomingSaveFileClient is null)
                {
                    _incomingSaveFileChunks = new SortedList<int, ImportGameStatePacket>();
                    _incomingSaveFileClient = sender;
                }
                
                var importPacket = JsonSerializer.Deserialize<ImportGameStatePacket>(jsonPacket.Payload);
                _incomingSaveFileChunks.Add(importPacket.Part, importPacket);
                if (_incomingSaveFileChunks.Count == importPacket.NumParts)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var chunk in _incomingSaveFileChunks)
                    {
                        sb.Append(chunk.Value.JSON);
                    }
                    ServerGameState gameState;
                    try
                    {
                        gameState = GameStateSaver.CreateGamestateFromJson(this, sb.ToString());
                    }
                    catch (InvalidOperationException)
                    {
                        Logger.Log($"Failed to deserialize save file from client at {sender.Address}");
                        return new NetworkResponse(jsonPacket.CallbackId, "Failed to deserialize provided JSON", NetworkResponseError.Error);
                    }

                    GameState = gameState;
                    foreach (var client in EnumerateConnectedPeers()) ResyncClient(client, ResyncReason.LoadingScenario);
                    _incomingSaveFileChunks.Clear();
                    _incomingSaveFileClient = null;
                }
                var response = new ImportGameStatePacketResponse(success: true);
                return new NetworkResponse(jsonPacket.CallbackId, response);
#if DEBUG
            default: throw new NotImplementedException($"Unsupported packet type {jsonPacket.Type}");
#else
            default: break;
#endif
        }
    }

    private IEnumerable<NetPeer> EnumerateConnectedPeers()
    {
        lock (_peersAloc)
        {
            _netManager.GetConnectedPeers(_peersAloc);
            foreach (var peer in _peersAloc)
            {
                yield return peer;
            }
        }
    }

    protected override void ReceiveCallback(INetworkRequest originalRequest, NetworkResponse response)
    {
        return; // Currently the server is fire-and-forget only
    }

    private void ResyncClient(NetPeer client, ResyncReason reason)
    {
        lock (Mutex)
        {
            SendJsonPacket(client, new ResyncHeaderPacket(reason: reason), 0);
            SendJsonPacket(client, new SetStepPacket(GameState.GameStepID), 0);
            foreach (var entity in GameState.EntitiesById.Values)
            {
                foreach (var data in entity.__SyncedData.Values)
                {
                    foreach (var syncPacket in data.GenerateSyncPacketsForEntireHistory())
                    {
                        SendJsonPacket(client, syncPacket, 0);
                    }
                }
            }
        }
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
        ResyncClient(peer, ResyncReason.InitialConnect);
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