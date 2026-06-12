using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using LiteNetLib;
using TT2026.IzzysUI;
using TT2026.IzzysUI.Tooltips;
using TT2026.NetworkedBoardGameEntitySystem;

namespace TT2026.networking;

public class Client : NetworkPeer, IClientNetworkInterface
{
    public ClientGameState GameState { get; private set; }
    private NetworkEventListener _listener;
    private NetManager _netManager;
    
    public Client()
    {
        _listener = new NetworkEventListener(this);
        _netManager = new NetManager(_listener);
        _netManager.Start();
        GameController.NetworkPeers.Add(this);
    }
    
    protected override void ReceiveJsonPacket(JsonPacket jsonPacket)
    {
        switch (jsonPacket.Type)
        {
            case nameof(EntityVariableUpdatePacket):
                var updatePacket = JsonSerializer.Deserialize<EntityVariableUpdatePacket>(jsonPacket.payload);
                var err = GameState.ApplyEntityUpdatePacket(updatePacket);
                if (err != EntityUpdatePacketApplyError.AllGroovy) throw new BadPacketException($"Failed to apply entity update. JSON below:\n\n{jsonPacket.payload}\n\n");
                break;
            default: throw new BadPacketException($"Received nsupported packet type {jsonPacket.Type}");
        }
    }

    public void Connect(string address, int port, string password)
    {
        if (address == "localhost") address = "127.0.0.1";
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

    // Game State Network Interface
    private Queue<EntityVariableUpdatePacket> _entityVariableUpdatePacketQueue = new();
    public bool TryDequeueVariableChangeUpdate(out EntityVariableUpdatePacket entityVariableUpdatePacket)
    {
        return _entityVariableUpdatePacketQueue.TryDequeue(out entityVariableUpdatePacket);
    }

    // Network Event Listener
    public override void OnPeerConnected(NetPeer peer)
    {
        _entityVariableUpdatePacketQueue.Clear();
        GameState = new ClientGameState(networkInterface: this);
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