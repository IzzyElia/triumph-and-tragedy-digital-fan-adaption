using Godot;
using TT2026.networking;

namespace TT2026;

public partial class TestClient : Node
{
    Client _client;
    public override void _Ready()
    {
        _client = new Client();
    }

    public void Connect()
    {
        _client.Connect("localhost", 8080, "");
    }
}