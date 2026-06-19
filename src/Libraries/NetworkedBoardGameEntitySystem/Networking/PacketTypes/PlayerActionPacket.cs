using System;
using System.Text.Json;
using TT2026.libraries.NetworkedBoardGameEntitySystem;
using TT2026.Libraries.NetworkedBoardGameEntitySystem.Actions;

namespace TT2026.Libraries.NetworkedBoardGameEntitySystem.Networking.PacketTypes;

public struct PlayerActionPacket (IPlayerAction action) : INetworkRequest
{
    public string PlayerActionTypeName { get; set; } = action.GetType().FullName;
    public string SerializedPlayerAction { get; set; } = JsonSerializer.Serialize(action, action.GetType());

    public IPlayerAction Deserialize()
    {
        if (!GameState.KnownPlayerActionTypes.TryGetValue(PlayerActionTypeName, out var type))
            throw new InvalidOperationException($"Player action referenced an invalid type {PlayerActionTypeName}");
        var playerAction = JsonSerializer.Deserialize(SerializedPlayerAction, type) as IPlayerAction;
        return playerAction;
    }
}