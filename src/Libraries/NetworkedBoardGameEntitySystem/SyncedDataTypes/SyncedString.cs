using System;

namespace TT2026.libraries.NetworkedBoardGameEntitySystem.SyncedDataTypes;

public class SyncedString(GameEntity gameEntity, string variableKey, string defaultValue = null)
    : EntityGameData(gameEntity, variableKey)
{
    private string _defaultValue = defaultValue;
    public string Value { get; set; } = defaultValue;
    public override string SerializeData()
    {
        return Value;
    }

    public override void DeserializeDataAndSetStateToIt(string data)
    {
        if (data is null) Value = _defaultValue;
        else Value = data;
    }
    
    public override bool ValidateData(string data)
    {
        return true;
    }
}