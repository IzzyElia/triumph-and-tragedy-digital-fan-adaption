using Godot;
using TT2026.Game.Behaviors;
using TT2026.Libraries.NetworkedBoardGameEntitySystem;
using TT2026.libraries.NetworkedBoardGameEntitySystem.Networking;
using TT2026.libraries.NetworkedBoardGameEntitySystem.Rendering;

namespace TT2026.Game;

public static class Factory
{
    public static Server CreateServer(int port)
    {
        Server server = new Server(port);
        server.GameState.SetEnabledGameBehaviors(typeof(TileOwnershipBehavior));
        return server;
    }

    public static Client CreateClientAndConnectLocally(int port, string password, PackedScene rendererPrefab)
    {
        GameRenderer renderer = rendererPrefab.Instantiate<GameRenderer>();
        Client client = new Client(renderer);
        client.Connect("localhost", port, password: password);
        return client;
    }
    
    public static Client CreateStandaloneClient(PackedScene rendererPrefab)
    {
        GameRenderer renderer = rendererPrefab.Instantiate<GameRenderer>();
        Client client = new Client(renderer, forceCreateGameState: true);
        return client;
    }
}