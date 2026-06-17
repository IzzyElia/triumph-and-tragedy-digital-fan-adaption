using TT2026.Libraries.NetworkedBoardGameEntitySystem.Networking;

namespace TT2026.libraries.NetworkedBoardGameEntitySystem.Networking;

public struct SetStepPacket(int gameStepId) : INetworkRequest
{
    public int GameStepId { get; set; } = gameStepId;
}