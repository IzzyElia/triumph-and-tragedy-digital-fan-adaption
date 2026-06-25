using System;
using System.Text.Json;
using TT2026.libraries.NetworkedBoardGameEntitySystem;
using TT2026.Libraries.NetworkedBoardGameEntitySystem.Actions;

namespace TT2026.Libraries.NetworkedBoardGameEntitySystem.Networking.PacketTypes;

public struct StartGamePacket(IGameStartInfo gameStartInfo) : INetworkRequest
{
    public string GameStartInfoTypeName { get; set; } = gameStartInfo.GetType().FullName;
    public string SerializedGameStartInfo { get; set; } = JsonSerializer.Serialize(gameStartInfo, gameStartInfo.GetType());
    
    public IGameStartInfo Deserialize()
    {
        if (!GameState.KnownGameStartInfoTypes.TryGetValue(GameStartInfoTypeName, out var type))
            throw new InvalidOperationException($"Game Start Info referenced an invalid type {GameStartInfoTypeName}");
        var gameStartInfo = JsonSerializer.Deserialize(SerializedGameStartInfo, type) as IGameStartInfo;
        return gameStartInfo;
    }
}