namespace TT2026.libraries.NetworkedBoardGameEntitySystem.Networking;

public struct JsonPacket(string type, string payload, int callbackId)
{
    public int CallbackId { get; set; } = callbackId;
    public string Type { get; set; } = type;
    public string Payload { get; set; } = payload;
}