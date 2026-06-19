using System;
using Godot;
using TT2026.Game.Behaviors;
using TT2026.Libraries.NetworkedBoardGameEntitySystem;
using TT2026.libraries.NetworkedBoardGameEntitySystem.Networking;
using TT2026.libraries.NetworkedBoardGameEntitySystem.Rendering;

namespace TT2026.Game;

public static class Factory
{
    [Export] private const string _editorRendererUid = "uid://divfuioy1iisg";
    [Export] private const string _playerRendererUid = "uid://4ws7mfuqpxd0";
    public static PackedScene EditorRenderer;
    public static PackedScene PlayerRenderer;

    static Factory()
    {
        EditorRenderer = ResourceLoader.Load<PackedScene>(_editorRendererUid);
        if (EditorRenderer is null) Logger.Error($"{nameof(EditorRenderer)} not found");
        PlayerRenderer = ResourceLoader.Load<PackedScene>(_playerRendererUid);
        if (PlayerRenderer is null) Logger.Error($"{nameof(PlayerRenderer)} not found");
    }
    public static Server CreateServer(int port)
    {
        Server server = new Server(port);
        server.GameState.SetEnabledGameBehaviors(typeof(TileOwnershipBehavior));
        return server;
    }

    public static Client CreateClientAndConnectLocally(int port, string password)
    {
        Client client = new Client();
        client.Connect("localhost", port, password: password);
        return client;
    }
    
    public static Client CreateStandaloneClient()
    {
        Client client = new Client(forceCreateGameState: true);
        return client;
    }
}