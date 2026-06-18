using System.Globalization;
using System.Linq;
using System.Text;
using TT2026.libraries.Izzy;
using TT2026.libraries.Izzy.Geometry;

namespace TT2026.libraries.NetworkedBoardGameEntitySystem.SyncedDataTypes;

public class SyncedTileReferenceCollection : EntityGameData
{
    public SyncedTileReferenceCollection(GameEntity gameEntity, string variableKey) : base(gameEntity, variableKey)
    {
        
    }

    public AssociationCollection<int, ICoordinate2d> Dict = new();


    private StringBuilder sb = new();
    public override string SerializeData()
    {
        lock (sb)
        {
            sb.Clear();
            foreach (var key in Dict.Keys)
            {
                if (!Dict.GetValuesOfKey(key).Any()) sb.Append($"{key.ToString(CultureInfo.InvariantCulture)}&");
                foreach (var value in Dict.GetValuesOfKey(key))
                {
                    sb.Append($"{key.ToString(CultureInfo.InvariantCulture)}:{value.x.ToString(CultureInfo.InvariantCulture)},{value.y.ToString(CultureInfo.InvariantCulture)};");
                }
            }
            return sb.ToString();
        }
    }
    
    public override void DeserializeDataAndSetStateToIt(string data)
    {
        lock (sb)
        {
            Dict.Clear();
            if (data is null) return;
            int pointer = 0;
            sb.Clear();
            while (pointer < data.Length)
            {
                char character = data[pointer];
                if (character == ';')
                {
                    string[] split = sb.ToString().Split(':');
                    int key = int.Parse(split[0], CultureInfo.InvariantCulture);
                    string[] value = split[1].Split(',');
                    int x = int.Parse(value[0], CultureInfo.InvariantCulture);
                    int y = int.Parse(value[1], CultureInfo.InvariantCulture);
                    Dict.Set(key, new GenericCoordinate2d(x, y));
                    sb.Clear();
                }
                else if (character == '&')
                {
                    int key = int.Parse(sb.ToString(), CultureInfo.InvariantCulture);
                    Dict.EnsureKey(key);
                    sb.Clear();
                }
                else
                {
                    sb.Append(character);
                }

                pointer++;
            }
        }
    }

    public override bool ValidateData(string data)
    {
        lock (sb)
        {
            int pointer = 0;
            sb.Clear();
            if (data is null) return true;
            if (data.Length == 0) return true;
            if (!(data.EndsWith(';') || data.EndsWith('&'))) return false;
            while (pointer < data.Length)
            {
                char character = data[pointer];
                if (character == ';')
                {
                    string[] split = sb.ToString().Split(':');
                    if (split.Length != 2) return false;
                    if (!int.TryParse(split[0], CultureInfo.InvariantCulture, out _)) return false;
                    string[] value =  split[1].Split(',');
                    if (value.Length != 2) return false;
                    if (!int.TryParse(value[0], CultureInfo.InvariantCulture, out _)) return false;
                    if (!int.TryParse(value[1], CultureInfo.InvariantCulture, out _)) return false;
                    sb.Clear();
                }
                else if (character == '&')
                {
                    if (!int.TryParse(sb.ToString(), CultureInfo.InvariantCulture, out _)) return false;
                    sb.Clear();
                }
                else
                {
                    sb.Append(character);
                }

                pointer++;
            }
            return true;
        }
    }
}