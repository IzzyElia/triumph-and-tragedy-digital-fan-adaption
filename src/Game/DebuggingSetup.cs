using System;
using Godot;
using TT2026.Game.Rendering;
using TT2026.libraries.IzzysConsole.API;
using TT2026.libraries.IzzysConsole.Internal;
using TT2026.libraries.NetworkedBoardGameEntitySystem;
using TT2026.libraries.NetworkedBoardGameEntitySystem.Networking;

namespace TT2026.Game;

public partial class DebuggingSetup : Node
{
    [Export] private int _numClients;
    [Export] private int _port;
    
    private Client _client;

    public override void _Ready()
    {
        CommandRegistry.Initialize();
        GameState.LoadTypesFromCurrentAssembly();
        
        if (_numClients == 0 || _port == 0)
        {
            throw new ArgumentException($"You might have forgotten to setup {nameof(DebuggingSetup)} in the inspector (some values at defaults)");
        }
        var server = Factory.CreateServer(_port);
        for (int i = 0; i < _numClients; i++)
        {
            _client = Factory.CreateClientAndConnectLocally(_port, null);
            _client.SetRenderer(Factory.EditorRenderer, this.GetParent());
            _client.Renderer.Name = $"Client {i} Renderer";
        }
    }

    public override void _UnhandledKeyInput(InputEvent @event)
    {
        base._UnhandledKeyInput(@event);
        if (@event is InputEventKey key)
        {
            if (key.Pressed && key.Keycode == Key.Key0)
            {
                _client.SetRenderer(Factory.EditorRenderer, GetParent());
            }

            if (key.Pressed && key.Keycode == Key.Key9)
            {
                _client.SetRenderer(Factory.PlayerRenderer, GetParent());
            }
        }
    }
}