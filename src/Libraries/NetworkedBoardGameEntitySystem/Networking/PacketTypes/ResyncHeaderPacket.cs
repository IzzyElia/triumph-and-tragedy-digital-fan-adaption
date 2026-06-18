using TT2026.Libraries.NetworkedBoardGameEntitySystem.Networking;

namespace TT2026.libraries.NetworkedBoardGameEntitySystem.Networking;

/// <summary>
/// Tells a client a resync is incoming and the reason for the resync
/// </summary>
public struct ResyncHeaderPacket(ResyncReason reason) : INetworkRequest
{
    public ResyncReason Reason { get; set; }
}

public enum ResyncReason
{
    InitialConnect,
    StateMismatch,
    LoadingScenario
}