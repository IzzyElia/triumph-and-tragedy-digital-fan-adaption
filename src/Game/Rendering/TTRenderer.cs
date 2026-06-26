using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using TT2026.Game.Definitions;
using TT2026.Game.Entities;
using TT2026.Game.Rendering.BoardObjects;
using TT2026.libraries.Izzy.Geometry;
using TT2026.libraries.IzzysUI;
using TT2026.libraries.IzzysUI.Tooltips;
using TT2026.libraries.NetworkedBoardGameEntitySystem;
using TT2026.Libraries.NetworkedBoardGameEntitySystem;
using TT2026.libraries.NetworkedBoardGameEntitySystem.Rendering;
using TT2026.Libraries.NetworkedBoardGameEntitySystem.Rendering;

namespace TT2026.Game.Rendering;

public abstract partial class TTRenderer : TilingGameRenderer
{
    private const string _staticBoardObjectUid = "uid://bshll6csw7uxa";
    public static PackedScene StaticBoardObjectPrefab = ResourceLoader.Load<PackedScene>(_staticBoardObjectUid);

    private const string _resourcesIconTextureUid = "uid://crjaf3w20iim8";
    public static Texture2D ResourcesIconTexture => ResourceLoader.Load<Texture2D>(_resourcesIconTextureUid);

    private const string _fallbackCityIconTextureUid = "uid://ces18fqqwxras";
    
    static Random _random = new();

    protected HashSet<int> HighlightSpaces = new();
    protected HashSet<RefreshType> RefreshesNeeded = new();
    private List<RefreshType> _refreshQueue = new();
    private Dictionary<int, UnitBoardObject> _unitRenderers = new();
    
    public override void Initialize()
    {
        base.Initialize();
        
    }
    
    protected void OnNotConnected()
    {
        IzzysUIController.OpenContextWindow(new ContextWindowInfo(
            header: $"Disconnected from Server",
            interactions: [new SimpleUIAction("Reconnect", () =>
            {
                GameState.Client.Reconnect();
            })]
        ));
    }

    public TileInfo GetTileInfo(ICoordinate2d tileCoordinate, ref TileOwnershipBehavior tileOwnership)
    {
        int tileId = GetTileId(tileCoordinate);
        BoardSpace boardSpace = GameState.GetEntity<BoardSpace>(tileOwnership.GetOwnerOfTile(tileCoordinate));
        Nation nation = boardSpace is null ? null : boardSpace.OwnerNation;
        return new TileInfo()
        {
            BoardSpace = boardSpace,
            Nation = nation
        };
    }

    private Dictionary<int, StaticBoardObject> _cityRenderers = new();
    private Dictionary<int, StaticBoardObject> _resourceRenderers = new();

    private StaticBoardObject SetStaticRendererPosition(BoardSpace boardSpace, int tileId, ref System.Collections.Generic.Dictionary<int, StaticBoardObject> dict)
    {
        StaticBoardObject renderer;
        if (tileId == -1)
        {
            if (!dict.TryGetValue(boardSpace.ID, out renderer)) return null;
            renderer.QueueFree();
            dict.Remove(boardSpace.ID);
            return null;
        }
        
        if (!dict.TryGetValue(boardSpace.ID, out renderer))
        {
            renderer = StaticBoardObjectPrefab.Instantiate<StaticBoardObject>();
            dict.Add(boardSpace.ID, renderer);
        }

        var tile = GetTile(tileId);
        if (renderer.GetParent() is null) tile.AddChild(renderer);
        else renderer.Reparent(tile);
        renderer.SetPosition(Vector3.Zero);
        return renderer;
    }
    protected void MatchRendererDictToArray<TElement, TRenderer>(TElement[] array, ref Dictionary<int, TRenderer> dict, Func<TRenderer> instantiator) where TRenderer : Node3D
    {
        // Cleanup old renderers if needed
        Span<int> freedIds = stackalloc int[dict.Count];
        freedIds.Fill(-1);
        int i = 0;
        foreach (var index in dict.Keys)
        {
            if (index >= array.Length)
            {
                freedIds[i] = index;
            }

            i++;
        }
        foreach (int index in freedIds)
        {
            if (index == -1) continue;
            dict[index].QueueFree();
            dict.Remove(index);
        }
        
        // Create new renderers if needed
        for (i = 0; i < array.Length; i++)
        {
            if (!dict.ContainsKey(i))
            {
                var renderer = instantiator.Invoke();
                dict.Add(i, renderer);
            }
        }
    }
    
