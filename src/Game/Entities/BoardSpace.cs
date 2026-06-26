using Godot;
using TT2026.Game.Definitions;
using TT2026.libraries.IzzysUI.Tooltips;
using TT2026.libraries.NetworkedBoardGameEntitySystem;
using TT2026.Libraries.NetworkedBoardGameEntitySystem;
using TT2026.libraries.NetworkedBoardGameEntitySystem.SyncedDataTypes;

namespace TT2026.Game.Entities;

public class BoardSpace : GameEntity
{
    public SyncedString Name;
    public SyncedColor Color;
    [EntityIdRef] public SyncedInt OccupierId;
    [EntityIdRef] public SyncedInt NationId;
    public SyncedInt TerrainType;
    [EntityIdRef] public SyncedInt CityTypeId;
    public SyncedInt Resources;
    public SyncedInt CityTilePosition;
    public SyncedInt ResourcesTilePosition;
    
    [EntityIdRef] public SyncedIntArray WaterTileOverrides;
    
    
    public Nation OwnerNation => GameState.GetEntity<Nation>(NationId.Value);
    public Nation OccupierNation => OccupierId.Value == -1 ? OwnerNation?.Occupier : GameState.GetEntity<Nation>(OccupierId.Value);
    public CityType CityType => GameState.GetEntity<CityType>(CityTypeId.Value);
    public bool IsLand() => TerrainType.Value == (int)Entities.TerrainType.Land || TerrainType.Value == (int)Entities.TerrainType.Strait;

    public BoardSpace() : base()
    {
        RandomNumberGenerator random = new();
        Name = new (this, nameof(Name));
        Color = new SyncedColor(this, nameof(Color), Colors.DeepPink);
        Color.Value = new Color(random.Randf(), random.Randf(), random.Randf());
        OccupierId = new SyncedInt(this, nameof(OccupierId), defaultValue: -1);
        NationId = new SyncedInt(this, nameof(NationId), defaultValue: -1);
        CityTypeId = new SyncedInt(this, nameof(CityTypeId), defaultValue: -1);
        TerrainType = new SyncedInt(this, nameof(TerrainType), defaultValue: 0);
        Resources = new SyncedInt(this, nameof(Resources), defaultValue: 0);
        CityTilePosition = new SyncedInt(this, nameof(CityTilePosition), defaultValue: -1);
        ResourcesTilePosition = new SyncedInt(this, nameof(ResourcesTilePosition), defaultValue: -1);
        
        WaterTileOverrides = new SyncedIntArray(this, nameof(WaterTileOverrides));
    }
    
    public TerrainType GetTerrainType() => (TerrainType)TerrainType.Value;
}

public enum TerrainType
{
    Land,
    Water,
    Strait,
    Impassable,
}