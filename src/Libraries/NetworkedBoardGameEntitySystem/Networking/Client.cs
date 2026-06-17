using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using TT2026.libraries.LiteNetLib_2._1._4.LiteNetLib;
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
    
    public Client(GameRenderer renderer, bool forceCreateGameState = false)
    {
        Renderer = renderer;
        renderer.Client = this;
        if (forceCreateGameState) GameState = new ClientGameState(client: this);
        _listener = new NetworkEventListener(this);
        _netManager = new NetManager(_listener);
        _netManager.Start();
        GlobalSingletons.NetworkController.RegisterNetworkPeer(this);
    }

    private (string address, int port, string password) _lastConnection;
    public void Reconnect()
    {
        Connect(_lastConnection.address, _lastConnection.port, _lastConnection.password);
    }

    public void Connect(string address, int port, string password)
    {
        if (address == "localhost") address = "127.0.0.1";
        _lastConnection = (address, port, password);
        NetPeer peer = null;
        try
        {
            peer = _netManager.Connect(new IPEndPoint(IPAddress.Parse(address), port), key: password);
        }
        catch (FormatException)
        {
            Logger.Log($"Invalid IPv4 address {address}:{port}");
        }

        if (peer is null)
        {
            Logger.Log($"Could not connect to {address}:{port}");
            return;
        }
        else Logger.Log("Client connecting to " + peer.Address + "...");
    }

    // Network Event Listener
    protected override NetworkResponse? ReceiveJsonPacket(JsonPacket jsonPacket)
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
        GameState = new ClientGameState(client: this);
    }

    public override void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        
    }

    public override void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
    {
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