    protected void MatchRendererArray<TEntity, TRenderer>(ref Dictionary<int, TRenderer> dict, Func<TRenderer> instantiator) where TEntity : GameEntity where TRenderer : Node3D
    {
        // Cleanup old renderers if needed
        Span<int> freedIds = stackalloc int[dict.Count];
        freedIds.Fill(-1);
        int i = 0;
        foreach (int id in dict.Keys)
        {
            var entity = GameState.GetEntity<Unit>(id);
            if (entity is null || entity.Exists.Value == false) freedIds[i] = id;
            i++;
        }

        foreach (var freedId in freedIds)
        {
            if (freedId == -1) continue;
            dict[freedId].QueueFree();
            dict.Remove(freedId);
        }
        
        // Create new renderers if needed
        foreach (var entity in GameState.GetEntitiesOfType<TEntity>())
        {
            if (!dict.ContainsKey(entity.ID))
            {
                var renderer = instantiator.Invoke();
                dict.Add(entity.ID, renderer);
            }
        }
    }
    public virtual void UnitRefresh()
    {
        Logger.Log($"Running Unit refresh");
        MatchRendererArray<Unit, UnitBoardObject>(
            dict: ref _unitRenderers, 
            instantiator: () => UnitBoardObject.Prefab.Instantiate<UnitBoardObject>()
            );

        foreach ((int i, UnitBoardObject renderer) in _unitRenderers)
        {
            var entity = GameState.GetEntity<Unit>(i);
            PlaceUnitRendererOnSpace(entity.BoardSpaceId.Value, renderer);
        }
    }

    protected void PlaceUnitRendererOnSpace(int boardSpaceId, UnitBoardObject renderer)
    {
        var tileOwnership = GameState.GetGameBehavior<TileOwnershipBehavior>();
        if (renderer.BoardSpaceId != boardSpaceId)
        {
            var potentialTiles = tileOwnership.GetTilesOfOwner(boardSpaceId).ToArray();
            var tilePosition = potentialTiles[_random.Next(potentialTiles.Length)];
            var tile = GetTile(tilePosition);
            if (renderer.BoardSpaceId == -1) tile.AddChild(renderer);
            else renderer.Reparent(tile);
            renderer.SetPosition(new Vector3(_random.NextSingle() - 0.5f, _random.NextSingle() - 0.5f, 0f));
            renderer.BoardSpaceId = boardSpaceId;
        }
    }
    
