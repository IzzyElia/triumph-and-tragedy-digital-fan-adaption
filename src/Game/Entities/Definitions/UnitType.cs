using TT2026.libraries.NetworkedBoardGameEntitySystem;
using TT2026.libraries.NetworkedBoardGameEntitySystem.SyncedDataTypes;

namespace TT2026.Game.Definitions;

public class UnitType : GameEntity
{
    public SyncedObject<UnitTypeDefinition> Definition;

    public UnitType()
    {
        Definition = new SyncedObject<UnitTypeDefinition>(this, nameof(Definition), default);
    }
}

public struct UnitTypeDefinition : ISyncable
{
    public string Name { get; set; }
    public string PluralName { get; set; }
    public int UnitClassId { get; set; }
    public int AirAttack { get; set; }
    public int GroundAttack { get; set; }
    public int SeaAttack { get; set; }
    public int SubAttack { get; set; }
    public int BaseMovement { get; set; }
    public bool MayBePlaced { get; set; }
}

public enum UnitClass
{
    Undefined,
    Air,
    Ground,
    Sea,
    Submarine,
}