using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace TT2026.libraries.NetworkedBoardGameEntitySystem.SyncedDataTypes;

public class SyncedIntList(GameEntity gameEntity, string variableKey)
    : EntityGameData(gameEntity, variableKey)
{
    public readonly List<int> List = new List<int>();

    public override string SerializeData()
    {
        if (List is null) return "";
        if (List.Count == 0) return "";
        StringBuilder sb = new StringBuilder();
        foreach (var value in List)
        {
            sb.Append(value.ToString(CultureInfo.InvariantCulture));
            sb.Append(',');
        }
        sb.Remove(sb.Length - 1, 1);
        return sb.ToString();
    }

    public override void DeserializeDataAndSetStateToIt(string data)
    {
        List.Clear();
        if (data is null || data.Length == 0) return;
        
        foreach (var s in data.Split(','))
        {
            List.Add(int.Parse(s, CultureInfo.InvariantCulture));
        }
    }

    public override bool ValidateData(string data)
    {
        if (data is null || data.Length == 0) return true;
        return data.Split(',').All(s => int.TryParse(s, CultureInfo.InvariantCulture, out _));
    }
}