    public void TileRefresh()
    {
        Logger.Log($"Running tile refresh");
        var tileOwnership = GameState.GetGameBehavior<TileOwnershipBehavior>();
        if (tileOwnership is null) return;
        foreach (var boardSpace in GameState.GetEntitiesOfType<BoardSpace>())
        {
            if (boardSpace.CityTilePosition.Value != -1 && boardSpace.CityType is not null)
            {
                var cityDefinition = boardSpace.CityType.Definition.Value;
                var city = SetStaticRendererPosition(boardSpace, boardSpace.CityTilePosition.Value, ref _cityRenderers);
                city.SetScale(Vector3.One * cityDefinition.IconScale);
                city.IconMaterial.Emission = boardSpace.OwnerNation is not null ? boardSpace.OwnerNation.Color.Value : Colors.DeepPink;
                city.IconMaterial.AlbedoColor = city.IconMaterial.Emission;
                try
                {
                    city.IconMaterial.AlbedoTexture = GD.Load<Texture2D>(cityDefinition.IconUid);
                }
                catch (InvalidCastException)
                {
                    // TODO Handle this
                    city.IconMaterial.AlbedoTexture = GD.Load<Texture2D>(_fallbackCityIconTextureUid);
                }
            }
            else SetStaticRendererPosition(boardSpace, -1, ref _cityRenderers);

            if (boardSpace.ResourcesTilePosition.Value != -1 && boardSpace.Resources.Value > 0)
            {
                var resources = SetStaticRendererPosition(boardSpace, boardSpace.ResourcesTilePosition.Value, ref _resourceRenderers);
                resources.IconLabel.Text = $"{boardSpace.Resources.Value}";
                resources.IconLabel.FontSize = 72;
                resources.IconMaterial.AlbedoTexture = ResourcesIconTexture;
                resources.IconMaterial.AlbedoColor = Colors.Black;
                resources.IconMaterial.Emission = Colors.Black;
                resources.BackgroundMaterial.AlbedoColor = Colors.Transparent;
            }
            else SetStaticRendererPosition(boardSpace, -1, ref _resourceRenderers);
            
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
                if (boardSpace.OwnerNation is not null) color = boardSpace.OwnerNation.Color.Value.Lerp(boardSpace.Color.Value, 0.05f);
                else color = boardSpace.Color.Value;
                ShaderMaterial tileMaterial = (ShaderMaterial)tileRenderer.GetSurfaceOverrideMaterial(0);
                Color borderColor = tileInfo.Nation is null ? Colors.Black : tileInfo.Nation.Color.Value * 1.1f;
                tileMaterial.SetShaderParameter("albedo_color", color);
                tileMaterial.SetShaderParameter("border_color", borderColor);
                tileMaterial.SetShaderParameter("alt_border_color", boardSpace is not null && HighlightSpaces.Contains(boardSpace.ID) ? Colors.Cornsilk : Colors.Black);
                tileMaterial.SetShaderParameter("tile_mask", bitmask.GetBitmask());
            }
        }
    }
    
    public override void _Process(double delta)
    {
        base._Process(delta);
        
        foreach (var changedEntity in EntitiesChanged)
        {
            if (changedEntity == Constants.GameStateChangePlayerSignalId)
            {
                RefreshesNeeded.Add(RefreshType.Everything);
                continue;
            }
            if (changedEntity == Constants.GameStateAdvanceSignalId)
            {
                RecalculatePotentialActions();
                continue;
            }
            GameEntity entity = GameState.GetEntity(changedEntity);
            if (entity is null) // entity was deleted
            {
                RefreshesNeeded.Add(RefreshType.Everything);
                break;
            }

            if (entity is BoardSpace 
                || entity is TileOwnershipBehavior 
                || entity is Nation 
                || entity is CityType)
            {
                RefreshesNeeded.Add(RefreshType.Tiles);
            }

            if (entity is Unit)
            {
                RefreshesNeeded.Add(RefreshType.Units);
            }
        }
        EntitiesChanged.Clear();
        
        // Refresh step --------------------
        _refreshQueue.Clear();
        if (RefreshesNeeded.Contains(RefreshType.Everything)) FullRefresh();
        else
        {
            foreach (var refreshType in RefreshesNeeded)
            {
                _refreshQueue.Add(refreshType);
            }
        }
        RefreshesNeeded.Clear();

        foreach (var refreshType in _refreshQueue)
        {
            switch (refreshType)
            {
                case RefreshType.Tiles: TileRefresh(); break;
                case RefreshType.Units: UnitRefresh(); break;
                default: throw new NotImplementedException();
            }
        }
        
        
    }
    
    public override void FullRefresh()
    {
        base.FullRefresh();
        RecalculatePotentialActions();
        TileRefresh();
        UnitRefresh();
    }

    public virtual void OnPlayerChanged()
    {
        FullRefresh();
    }

    protected override void OnTileClicked(ICoordinate2d tileCoordinate, TileClickMetadata input)
    {
        
    }

    protected abstract void RecalculatePotentialActions();

    protected enum RefreshType
    {
        Everything,
        Tiles,
        Units,
    }
}