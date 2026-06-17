using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using TT2026.libraries.IzzysUI;
using TT2026.libraries.IzzysUI.Tooltips;
using TT2026.libraries.NetworkedBoardGameEntitySystem.Networking;

namespace TT2026.GlobalSingletons;

public partial class NetworkController : Node
{
    private static NetworkController _instance;
    private HashSet<NetworkPeer> _networkPeers = new HashSet<NetworkPeer>();

    public static void RegisterNetworkPeer(NetworkPeer networkPeer)
    {
        if (_instance is null) throw new InvalidOperationException($"No NetworkController instance created in scene");
        _instance._networkPeers.Add(networkPeer);
    }

    public static void UnregisterNetworkPeer(NetworkPeer networkPeer)
    {
        if (_instance is null) throw new InvalidOperationException($"No NetworkController instance created in scene");
        _instance._networkPeers.Remove(networkPeer);
    }
    public override void _EnterTree()
    {
        _instance = this;
    }

    public override void _ExitTree()
    {
        TT2026.Logger.Warn($"Undefined behavior for NetworkController exiting the scene tree");
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        foreach (var peer in _networkPeers)
        {
            peer.NetManager.PollEvents();
            peer.MonitorTimeouts();
        }
    }
}