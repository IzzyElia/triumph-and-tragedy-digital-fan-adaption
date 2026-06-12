using Godot;
using TT2026.networking;

namespace TT2026;

public partial class TestServer : Node
{
    private Server _server;
    public override void _Ready()
    {
        _server = new Server();
    }
}