using System.Globalization;

namespace TT2026.libraries.NetworkedBoardGameEntitySystem.SyncedDataTypes;

public class SyncedInt(GameEntity gameEntity, string variableKey, int defaultValue)
    : EntityGameData(gameEntity, variableKey)
{
    private int _defaultValue = defaultValue;
    public int Value { get; set; } = defaultValue;
    public override string SerializeData()
    {
        return Value.ToString(CultureInfo.InvariantCulture);
    }

    public override void DeserializeDataAndSetStateToIt(string data)
    {
        if (data is null) Value = _defaultValue;
        else Value = int.Parse(data,  CultureInfo.InvariantCulture);
    }
    
    public override bool ValidateData(string data)
    {
        if (data is null) return true; 
        else return int.TryParse(data, CultureInfo.InvariantCulture, out _);
    }
}