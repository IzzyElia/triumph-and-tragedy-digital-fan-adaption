using Godot;
using TT2026.libraries.IzzysUI.Tooltips;
using TT2026.libraries.NetworkedBoardGameEntitySystem;
using TT2026.libraries.NetworkedBoardGameEntitySystem.SyncedDataTypes;

namespace TT2026.Game.Entities;

public class BoardSpace : GameEntity
{
    public SyncedString Name;
    public SyncedColor Color;
    public SyncedInt ControllerId;
    public SyncedInt NationId;
    public SyncedInt TerrainType;
    
    public SyncedIntArray WaterTileOverrides;
    
    
    public Nation Nation => GameState.GetEntity<Nation>(NationId.Value);

    public BoardSpace() : base()
    {
        RandomNumberGenerator random = new();
        Name = new (this, nameof(Name));
        Color = new SyncedColor(this, nameof(Color), Colors.DeepPink);
        Color.Value = new Color(random.Randf(), random.Randf(), random.Randf());
        ControllerId = new SyncedInt(this, nameof(ControllerId), defaultValue: -1);
        NationId = new SyncedInt(this, nameof(NationId), defaultValue: -1);
        TerrainType = new SyncedInt(this, nameof(TerrainType), defaultValue: 0);
        
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