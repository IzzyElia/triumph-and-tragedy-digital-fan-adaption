using System;
using System.Collections.Generic;
using System.Linq;

namespace TT2026.NetworkedBoardGameEntitySystem;

public abstract class EntityGameData
{
    private GameEntity _entity;
    private string _variableKey;
    private int _showingAtStateId = -1;
    private List<DataHistoryFrame> _historyFrames = new ();
    
    protected EntityGameData(GameEntity gameEntity, string variableKey)
    {
        _entity = gameEntity;
        _entity.__SyncedData.Add(variableKey, this);
        _variableKey = variableKey;
    }
    
    public void CommitState(int gameStepId)
    {
        lock (_entity.GameState.Mutex)
        {
            if (!_entity.GameState.IsServerSide)
                throw new InvalidOperationException(
                    "Tried to call CommitState() from the client (CommitState() is for confirming changes on the server)");
            string currentSerializedState = SerializeData();
            var frame = new DataHistoryFrame(gameStepId, currentSerializedState);
            if (_historyFrames.Count == 0)
            {
                _historyFrames.Add(frame);
                ((ServerGameState)_entity.GameState).NetworkInterface.PushVariableChange(
                    new EntityVariableUpdatePacket(gameStepId, _entity, _variableKey, currentSerializedState));
                return;
            }

            if (_historyFrames.Last().Data != currentSerializedState)
            {
                if (gameStepId > _historyFrames[^1].GameStepID)
                {
                    _historyFrames.Add(frame);
                    ((ServerGameState)_entity.GameState).NetworkInterface.PushVariableChange(
                        new EntityVariableUpdatePacket(gameStepId, _entity, _variableKey, currentSerializedState));
                }
                else
                    throw // The data was already committed for this step. It shouldn't ever be recommitted until the game advances a step
                        new InvalidOperationException(
                            "Tried to recommit committed data without advancing the Game Step ID"); //_historyFrames[^1] = new DataHistoryFrame(gameStepId, currentSerializedState);
            }
        }
    }

    public EntityUpdatePacketApplyError TryApplyUpdatePacket(EntityVariableUpdatePacket updatePacket)
    {
        lock (_entity.GameState.Mutex)
        {
            if (!ValidateData(updatePacket.VariableValue)) return EntityUpdatePacketApplyError.InvalidVariableValue;

            var frame = new DataHistoryFrame(updatePacket.GameStepId, updatePacket.VariableValue);

            for (int i = 0; i < _historyFrames.Count; i++)
            {
                if (_historyFrames[i].GameStepID == updatePacket.GameStepId)
                {
                    // Same step already recorded — overwrite it.
                    _historyFrames[i] = frame;
                    RefreshState();
                    return EntityUpdatePacketApplyError.AllGroovy;
                }
                else if (_historyFrames[i].GameStepID > updatePacket.GameStepId)
                {
                    // First frame that belongs after the new one — insert before it.
                    _historyFrames.Insert(i, frame);
                    RefreshState();
                    return EntityUpdatePacketApplyError.AllGroovy;
                }
            }

            // New step is greater than everything (or the list was empty) — append.
            _historyFrames.Add(frame);
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
        lock (_entity.GameState.Mutex)
        {
            int gameStepId = _showingAtStateId == -1 ? _entity.GameState.GameStepID : _showingAtStateId;

            // If the first frame is beyond the one we are searching for, the data doesn't exist yet
            if (_historyFrames.Count == 0 || _historyFrames[0].GameStepID > gameStepId)
            {
                DeserializeDataAndSetStateToIt(null);
                return;
            }

            for (int i = 0; i < _historyFrames.Count; i++)
            {
                if (_historyFrames[i].GameStepID == gameStepId)
                {
                    DeserializeDataAndSetStateToIt(_historyFrames[i].Data);
                    return;
                }
                else if (_historyFrames[i].GameStepID > gameStepId)
                {
                    DeserializeDataAndSetStateToIt(_historyFrames[i - 1].Data);
                    return;
                }
            }

            DeserializeDataAndSetStateToIt(_historyFrames[^1].Data);
        }
    }

    public IEnumerable<EntityVariableUpdatePacket> GenerateSyncPacketsForEntireHistory()
    {
        lock (_entity.GameState.Mutex)
        {
            foreach (var historyFrame in _historyFrames)
            {
                yield return new EntityVariableUpdatePacket(
                    gameStepID: historyFrame.GameStepID,
                    variableName: _variableKey,
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
}

public struct DataHistoryFrame(int gameStepId, string data)
{
    public int GameStepID { get; set; } = gameStepId;
    public string Data { get; set; } = data;
}