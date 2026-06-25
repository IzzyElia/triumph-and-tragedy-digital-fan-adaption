using System;
using System.Collections.Generic;
using Godot;
using TT2026.libraries.Izzy;
using TT2026.libraries.NetworkedBoardGameEntitySystem.Networking;

namespace TT2026.libraries.NetworkedBoardGameEntitySystem.Rendering;

public abstract partial class GameRenderer : Node3D
{
    public Client Client { get; set; }
    public ClientGameState GameState => Client.GameState;
    /// <summary>
    /// Tracks entities who's state has changed since the renderer last handled them.
    /// Special values include -2 (game step change)
    /// </summary>
    public HashSet<int> EntitiesChanged = new();

    /// <summary>
    /// Called once, and tells the renderer to setup its internal state
    /// </summary>
    public abstract void Initialize();
    /// <summary>
    /// Tells the renderer it should refresh every object. Called only when there has been a
    /// potentially drastic change to the state like a resync.
    /// When this method is called, the existence of previously known entities is not
    /// guaranteed, and so the full state of the board should be rebuilt
    /// </summary>
    public abstract void FullRefresh();
}