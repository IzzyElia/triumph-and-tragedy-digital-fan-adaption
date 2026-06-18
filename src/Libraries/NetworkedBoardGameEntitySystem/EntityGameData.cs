using System;
using System.Collections.Generic;
using System.Linq;
using TT2026.libraries.NetworkedBoardGameEntitySystem.Networking;

namespace TT2026.libraries.NetworkedBoardGameEntitySystem;

public abstract class EntityGameData
{
    private GameEntity _entity;
    public string Key { get; private set; }

    private string _currentSerializedState;
    private int _showingAtStateId = -1;
    public List<DataHistoryFrame> __HistoryFrames = new ();
    
    protected EntityGameData(GameEntity gameEntity, string variableKey)
    {
        if (gameEntity is not null)
        {
            _entity = gameEntity;
            _entity.__SyncedData.Add(variableKey, this);
        }
        Key = variableKey;
    }

    private void NotifyEntityStateChanged()
    {
        lock (_entity.GameState.NetworkManager.Mutex)
        {
            _entity.GameState.Renderer?.EntitiesChanged.Add(_entity.ID);
        }
    }

    public void CommitState(int gameStepId)
    {
        lock (_entity.GameState.NetworkManager.Mutex)
        {
            if (!_entity.GameState.IsServerSide)
                throw new InvalidOperationException(
                    "Tried to call CommitState() from the client (CommitState() is for confirming changes on the server)");
            string currentSerializedState = SerializeData();
            var frame = new DataHistoryFrame(gameStepId, currentSerializedState);
            if (__HistoryFrames.Count == 0)
            {
                __HistoryFrames.Add(frame);
                NotifyEntityStateChanged();
                ((ServerGameState)_entity.GameState).Server.PushUpdate(
                    new EntityVariableUpdatePacket(gameStepId, _entity, Key, currentSerializedState));
                return;
            }

            if (__HistoryFrames.Last().Data != currentSerializedState)
            {
                if (gameStepId > __HistoryFrames[^1].GameStepID)
                {
                    __HistoryFrames.Add(frame);
                    NotifyEntityStateChanged();
                    ((ServerGameState)_entity.GameState).Server.PushUpdate(
                        new EntityVariableUpdatePacket(gameStepId, _entity, Key, currentSerializedState));
                }
                else if (gameStepId == __HistoryFrames[^1].GameStepID)
                {
                    __HistoryFrames[^1] = frame;
                }
                else
                    throw
                        new InvalidOperationException(
                            "Tried to commit an entity data value to a step *before* the last time it was committed"); 
            }
        }
    }
    
    public void ForceSetValue(string serializedValue, int gameStepId, bool push = true)
    {
        if (!_entity.GameState.IsServerSide) throw new InvalidOperationException($"ForceSetValue should be called on the server only");
        lock (_entity.GameState.NetworkManager.Mutex)
        {
            if (!ValidateData(serializedValue)) throw new ArgumentException($"Invalid value '{serializedValue}' for entity data type {this.GetType().Name}");

            var frame = new DataHistoryFrame(gameStepId, serializedValue);

            for (int i = 0; i < __HistoryFrames.Count; i++)
            {
                if (__HistoryFrames[i].GameStepID == gameStepId)
                {
                    // Same step already recorded — overwrite it.
                    __HistoryFrames[i] = frame;
                    RefreshState();
                    NotifyEntityStateChanged();
                    if (push) ((ServerGameState)_entity.GameState).Server.PushUpdate(
                        new EntityVariableUpdatePacket(gameStepId, _entity, Key, serializedValue));
                    return;
                }
                else if (__HistoryFrames[i].GameStepID > gameStepId)
                {
                    // First frame that belongs after the new one — insert before it.
                    __HistoryFrames.Insert(i, frame);
                    RefreshState();
                    NotifyEntityStateChanged();
                    if (push) ((ServerGameState)_entity.GameState).Server.PushUpdate(
                        new EntityVariableUpdatePacket(gameStepId, _entity, Key, serializedValue));
                    return;
                }
            }

            // New step is greater than everything (or the list was empty) — append.
            __HistoryFrames.Add(frame);
            RefreshState();
            NotifyEntityStateChanged();
            if (push) ((ServerGameState)_entity.GameState).Server.PushUpdate(
                new EntityVariableUpdatePacket(gameStepId, _entity, Key, serializedValue));
            return;
        }
    }

