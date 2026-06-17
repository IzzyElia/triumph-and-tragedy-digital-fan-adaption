using System.Globalization;
using Godot;

namespace TT2026.libraries.NetworkedBoardGameEntitySystem.SyncedDataTypes;

public class SyncedColor(GameEntity gameEntity, string variableKey, Color defaultValue)
    : EntityGameData(gameEntity, variableKey)
{
    private Color _defaultValue = defaultValue;
    public Color Value { get; set; } = defaultValue;
    public override string SerializeData()
    {
        return $"{Value.R.ToString(CultureInfo.InvariantCulture)}," +
               $"{Value.G.ToString(CultureInfo.InvariantCulture)}," +
               $"{Value.B.ToString(CultureInfo.InvariantCulture)}," +
               $"{Value.A.ToString(CultureInfo.InvariantCulture)}";
    }

    public override void DeserializeDataAndSetStateToIt(string data)
    {
        if (data is null)
        {
            Value = _defaultValue;
            return;
        }
        string[] split = data.Split(',');
        Value = new Color(
            float.Parse(split[0], CultureInfo.InvariantCulture), 
            float.Parse(split[1], CultureInfo.InvariantCulture), 
            float.Parse(split[2], CultureInfo.InvariantCulture),  
            float.Parse(split[3], CultureInfo.InvariantCulture));
    }
    
    public override bool ValidateData(string data)
    {
        if (data is null) return true;
        string[] split = data.Split(',');
        if (split.Length != 4) return false;
        for (int i = 0; i < split.Length; i++) if (!float.TryParse(split[i], CultureInfo.InvariantCulture, out _)) return false;
        return true;
    }
}