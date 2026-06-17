using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using TT2026.libraries.LiteNetLib_2._1._4.LiteNetLib;
using TT2026.libraries.LiteNetLib_2._1._4.LiteNetLib.Utils;
using TT2026.Libraries.NetworkedBoardGameEntitySystem.Networking;

namespace TT2026.libraries.NetworkedBoardGameEntitySystem.Networking;

public abstract class NetworkPeer : INetEventListener
{
    public const int TimeoutSeconds = 30;
    
    public readonly object Mutex = new object();
    private NetDataWriter _dataWriter = new ();
    public Dictionary<int, SentRequest> SentRequests { get; } = new ();

    private List<int> _deadRequests = new();
    public void MonitorTimeouts()
    {
        lock (Mutex)
        {
            foreach (var request in SentRequests.Values)
            {
                if (DateTime.UtcNow > request.TimeSent + TimeSpan.FromSeconds(TimeoutSeconds))
                {
                    if (_deadRequests.Any()) _deadRequests.Clear();
                    _deadRequests.Add(request.CallbackId);
                }
            }

            if (_deadRequests.Any())
            {
                foreach (var requestCallbackId in _deadRequests)
                {
                    SentRequest originalRequest =  SentRequests[requestCallbackId];
                    SentRequests.Remove(requestCallbackId);
                    if (originalRequest.Destination.ConnectionState == ConnectionState.Connected)
                        originalRequest.Destination.Disconnect();
                }
                _deadRequests.Clear();
            }
        }
    }
    
    public void SendJsonPacket<TPayloadType>(NetPeer destination, TPayloadType payload, int callbackId)
    {
        lock (Mutex)
        {
            _dataWriter.Reset();
            
            string payloadJson = JsonSerializer.Serialize(payload, payload.GetType());
            string packetJson = JsonSerializer.Serialize(new JsonPacket(type: payload.GetType().Name, payloadJson, callbackId));
            _dataWriter.Put(packetJson);
            destination.Send(_dataWriter, DeliveryMethod.ReliableUnordered);
        }
    }
    
    Random _random = new();
    public void SendRequest(NetPeer destination, INetworkRequest networkRequest)
    {
        lock (Mutex)
        {
            int callbackId = _random.Next();
            while (SentRequests.ContainsKey(callbackId) || callbackId == 0) callbackId = _random.Next();
            SentRequests.Add(callbackId, new SentRequest(destination, networkRequest, callbackId));
            SendJsonPacket(destination, networkRequest, callbackId);
        }
    }

    public async Task<NetworkResponse> SendRequestAwaitCallback(NetPeer destination, INetworkRequest networkRequest)
    {
        SentRequest sentRequest;
        lock (Mutex)
        {
            int callbackId = _random.Next();
            while (SentRequests.ContainsKey(callbackId) || callbackId == 0) callbackId = _random.Next();
            sentRequest = new SentRequest(destination, networkRequest, callbackId, tcs: new (TaskCreationOptions.RunContinuationsAsynchronously));
            SentRequests.Add(callbackId, sentRequest);
            SendJsonPacket(destination, networkRequest, callbackId);
        }

        await sentRequest.TCS.Task;

        return sentRequest.TCS.Task.Result;
    }

    protected abstract NetworkResponse? ReceiveJsonPacket(JsonPacket jsonPacket);
    protected abstract void ReceiveCallback(INetworkRequest originalRequest, NetworkResponse response);
    
    public abstract void OnPeerConnected(NetPeer peer);
    public abstract void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo);
    public abstract void OnNetworkError(IPEndPoint endPoint, SocketError socketError);

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber,
        DeliveryMethod deliveryMethod)
    {
        lock (Mutex)
        {
            JsonPacket jsonPacket;
            try
            {
                jsonPacket = JsonSerializer.Deserialize<JsonPacket>(reader.GetString());
            }
            catch (ArgumentException)
            {
                reader.Recycle();
                return;
            }
            catch (JsonException)
            {
                reader.Recycle();
                return;
            }
            catch (NotSupportedException)
            {
                reader.Recycle();
                return;
            }

            if (jsonPacket.Type == nameof(NetworkResponse))
            {
                NetworkResponse receivedResponse = JsonSerializer.Deserialize<NetworkResponse>(jsonPacket.Payload);
                if (SentRequests.TryGetValue(jsonPacket.CallbackId, out var originalRequest))
                {
                    originalRequest.TCS?.SetResult(receivedResponse);
                    SentRequests.Remove(jsonPacket.CallbackId);
                    ReceiveCallback(originalRequest.Request, receivedResponse);
                }
            }
            else
            {
                NetworkResponse? outgoingResponse;
                try
                {
                    outgoingResponse = ReceiveJsonPacket(jsonPacket);
                }
                catch (BadPacketException)
                {
                    Logger.Log($"Received bad packet from client at {peer.Address}");
                    outgoingResponse = new NetworkResponse(jsonPacket.CallbackId, "Bad Packet", NetworkResponseError.Error);
                }
                catch (Exception e)
                {
                    Logger.Error(e.ToString());
                    outgoingResponse = new NetworkResponse(jsonPacket.CallbackId, "Unhandled Server Error", NetworkResponseError.Error);
                }
                if (outgoingResponse is not null) SendJsonPacket(peer, outgoingResponse, jsonPacket.CallbackId);
            }

            reader.Recycle();
        }
    }
    public abstract void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType);
    public abstract void OnNetworkLatencyUpdate(NetPeer peer, int latency);
    public abstract void OnConnectionRequest(ConnectionRequest request);
    public abstract NetManager NetManager { get; }
}