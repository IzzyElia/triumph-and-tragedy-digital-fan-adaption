using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using LiteNetLib;
using LiteNetLib.Utils;

namespace TT2026.networking;

public abstract class NetworkPeer : INetEventListener
{
    private NetDataWriter _dataWriter = new ();
    public void SendJsonPacket<TPayloadType>(NetPeer destination, TPayloadType payload)
    {
        lock (_dataWriter)
        {
            _dataWriter.Reset();
            
            string payloadJson = JsonSerializer.Serialize(payload);
            string packetJson = JsonSerializer.Serialize(new JsonPacket(type: typeof(TPayloadType).Name, payloadJson));
            _dataWriter.Put(packetJson);
            destination.Send(_dataWriter, DeliveryMethod.ReliableUnordered);
        }
    }

    protected abstract void ReceiveJsonPacket(JsonPacket jsonPacket);
    
    public abstract void OnPeerConnected(NetPeer peer);
    public abstract void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo);
    public abstract void OnNetworkError(IPEndPoint endPoint, SocketError socketError);

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber,
        DeliveryMethod deliveryMethod)
    {
        JsonPacket jsonPacket = JsonSerializer.Deserialize<JsonPacket>(reader.GetString());
        try
        {
            ReceiveJsonPacket(jsonPacket);
        }
        catch (BadPacketException)
        {
            Logger.Log($"Received bad packet from client at {peer.Address}, disconnecting...");
            peer.Disconnect();
        }
        reader.Recycle();
    }
    public abstract void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType);
    public abstract void OnNetworkLatencyUpdate(NetPeer peer, int latency);
    public abstract void OnConnectionRequest(ConnectionRequest request);
    public abstract NetManager NetManager { get; }
}