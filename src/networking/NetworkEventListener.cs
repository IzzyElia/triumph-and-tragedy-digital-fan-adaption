using System;
using System.Net;
using System.Net.Sockets;
using LiteNetLib;

namespace TT2026.networking;

public class NetworkEventListener : INetEventListener
{
    private INetEventListener _master;
    public NetworkEventListener(INetEventListener master)
    {
        _master = master;
    }
    
    public void OnPeerConnected(NetPeer peer)
    {
        Logger.Log($"{_master.GetType().Name} Peer Connected - \n" +
                   $"\tid = {peer.Id}" +
                   $"\taddress = {peer.Address}",
            onlyShowInContext: LoggingContexts.Networking);
        _master.OnPeerConnected(peer);
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        Logger.Log($"{_master.GetType().Name} Peer Disconnected - \n" +
                   $"\tid = {peer.Id}" +
                   $"\treason = {disconnectInfo.Reason}",
            onlyShowInContext: LoggingContexts.Networking);
        _master.OnPeerDisconnected(peer, disconnectInfo);
    }

    public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
    {
        Logger.Log($"{_master.GetType().Name} Network !ERROR! - \n" +
                   $"\terror = {Enum.GetName(socketError)}" +
                   $"\tclient = {endPoint.Address}",
            onlyShowInContext: LoggingContexts.Networking);
        _master.OnNetworkError(endPoint, socketError);
    }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
        _master.OnNetworkReceive(peer, reader, channelNumber, deliveryMethod);
    }

    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
        _master.OnNetworkReceiveUnconnected(remoteEndPoint, reader, messageType);
    }

    public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {
        _master.OnNetworkLatencyUpdate(peer, latency);
    }

    public void OnConnectionRequest(ConnectionRequest request)
    {
        Logger.Log($"{_master.GetType().Name} Connection Request - \n" +
                   $"\tfrom = {request.RemoteEndPoint.Address}",
            onlyShowInContext: LoggingContexts.Networking);
        _master.OnConnectionRequest(request);
    }
}