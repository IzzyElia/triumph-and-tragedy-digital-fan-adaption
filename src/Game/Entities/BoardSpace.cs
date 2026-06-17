using Godot;
using TT2026.libraries.NetworkedBoardGameEntitySystem;
using TT2026.libraries.NetworkedBoardGameEntitySystem.SyncedDataTypes;

namespace TT2026.Game.Entities;

public class BoardSpace : GameEntity
{
    public SyncedString Name;
    public SyncedColor Color;
    public SyncedInt ControllerId;

    public BoardSpace() : base()
    {
        RandomNumberGenerator random = new();
        Name = new (this, nameof(Name));
        Color = new SyncedColor(this, nameof(Color), Colors.DeepPink);
        Color.Value = new Color(random.Randf(), random.Randf(), random.Randf());
        ControllerId = new SyncedInt(this, nameof(ControllerId), defaultValue: -1);
    }
}