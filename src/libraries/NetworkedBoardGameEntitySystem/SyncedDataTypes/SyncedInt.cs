namespace TT2026.NetworkedBoardGameEntitySystem.SyncedDataTypes;

public class SyncedInt(GameEntity gameEntity, string variableKey, int defaultValue, int value)
    : EntityGameData(gameEntity, variableKey)
{
    public int Value { get; set; } = value;
    public override string SerializeData()
    {
        return Value.ToString();
    }

    public override void DeserializeDataAndSetStateToIt(string data)
    {
        if (data is null) Value = defaultValue;
        else Value = int.Parse(data);
    }
    
    public override bool ValidateData(string data)
    {
        if (data is null) return true; 
        else return int.TryParse(data, out _);
    }
}