using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using Godot;
using TT2026.libraries.LiteNetLib_2._1._4.LiteNetLib;
using TT2026.Libraries.NetworkedBoardGameEntitySystem;
using TT2026.Libraries.NetworkedBoardGameEntitySystem.Networking;
using TT2026.libraries.NetworkedBoardGameEntitySystem.Rendering;

namespace TT2026.libraries.NetworkedBoardGameEntitySystem.Networking;

public class Client : NetworkPeer
{
    public ClientGameState GameState { get; private set; }
    public GameRenderer Renderer;
    public NetPeer ConnectedServer => _netManager.FirstPeer;
    private NetworkEventListener _listener;
    private NetManager _netManager;
    private TaskCompletionSource<ConnectionState> _connectionTcs = new();
    
    public Client(bool forceCreateGameState = false)
    {
        if (forceCreateGameState) GameState = new ClientGameState(client: this);
        _listener = new NetworkEventListener(this);
        _netManager = new NetManager(_listener);
        _netManager.Start();
        GlobalSingletons.NetworkController.RegisterNetworkPeer(this);
    }

    public void SetRenderer(PackedScene rendererPrefab, Node parentNode)
    {
        if (Renderer is not null)
        {
            Renderer.QueueFree();
        }
        
        Renderer = rendererPrefab.Instantiate<GameRenderer>();
        Renderer.Client = this;
        parentNode.CallDeferred("add_child", Renderer);
        Renderer.Initialize();
        
        if (GameState is not null) Renderer.FullRefresh();
    }

    private (string address, int port, string password) _lastConnection;
    public void Reconnect()
    {
        Connect(_lastConnection.address, _lastConnection.port, _lastConnection.password);
    }

    public async Task<ConnectionState> Connect(string address, int port, string password)
    {
        if (_netManager.FirstPeer is not null)
        {
            _netManager.FirstPeer.Disconnect();
            if (_connectionTcs is not null)
                await _connectionTcs.Task;
        }
        
        if (address == "localhost") address = "127.0.0.1";
        _lastConnection = (address, port, password);
        NetPeer peer = null;
        try
        {
            _connectionTcs = new TaskCompletionSource<ConnectionState>(creationOptions: TaskCreationOptions.RunContinuationsAsynchronously);
            peer = _netManager.Connect(new IPEndPoint(IPAddress.Parse(address), port), key: password);
        }
        catch (FormatException)
        {
            Logger.Log($"Invalid IPv4 address {address}:{port}");
        }

        if (peer is null)
        {
            Logger.Log($"Could not connect to {address}:{port}");
            return ConnectionState.Disconnected;
        }
        else Logger.Log("Client connecting to " + peer.Address + "...");
        await _connectionTcs.Task;
        var result = _connectionTcs.Task.Result;
        _connectionTcs = null;
        return result;
    }

    // Network Event Listener
    protected override NetworkResponse? ReceiveJsonPacket(NetPeer sender, JsonPacket jsonPacket)
    {
        switch (jsonPacket.Type)
        {
            case nameof(EntityVariableUpdatePacket):
                EntityVariableUpdatePacket updatePacket = JsonSerializer.Deserialize<EntityVariableUpdatePacket>(jsonPacket.Payload);
                var updatePacketErr = GameState.ApplyEntityUpdatePacket(updatePacket);
                if (updatePacketErr != EntityUpdatePacketApplyError.AllGroovy)
                    throw new BadPacketException($"Failed to apply entity update: {updatePacketErr}");
                return null;
            
            case nameof(SetStepPacket):
                SetStepPacket packet = JsonSerializer.Deserialize<SetStepPacket>(jsonPacket.Payload);
                GameState.GameStepID = packet.GameStepId;
                GameState.Renderer.EntitiesChanged.Add(Constants.GameStateAdvanceSignalId);
                return null;
            
            case nameof(ResyncHeaderPacket):
                var resyncHeaderPacket = JsonSerializer.Deserialize<ResyncHeaderPacket>(jsonPacket.Payload);
                switch (resyncHeaderPacket.Reason)
                {
                    case ResyncReason.InitialConnect: break; // Gamestate should already be fresh
                    case ResyncReason.LoadingScenario: GameState = new ClientGameState(this); break;
                    case ResyncReason.StateMismatch: GameState = new ClientGameState(this); break;
                    default: throw new NotImplementedException();
                }
                return null;
            
            default: 
                throw new BadPacketException($"Unknown packet type {jsonPacket.Type}");
        }
    }

    protected override void ReceiveCallback(INetworkRequest originalRequest, NetworkResponse response)
    {
        if (response.Error == NetworkResponseError.Error) 
            Logger.Error($"Failed to call {originalRequest.GetType().Name}, server responded with {response.Message}");
    }

    public override void OnPeerConnected(NetPeer peer)
    {
        if (_connectionTcs is null) // TODO Can this state even be reached (the task is null with an outgoing connection request?
        {
            peer.Disconnect();
            return;
        }
        GameState = new ClientGameState(client: this);
        _connectionTcs.TrySetResult(peer.ConnectionState);
    }

    public override void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        if (_connectionTcs is not null)
        {
            _connectionTcs.TrySetResult(ConnectionState.Disconnected);
        }
    }

    public override void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
    {
        if (_connectionTcs is not null)
        {
            _connectionTcs.TrySetResult(ConnectionState.Disconnected);
        }
    }

    public override void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
        return; // If we're not connected to a server, all traffic
    }

    public override void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {
    }

    public override void OnConnectionRequest(ConnectionRequest request)
    {
        return; // Should never happen. Ignore it
    }
    
    public override NetManager NetManager => _netManager;
}