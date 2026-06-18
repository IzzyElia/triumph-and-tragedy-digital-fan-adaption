using TT2026.Libraries.NetworkedBoardGameEntitySystem.Networking;

namespace TT2026.libraries.NetworkedBoardGameEntitySystem.Networking;

public struct ImportGameStatePacket(int part, int numParts, string json, string editorAuthKey) : INetworkRequest
{
    public int Part { get; set; } = part;
    public int NumParts { get; set; } = numParts;
    public string JSON {get; set;} = json;
    public string EditorAuthenticationKey { get; set; } = editorAuthKey;
}

public struct ImportGameStatePacketResponse (bool success)
{
    public bool Success { get; set; } = success;
}