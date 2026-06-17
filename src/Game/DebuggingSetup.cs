using System;
using Godot;
using TT2026.Game.Rendering;
using TT2026.libraries.NetworkedBoardGameEntitySystem;

namespace TT2026.Game;

public partial class DebuggingSetup : Node
{
    [Export] private int _numClients;
    [Export] private int _port;
    [Export] private PackedScene _rendererPrefab;

    public override void _Ready()
    {
        GameState.LoadTypesFromCurrentAssembly();
        
        if (_numClients == 0 || _port == 0 || _rendererPrefab == null)
        {
            throw new ArgumentException($"You might have forgotten to setup {nameof(DebuggingSetup)} in the inspector (some values at defaults)");
        }
        var server = Factory.CreateServer(_port);
        for (int i = 0; i < _numClients; i++)
        {
            var client = Factory.CreateClientAndConnectLocally(_port, null, _rendererPrefab);
            client.Renderer.Name = $"Client {i} Renderer";
            this.GetParent().CallDeferred("add_child", client.Renderer);
            client.Renderer.Initialize();
            client.Renderer.FullRefresh();
        }
    }
}