    public EntityUpdatePacketApplyError TryApplyUpdatePacket(EntityVariableUpdatePacket updatePacket)
    {
        lock (_entity.GameState.NetworkManager.Mutex)
        {
            if (!ValidateData(updatePacket.VariableValue)) return EntityUpdatePacketApplyError.InvalidVariableValue;
            
            var frame = new DataHistoryFrame(updatePacket.GameStepId, updatePacket.VariableValue);
            Logger.Log($"Applying update to entity #{updatePacket.EntityId} ({_entity.GetType().Name}): {updatePacket.VariableName} = {updatePacket.VariableValue} at step {updatePacket.GameStepId}");
            for (int i = 0; i < __HistoryFrames.Count; i++)
            {
                if (__HistoryFrames[i].GameStepID == updatePacket.GameStepId)
                {
                    // Same step already recorded — overwrite it.
                    __HistoryFrames[i] = frame;
                    RefreshState();
                    return EntityUpdatePacketApplyError.AllGroovy;
                }
                else if (__HistoryFrames[i].GameStepID > updatePacket.GameStepId)
                {
                    // First frame that belongs after the new one — insert before it.
                    __HistoryFrames.Insert(i, frame);
                    RefreshState();
                    return EntityUpdatePacketApplyError.AllGroovy;
                }
            }

            // New step is greater than everything (or the list was empty) — append.
            __HistoryFrames.Add(frame);
            RefreshState();
            return EntityUpdatePacketApplyError.AllGroovy;
        }
    }
    /// <summary>
    /// Sets the data to its state at a certain game step. -1 will show the most up to date data while any other value will lock the value to its state at a certain step
    /// </summary>
    public void SetStateToStep(int gameStepId)
    {
        _showingAtStateId = gameStepId;
        RefreshState();
    }

    private void RefreshState()
    {
        lock (_entity.GameState.NetworkManager.Mutex)
        {
            int gameStepId = _showingAtStateId == -1 ? _entity.GameState.GameStepID : _showingAtStateId;
            string frameData;
            // If the first frame is beyond the one we are searching for, the data doesn't exist yet
            if (__HistoryFrames.Count == 0 || __HistoryFrames[0].GameStepID > gameStepId)
            {
                DeserializeDataAndSetStateToIt(null);
                if (_currentSerializedState != null) NotifyEntityStateChanged();
                _currentSerializedState = null;
                return;
            }

            for (int i = 0; i < __HistoryFrames.Count; i++)
            {
                if (__HistoryFrames[i].GameStepID == gameStepId)
                {
                    frameData = __HistoryFrames[i].Data;
                    
                    DeserializeDataAndSetStateToIt(frameData); 
                    if (frameData != _currentSerializedState) NotifyEntityStateChanged();
                    _currentSerializedState = frameData;
                    return;
                }
                else if (__HistoryFrames[i].GameStepID > gameStepId)
                {
                    frameData = __HistoryFrames[i-1].Data;
                    DeserializeDataAndSetStateToIt(frameData);
                    if (frameData != _currentSerializedState) NotifyEntityStateChanged();
                    _currentSerializedState = frameData;
                    return;
                }
            }

            frameData = __HistoryFrames[^1].Data;
            DeserializeDataAndSetStateToIt(frameData);
            if (frameData != _currentSerializedState) NotifyEntityStateChanged();
            _currentSerializedState = frameData;
        }
    }

    public IEnumerable<EntityVariableUpdatePacket> GenerateSyncPacketsForEntireHistory()
    {
        lock (_entity.GameState.NetworkManager.Mutex)
        {
            foreach (var historyFrame in __HistoryFrames)
            {
                yield return new EntityVariableUpdatePacket(
                    gameStepId: historyFrame.GameStepID,
                    variableName: Key,
                    entity: _entity,
                    variableValue: historyFrame.Data
                );
            }
        }
    }
    
    /// <returns>The data stored by this object as a string</returns>
    public abstract string SerializeData();
    /// <returns>Restores this objects data from a string, which may be null</returns>
    public abstract void DeserializeDataAndSetStateToIt(string data);
    /// <returns>Returns whether the data is valid for deserialization, then discards it</returns>
    public abstract bool ValidateData(string data);

    public int GetStateHash()
    {
        unchecked
        {
            int hash = 0;
            foreach (var frame in __HistoryFrames)
            {
                hash += Utils.Mix32Hash(frame.GameStepID) * Utils.Fnv1AHash(frame.Data) * 17;
            }
            return hash;
        }
    }
}

public struct DataHistoryFrame(int gameStepId, string data)
{
    public int GameStepID { get; set; } = gameStepId;
    public string Data { get; set; } = data;
}