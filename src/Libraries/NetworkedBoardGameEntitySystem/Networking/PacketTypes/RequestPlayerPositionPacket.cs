namespace TT2026.Libraries.NetworkedBoardGameEntitySystem.Networking.PacketTypes;

public struct RequestPlayerPositionPacket : INetworkRequest
{
    public int RequestedPlayerId { get; set; }
}