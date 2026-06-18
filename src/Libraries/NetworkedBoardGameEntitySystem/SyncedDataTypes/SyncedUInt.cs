using System.Globalization;

namespace TT2026.libraries.NetworkedBoardGameEntitySystem.SyncedDataTypes;

public class SyncedUInt(GameEntity gameEntity, string variableKey, uint defaultValue)
    : EntityGameData(gameEntity, variableKey)
{
    private uint _defaultValue = defaultValue;
    public uint Value { get; set; } = defaultValue;
    public override string SerializeData()
    {
        return Value.ToString(CultureInfo.InvariantCulture);
    }

    public override void DeserializeDataAndSetStateToIt(string data)
    {
        if (data is null) Value = _defaultValue;
        else Value = uint.Parse(data,  CultureInfo.InvariantCulture);
    }
    
    public override bool ValidateData(string data)
    {
        if (data is null) return true; 
        else return uint.TryParse(data, CultureInfo.InvariantCulture, out _);
    }
}