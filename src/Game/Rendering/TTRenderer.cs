using Godot;
using TT2026.Game.Behaviors;
using TT2026.Game.Entities;
using TT2026.libraries.NetworkedBoardGameEntitySystem;
using TT2026.Libraries.NetworkedBoardGameEntitySystem;
using TT2026.libraries.NetworkedBoardGameEntitySystem.Rendering;

namespace TT2026.Game.Rendering;

public partial class TTRenderer : TilingGameRenderer
{

    public override void Initialize()
    {
        base.Initialize();
        
    }

    public override void FullRefresh()
    {
        base.FullRefresh();
    }

    public void TileRefresh()
    {
        Logger.Log($"Running tile refresh");
        var tileOwnership = GameState.GetGameBehavior<TileOwnershipBehavior>();
        foreach (var boardSpace in GameState.GetEntitiesOfType<BoardSpace>())
        {
            foreach (var tileId in tileOwnership.TileOwnership.Dict.GetValuesOfKey(boardSpace.ID))
            {
                MeshInstance3D tileRenderer = GetTile(tileId);
                ((StandardMaterial3D)tileRenderer.GetSurfaceOverrideMaterial(0)).AlbedoColor = boardSpace.Color.Value;
            }
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        bool needsFullRefresh = false;
        bool needsTileRefresh = false;
        
        foreach (var changedEntity in EntitiesChanged)
        {
            GameEntity entity = GameState.GetEntity(changedEntity);
            if (entity is null) // entity was deleted
            {
                needsFullRefresh = true;
                break;
            }

            if (entity is BoardSpace || entity is TileOwnershipBehavior)
            {
                needsTileRefresh = true;
            }
        }
        
        EntitiesChanged.Clear();
        
        if (needsFullRefresh) FullRefresh();
        if (needsTileRefresh) TileRefresh();
    }

    protected override void OnTileClicked(int x, int y)
    {
        
    }
}