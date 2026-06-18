using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace TT2026.libraries.NetworkedBoardGameEntitySystem.SyncedDataTypes;

public class SyncedIntArray(GameEntity gameEntity, string variableKey)
    : EntityGameData(gameEntity, variableKey)
{
    public int[] Value = System.Array.Empty<int>();

    public override string SerializeData()
    {
        if (Value is null) return "";
        if (Value.Length == 0) return "";
        StringBuilder sb = new StringBuilder();
        foreach (var value in Value)
        {
            sb.Append(value.ToString(CultureInfo.InvariantCulture));
            sb.Append(',');
        }
        sb.Remove(sb.Length - 1, 1);
        return sb.ToString();
    }

    public override void DeserializeDataAndSetStateToIt(string data)
    {
        if (data is null || data.Length == 0)
        {
            Value = Array.Empty<int>();
            return;
        }
        string[] split = data.Split(',');

        Value = new int[split.Length];
        for (int i = 0; i < split.Length; i++)
        {
            Value[i] = int.Parse(split[i], CultureInfo.InvariantCulture);
        }
    }

    public override bool ValidateData(string data)
    {
        if (data is null || data.Length == 0) return true;
        return data.Split(',').All(s => int.TryParse(s, CultureInfo.InvariantCulture, out _));
    }
}
