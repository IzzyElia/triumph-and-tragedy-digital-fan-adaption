using System;
using System.Threading.Tasks;
using TT2026.libraries.LiteNetLib_2._1._4.LiteNetLib;

namespace TT2026.Libraries.NetworkedBoardGameEntitySystem.Networking;

/// <summary>
/// Represents a request sent over the network that awaits a callback
/// </summary>
public struct SentRequest (NetPeer destination, INetworkRequest request, int callbackId, TaskCompletionSource<NetworkResponse> tcs = null)
{
    public NetPeer Destination { get; } = destination;
    public INetworkRequest Request { get; private set; } = request;
    public int CallbackId { get; private set; } = callbackId;
    public DateTime TimeSent { get; private set; } = DateTime.UtcNow;
    public TaskCompletionSource<NetworkResponse> TCS { get; private set; } = tcs;
}