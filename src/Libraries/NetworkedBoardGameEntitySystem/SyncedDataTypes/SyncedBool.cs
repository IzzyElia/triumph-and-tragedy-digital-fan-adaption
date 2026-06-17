using System.Globalization;

namespace TT2026.libraries.NetworkedBoardGameEntitySystem.SyncedDataTypes;

public class SyncedBool(GameEntity gameEntity, string variableKey, bool defaultValue)
    : EntityGameData(gameEntity, variableKey)
{
    private bool _defaultValue = defaultValue;
    public bool Value { get; set; } = defaultValue;
    public override string SerializeData()
    {
        return Value.ToString(CultureInfo.InvariantCulture);
    }

    public override void DeserializeDataAndSetStateToIt(string data)
    {
        if (data is null) Value = _defaultValue;
        else Value = bool.Parse(data);
    }
    
    public override bool ValidateData(string data)
    {
        if (data is null) return true; 
        else return bool.TryParse(data, out _);
    }
}