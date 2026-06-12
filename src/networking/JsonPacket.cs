namespace TT2026.networking;

public struct JsonPacket(string type, string payload)
{
    public string Type { get; set; } = type;
    public string payload { get; set; } = payload;
}