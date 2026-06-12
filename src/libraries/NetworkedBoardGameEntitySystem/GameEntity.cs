using System;
using System.Collections.Generic;
using Godot;

namespace TT2026.NetworkedBoardGameEntitySystem;

public abstract class GameEntity
{
    public int ID { get; private set; }
    public GameState GameState { get; private set; }
    public Dictionary<string, EntityGameData> __SyncedData = new();

    public void CommitState()
    {
        foreach (var syncedData in __SyncedData.Values)
        {
            syncedData.CommitState(GameState.GameStepID);
        }
    }

    public EntityUpdatePacketApplyError TryApplyUpdatePacket(EntityVariableUpdatePacket updatePacket)
    {
        if (!__SyncedData.TryGetValue(updatePacket.VariableName, out var syncedData))
        {
            return EntityUpdatePacketApplyError.InvalidVariableName;
        }
        return syncedData.TryApplyUpdatePacket(updatePacket);
    }

    public void SetStateToStep(int gameStepId)
    {
        foreach (var syncedData in __SyncedData.Values)
            syncedData.SetStateToStep(gameStepId);
    }

    public void Setup(GameState gameState, int id)
    {
        GameState = gameState;
        ID = id;
        gameState.EntitiesById.Add(ID, this);
    }
}