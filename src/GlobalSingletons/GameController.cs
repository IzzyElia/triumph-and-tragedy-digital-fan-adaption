using System.Collections.Generic;
using Godot;
using Godot.Collections;
using TT2026.IzzysUI;
using TT2026.IzzysUI.Tooltips;
using TT2026.networking;

namespace TT2026;

public partial class GameController : Node
{
    [Export] private Array<LoggingContexts> _loggingContexts;
    private static GameController _instance;
    private HashSet<NetworkPeer> _networkPeers = new HashSet<NetworkPeer>();
    public static HashSet<NetworkPeer> NetworkPeers => _instance._networkPeers;
    public override void _EnterTree()
    {
        _instance = this;
        foreach (LoggingContexts context in _loggingContexts)
        {
            Logger.SetEnabledContext(context, true);
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        foreach (var peer in NetworkPeers)
        {
            peer.NetManager.PollEvents();
        }
    }

    public override void _UnhandledKeyInput(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent)
        {
            if (keyEvent.IsReleased())
            {
                if (keyEvent.Keycode == Key.Key1)
                {
                    IzzysUIController.OpenContextWindow(new ContextWindowInfo(
                        header: "Server Debug Menu",
                        interactions: [
                            new SimpleUIAction("Restart Server", () =>
                            {
                                TestServer server = new TestServer();
                                AddChild(server);
                            }),
                            new SimpleUIAction("Create and Connect Client", () =>
                            {
                                TestClient client = new TestClient();
                                AddChild(client);
                                client.Connect();
                            })
                        ]
                        ));
                }
            }
        }
    }
}