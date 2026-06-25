using System;
using System.Text.Json;

namespace TT2026.libraries.NetworkedBoardGameEntitySystem.SyncedDataTypes;

public interface ISyncedObject : IEntityGameData
{
    public object __Value { get; set; }
}

public class SyncedObject<T>(GameEntity gameEntity, string variableKey, T defaultValue)
    : EntityGameData(gameEntity, variableKey), ISyncedObject where T : ISyncable, new()
{
    private T _defaultValue = defaultValue;
    public T Value { get; set; } = defaultValue;
    public object __Value { get => Value; set => Value = (T)value; }

    public override string SerializeData()
    {
        return JsonSerializer.Serialize(Value);
    }

    public override void DeserializeDataAndSetStateToIt(string data)
    {
        if (data is null) Value = _defaultValue;
        else Value = JsonSerializer.Deserialize<T>(data);
    }
    
    public override bool ValidateData(string data)
    {
        if (data is null) return true;
        else
        {
            try
            {
                JsonSerializer.Deserialize<T>(data);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}