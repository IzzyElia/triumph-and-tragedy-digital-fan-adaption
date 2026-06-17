using System;
using System.Collections.Generic;
using TT2026.libraries.NetworkedBoardGameEntitySystem.Networking;
using TT2026.libraries.NetworkedBoardGameEntitySystem.SyncedDataTypes;

namespace TT2026.libraries.NetworkedBoardGameEntitySystem;

public abstract class GameEntity
{
    public int ID { get; private set; }
    public GameState GameState { get; private set; }
    public Dictionary<string, EntityGameData> __SyncedData = new();
    public SyncedBool Exists;

    public GameEntity()
    {
        Exists = new SyncedBool(this, "Exists", false);
    }

    public void CommitState()
    {
        foreach (var syncedData in __SyncedData.Values)
        {
            syncedData.CommitState(GameState.GameStepID);
        }
    }

    public EntityUpdatePacketApplyError TryApplyUpdatePacket(EntityVariableUpdatePacket updatePacket)
    {
        if (updatePacket.VariableName is null) return EntityUpdatePacketApplyError.AllGroovy;
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
        try
        {
            gameState.EntitiesByType.Add_CertainOfKey(this.GetType(), this);
        }
        catch (KeyNotFoundException e)
        {
            throw new InvalidOperationException($"Tried to create a {this.GetType()} but the type was not found in {nameof(gameState.EntitiesByType)}. Was {nameof(NetworkedBoardGameEntitySystem.GameState.LoadTypesFromCurrentAssembly)} called before the GameState was created?");
        }

        if (gameState.IsServerSide)
        {
             Exists.ForceSetValue("true", GameState.GameStepID);
        }
    }

    public int GetStateHash()
    {
        unchecked
        {
            int hash = 31;
            foreach (var data in __SyncedData.Values)
            {
                hash += data.GetStateHash();
            }
            return hash;
        }

    }

    public void ForceSetValue(string variableName, string variableValue, int gameStepId)
    {
        __SyncedData[variableName].ForceSetValue(variableValue, gameStepId);
    }
}