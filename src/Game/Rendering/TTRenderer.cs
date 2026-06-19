using System.Linq;
using Godot;
using TT2026.Game.Behaviors;
using TT2026.Game.Entities;
using TT2026.libraries.Izzy.Geometry;
using TT2026.libraries.NetworkedBoardGameEntitySystem;
using TT2026.Libraries.NetworkedBoardGameEntitySystem;
using TT2026.libraries.NetworkedBoardGameEntitySystem.Rendering;
using TT2026.Libraries.NetworkedBoardGameEntitySystem.Rendering;

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
        TileRefresh();
    }

    private TileInfo GetTileInfo(ICoordinate2d tileCoordinate, ref TileOwnershipBehavior tileOwnership)
    {
        int tileId = GetTileId(tileCoordinate);
        BoardSpace boardSpace = GameState.GetEntity<BoardSpace>(tileOwnership.GetOwnerOfTile(tileCoordinate));
        Nation nation = boardSpace is null ? null : boardSpace.Nation;
        return new TileInfo()
        {
            BoardSpace = boardSpace,
            Nation = nation
        };
    }
    
    public void TileRefresh()
    {
        Logger.Log($"Running tile refresh");
        var tileOwnership = GameState.GetGameBehavior<TileOwnershipBehavior>();
        if (tileOwnership is null) return;
        foreach (var boardSpace in GameState.GetEntitiesOfType<BoardSpace>())
        {
            foreach (ICoordinate2d tileCoordinate in tileOwnership.TileOwnership.Dict.GetValuesOfKey(boardSpace.ID))
            {
                int tileid = GetTileId(tileCoordinate);
                TileInfo tileInfo = GetTileInfo(tileCoordinate, ref tileOwnership);
                GenericCoordinate2d upCoordinate = new GenericCoordinate2d(tileCoordinate.x, tileCoordinate.y - 1);
                GenericCoordinate2d downCoordinate = new GenericCoordinate2d(tileCoordinate.x, tileCoordinate.y + 1);
                GenericCoordinate2d leftCoordinate = new GenericCoordinate2d(tileCoordinate.x - 1, tileCoordinate.y);
                GenericCoordinate2d rightCoordinate = new GenericCoordinate2d(tileCoordinate.x + 1, tileCoordinate.y);
                TileInfo up = GetTileInfo(upCoordinate, ref tileOwnership);
                TileInfo down = GetTileInfo(downCoordinate, ref tileOwnership);
                TileInfo left = GetTileInfo(leftCoordinate, ref tileOwnership);
                TileInfo right = GetTileInfo(rightCoordinate, ref tileOwnership);
                bool isOcean = (TerrainType)tileInfo.BoardSpace.TerrainType.Value == TerrainType.Water;
                if (tileInfo.BoardSpace.WaterTileOverrides.Value.Contains(tileid)) isOcean = !isOcean;
                bool upIsInnerCoastline = up.BoardSpace == boardSpace && (boardSpace.WaterTileOverrides.Value.Contains(tileid) != 
                                           boardSpace.WaterTileOverrides.Value.Contains(GetTileId(upCoordinate)));
                bool downIsInnerCoastline = down.BoardSpace == boardSpace && (boardSpace.WaterTileOverrides.Value.Contains(tileid) != 
                                                                            boardSpace.WaterTileOverrides.Value.Contains(GetTileId(downCoordinate)));
                bool leftIsInnerCoastline = left.BoardSpace == boardSpace && (boardSpace.WaterTileOverrides.Value.Contains(tileid) != 
                                                                            boardSpace.WaterTileOverrides.Value.Contains(GetTileId(leftCoordinate)));
                bool rightIsInnerCoastline = right.BoardSpace == boardSpace && (boardSpace.WaterTileOverrides.Value.Contains(tileid) != 
                                                                             boardSpace.WaterTileOverrides.Value.Contains(GetTileId(rightCoordinate)));
                bool upBorder = tileInfo.BoardSpace != up.BoardSpace || (isOcean && upIsInnerCoastline);
                bool downBorder = tileInfo.BoardSpace != down.BoardSpace || (isOcean && downIsInnerCoastline);
                bool leftBorder = tileInfo.BoardSpace != left.BoardSpace || (isOcean && leftIsInnerCoastline);
                bool rightBorder = tileInfo.BoardSpace != right.BoardSpace || (isOcean && rightIsInnerCoastline);
                bool upNationBorder = (tileInfo.Nation != up.Nation || (!isOcean && upIsInnerCoastline)) 
                                      && tileInfo.Nation is not null;
                bool downNationBorder = (tileInfo.Nation != down.Nation || (!isOcean && downIsInnerCoastline)) 
                                        && tileInfo.Nation is not null;
                bool leftNationBorder = (tileInfo.Nation != left.Nation || (!isOcean && leftIsInnerCoastline)) 
                                        && tileInfo.Nation is not null;
                bool rightNationBorder = (tileInfo.Nation != right.Nation || (!isOcean && rightIsInnerCoastline)) 
                                         && tileInfo.Nation is not null;
                TileShaderBitmask bitmask = new TileShaderBitmask()
                {
                    Up = upNationBorder,
                    Down = downNationBorder,
                    Left = leftNationBorder,
                    Right = rightNationBorder,
                    UpLeft = upNationBorder | leftNationBorder,
                    DownLeft = downNationBorder | leftNationBorder,
                    UpRight = upNationBorder | rightNationBorder,
                    DownRight = downNationBorder | rightNationBorder,
                    
                    AltUp = upBorder,
                    AltDown = downBorder,
                    AltLeft = leftBorder,
                    AltRight = rightBorder,
                    AltUpLeft = upBorder | leftBorder,
                    AltDownLeft = downBorder | leftBorder,
                    AltUpRight = upBorder | rightBorder,
                    AltDownRight = downBorder | rightBorder,
                    
                    RiverUp = isOcean, // | is on a river
                    RiverDown =  isOcean,
                    RiverLeft =  isOcean,
                    RiverRight =  isOcean,
                    RiverUpLeft =  isOcean,
                    RiverDownLeft =  isOcean,
                    RiverUpRight =  isOcean,
                    RiverDownRight =  isOcean,
                };
                MeshInstance3D tileRenderer = GetTile(tileCoordinate);
                Color color;
                if (boardSpace.Nation is not null) color = boardSpace.Nation.Color.Value.Lerp(boardSpace.Color.Value, 0.05f);
                else color = boardSpace.Color.Value;
                ShaderMaterial tileMaterial = (ShaderMaterial)tileRenderer.GetSurfaceOverrideMaterial(0);
                Color borderColor = tileInfo.Nation is null ? Colors.Black : tileInfo.Nation.Color.Value * 1.1f;
                tileMaterial.SetShaderParameter("albedo_color", color);
                tileMaterial.SetShaderParameter("border_color", borderColor);
                tileMaterial.SetShaderParameter("tile_mask", bitmask.GetBitmask());
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

            if (entity is BoardSpace || entity is TileOwnershipBehavior || entity is Nation)
            {
                needsTileRefresh = true;
            }
        }
        
        EntitiesChanged.Clear();
        
        if (needsFullRefresh) FullRefresh();
        else if (needsTileRefresh) TileRefresh();
    }

    protected override void OnTileClicked(ICoordinate2d tileCoordinate, TileClickMetadata input)
    {
        
    }
}