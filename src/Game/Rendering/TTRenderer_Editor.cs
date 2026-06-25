using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Godot;
using TT2026.Game.Behaviors;
using TT2026.Game.Definitions;
using TT2026.Game.Entities;
using TT2026.libraries.Izzy.Geometry;
using TT2026.libraries.IzzysUI;
using TT2026.Libraries.IzzysUI.Popups;
using TT2026.libraries.IzzysUI.Tooltips;
using TT2026.libraries.NetworkedBoardGameEntitySystem;
using TT2026.Libraries.NetworkedBoardGameEntitySystem;
using TT2026.libraries.NetworkedBoardGameEntitySystem.Networking;
using TT2026.Libraries.NetworkedBoardGameEntitySystem.Networking;
using TT2026.Libraries.NetworkedBoardGameEntitySystem.Saving;
using TT2026.libraries.NetworkedBoardGameEntitySystem.SyncedDataTypes;

namespace TT2026.Game.Rendering;

/// <summary>
/// A variant of the Renderer intended as an editor mode
/// </summary>
public partial class TTRenderer_Editor : TTRenderer
{
    private string editorAuthKey = string.Empty;
    private int _selectedBoardSpaceId = -1;
    private BoardSpace _selectedBoardSpace => _selectedBoardSpaceId == -1 ? null : GameState.GetEntity<BoardSpace>(_selectedBoardSpaceId);
    private Queue<(ICoordinate2d tileCoordinate, int boardSpaceAssignment)> _paintQueue = new();
    private Task paintTask = null;
    [Export] private MeshInstance3D _tracingImage;
    public Color ClipboardColor = Colors.Transparent;
    private static List<TTRenderer_Editor> _editors = new();

    public override void _EnterTree()
    {
        base._EnterTree();
        _editors.Add(this);
    }

    override public void _ExitTree()
    {
        _editors.Remove(this);
    }

    public async Task<int> CreateEntity<T>() where T : GameEntity => await CreateEntity(typeof(T));
    public async Task<int> CreateEntity(Type t)
    {
        NetworkResponse response;
        try
        {
            response = await GameState.Client.SendRequestAwaitCallback(
                GameState.Client.ConnectedServer,
                new EditorPacket(
                    entityExists: true,
                    entityId: -2, // Server assigns id to new entity
                    gameStepId: 0,
                    entityType: t,
                    variableName: null,
                    variableValue: null,
                    editorAuthKey: editorAuthKey
                ));
        }
        catch (ArgumentNullException)
        {
            // Client not connected
            OpenMenuContextWindow();
            return -1;
        }
        if (response.Error == NetworkResponseError.None)
        {
            try
            {
                EditorPacketResponse responseObj = JsonSerializer.Deserialize<EditorPacketResponse>(response.Message);
                int entityId = responseObj.EntityId;
                Logger.Log($"Client was told entity id: {entityId}");
                return entityId;
            }
            catch (JsonException)
            {
                Logger.Error($"Bad CreateEntity response from server: {response.Message}"); // TODO bad server responses should be handled
                return -1;
            }

        }
        else
        {
            Logger.Error($"Failed to create entity: {response.Error}");
            return -1;
        }
    }
    public void DeleteEntity(int entityId)
    {
        try
        {
            GameState.Client.SendRequest(
                GameState.Client.ConnectedServer,
                new EditorPacket(
                    entityExists: false,
                    entityId: entityId, // null
                    gameStepId: -1,
                    entityType: null,
                    variableName: null,
                    variableValue: null,
                    editorAuthKey: editorAuthKey
                ));
        }
        catch (ArgumentNullException)
        {
            // Client not connected
            OpenMenuContextWindow();
            return;
        }
    }
    public async Task EditValue(int entityId, string variableName, string variableValue)
    {
        try
        {
            await GameState.Client.SendRequestAwaitCallback(
                GameState.Client.ConnectedServer,
                new EditorPacket(
                    entityExists: true,
                    entityId: entityId, // null
                    gameStepId: 0,
                    entityType: null, // Ignored
                    variableName: variableName,
                    variableValue: variableValue,
                    editorAuthKey: editorAuthKey
                ));
        }
        catch (ArgumentNullException)
        {
            // Client not connected
            OpenMenuContextWindow();
            return;
        }
        return;
    }
    
    RandomNumberGenerator _random = new();
    protected override void OnTileClicked(ICoordinate2d tileCoordinate, TileClickMetadata metadata)
    {
        // base.OnTileClicked(x, y, metadata); // Don't trigger normal renderer input behavior, override it
        int tileId = GetTileId(tileCoordinate);
        if (metadata.ShiftHeld)
        {
            if (_selectedBoardSpaceId != -1 && !_paintQueue.Contains((tileCoordinate, _selectedBoardSpaceId)))
            {
                _paintQueue.Enqueue((tileCoordinate, _selectedBoardSpaceId));
            }
        }
        else
        {
            OpenTileContextWindow(tileCoordinate, GetTilePositionInViewport(tileId));
        }
    }

    protected override void RecalculatePotentialActions() { }

    private void OpenTileContextWindow(ICoordinate2d tileCoordinate, Vector2 position)
    {
        if (GameState.Client.ConnectedServer is null) {OnNotConnected(); return;}

        int tileId = GetTileId(tileCoordinate);
        List<IUIInteraction> interactions = new();
        TileOwnershipBehavior tileOwnership = GameState.GetGameBehavior<TileOwnershipBehavior>();
        if (tileOwnership is null) {Logger.Error($"The GameState needs to have a TileOwnershipBehavior behavior active for editor functionality to work"); return;}
        var boardSpaceId = tileOwnership.GetOwnerOfTile(tileCoordinate);
        if (_selectedBoardSpaceId != -1 && _selectedBoardSpaceId != boardSpaceId) interactions.Add(new SimpleUIActionAsync("Add To Selected", async () =>
        {
            await AddToSelected(tileCoordinate, _selectedBoardSpaceId, () => OpenTileContextWindow(tileCoordinate, position));
            return null;
        }));
        BoardSpace boardSpace = GameState.GetEntity<BoardSpace>(boardSpaceId);
        BoardSpace selectedBoardSpace = GameState.GetEntity<BoardSpace>(_selectedBoardSpaceId);
        string title = boardSpace is null ? $"Unassigned Tile ({tileCoordinate.x}, {tileCoordinate.y})" : $"{boardSpace.Name.Value} ({boardSpace.ID})";
        if (boardSpaceId != -1)
        {
            if (_selectedBoardSpaceId == boardSpaceId) interactions.Add(new SimpleUIAction("Deselect", () => Deselect(() => OpenTileContextWindow(tileCoordinate, position))));
            else interactions.Add(new SimpleUIAction("Select", () => Select(boardSpaceId, () => OpenTileContextWindow(tileCoordinate, position))));
            interactions.Add(new SimpleUIActionAsync("Rename", async () =>
            {
                await Rename(boardSpaceId, () => OpenTileContextWindow(tileCoordinate, position));
                return null;
            }));
            InjectColorActions(tileCoordinate, position, boardSpace, ref interactions);
            InjectSpacePropertiesActions(tileCoordinate, position, boardSpace, ref interactions);
            InjectTerrainActions(tileCoordinate, position, boardSpace, ref interactions);
            if (selectedBoardSpace?.OwnerNation is not null)
            {
                interactions.Add(new SimpleUIActionAsync($"Set Nation to {selectedBoardSpace.OwnerNation.Definition.Value.Name}", async () =>
                {
                    await SetNation(boardSpaceId, selectedBoardSpace.OwnerNation.ID, () => OpenTileContextWindow(tileCoordinate, position));
                    return null;
                }));
            }
            if (boardSpace?.OwnerNation is null)
            {
                interactions.Add(new SimpleUIActionAsync("Create Nation", async () =>
                {
                    await CreateNation(boardSpaceId, boardSpace, () => OpenTileContextWindow(tileCoordinate, position));
                    return null;
                }));
            }

            if (boardSpace.OwnerNation is not null)
            {
                interactions.Add(new Link<Nation>(boardSpace.OwnerNation));
            }
        }
        
        interactions.Add(new SimpleUIActionAsync("Create Space", async () =>
        {
            await CreateSpace(tileCoordinate, tileOwnership, () => OpenTileContextWindow(tileCoordinate, position));
            return null;
        }));

        string text = null;
        if (boardSpace is not null)
        {
            StringBuilder sb = new StringBuilder();
            if (boardSpace.CityType is not null) sb.AppendLine($"{boardSpace.CityType.Definition.Value.Name}");
            else sb.AppendLine($"Undeveloped");
            sb.Append($"Resources: {boardSpace.Resources.Value}");
            text = sb.ToString();
        }
        
        IzzysUIController.OpenContextWindow(new ContextWindowInfo(
            header: title,
            text: text,
            interactions: interactions
            ), position);
    }
    
    private void OpenMenuContextWindow()
    {
        if (GameState.Client.ConnectedServer is null) {OnNotConnected(); return;}
        
        var interactions = new List<IUIInteraction>();
        
        // Only allow saving/loading if the game is not started to prevent accidentally overriding the scenario
        if (GameState.GameStepID == 0)
        {
            interactions.Add(new SimpleUIAction("Quicksave", () => Quicksave()));
            interactions.Add(new SimpleUIAction("Quickload", () => Quickload()));
            interactions.Add(new SimpleUIAction("Start Game", () => StartGame()));
        }
        
        Type[] definitionTypes = new[]
        {
            typeof(UnitType),
            typeof(CityType),
        };
        foreach (var type in definitionTypes)
        {
            interactions.Add(new SimpleUIAction($"Edit {type.Name}s", () =>
            {
                var editorInteractions = new List<IUIInteraction>();
                editorInteractions.Add(new SimpleUIActionAsync($"Create New {type.Name}", async () =>
                {
                    try
                    {
                        await CreateEntity(type);
                        IzzysUIController.CloseActiveContextWindow();
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e.ToString());
                    }
                    return null;
                }));
                foreach (var entity in GameState.GetEntitiesOfType(type))
                {
                    if (entity is not ILinkable linkable) throw new InvalidOperationException($"{entity.GetType().Name} is not ILinkable");
                    editorInteractions.Add(new Link(linkable));
                }
                IzzysUIController.OpenContextWindow(new ContextWindowInfo(
                    header: $"Edit {type.Name}",
                    interactions: editorInteractions));
            }));
        }
        IzzysUIController.OpenContextWindow(new ContextWindowInfo(
            header: "Editor Menu",
            interactions: interactions));
    }

    private async Task AddBoardSpaceToSelected(ICoordinate2d tileCoordinate, int boardSpaceId)
    {
        if (boardSpaceId == -1) return;
        var tileOwnership = GameState.GetGameBehavior<TileOwnershipBehavior>();
        if (tileOwnership is null) {Logger.Error($"No {nameof(TileOwnershipBehavior)} active"); return;}
        tileOwnership.SetOwnerOfTile(tileCoordinate, boardSpaceId);
        await EditValue(tileOwnership.ID, nameof(tileOwnership.TileOwnership), tileOwnership.TileOwnership.SerializeData());
        return;
    }

    public override void _Ready()
    {
        base._Ready();
        _tracingImage.Scale = new Vector3(Width, Height, 1);
        _tracingImage.Position = new Vector3(Width / 2f, 0.1f, Height / 2f);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (paintTask is null || paintTask.IsCompleted)
        {
            paintTask?.Dispose();
            if (_paintQueue.TryDequeue(out var paintRequest))
            {
                try
                {
                    paintTask = AddBoardSpaceToSelected(paintRequest.tileCoordinate, paintRequest.boardSpaceAssignment);
                }
                catch (ArgumentNullException)
                {
                    OpenMenuContextWindow();
                }
            }
        }
    }
    
    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
        if (@event is InputEventMouseButton mb)
        {
            if (!mb.Pressed && mb.ButtonIndex == MouseButton.Right)
            {
                if (IzzysUIController.IsContextWindowOpen)
                {
                    IzzysUIController.CloseActiveContextWindow();
                }
                else
                {
                    OpenMenuContextWindow();
                }
            }
        }
        else if (@event is InputEventKey key)
        {
            if (key.Keycode == Key.T && key.Pressed)
            {
                _tracingImage.SetVisible(!_tracingImage.Visible);
            }
        }
    }

    private void InjectColorActions(ICoordinate2d tileCoordinate, Vector2 position, BoardSpace boardSpace, ref List<IUIInteraction> interactions)
    {
        interactions.Add(new SimpleUIActionAsync("Set Color", async () =>
        {
            await SetColor(boardSpace.ID, () => OpenTileContextWindow(tileCoordinate, position));
            return null;
        }));
        interactions.Add(new SimpleUIActionAsync("Paste Color", async () =>
        {
            await PasteColor(boardSpace.ID, () => OpenTileContextWindow(tileCoordinate, position));
            return null;
        }));
        // Nudge Color intentionally does not refresh the context window
        interactions.Add(new SimpleUIAction("Nudge Color", () => NudgeColor(boardSpace.ID, boardSpace)));
    }
    
    private void InjectTerrainActions(ICoordinate2d tileCoordinate, Vector2 position, BoardSpace boardSpace, ref List<IUIInteraction> interactions)
    {
        if (boardSpace.GetTerrainType() == TerrainType.Land)
            interactions.Add(new SimpleUIActionAsync("Set to Ocean", async () =>
            {
                await SetTerrain(boardSpace.ID, TerrainType.Water, () => OpenTileContextWindow(tileCoordinate, position));
                return null;
            }));
        else if (boardSpace.GetTerrainType() == TerrainType.Water)
        {
            interactions.Add(new SimpleUIActionAsync("Set to Land", async () =>
            {
                await SetTerrain(boardSpace.ID, TerrainType.Land, () => OpenTileContextWindow(tileCoordinate, position));
                return null;
            }));
        }
        interactions.Add(new SimpleUIActionAsync("Toggle Water Override", async () =>
        {
            await ToggleTileWaterStatus(tileCoordinate, () => OpenTileContextWindow(tileCoordinate, position));
            return null;
        }));
    }
    
    private void InjectSpacePropertiesActions(ICoordinate2d tileCoordinate, Vector2 position, BoardSpace boardSpace, ref List<IUIInteraction> interactions)
    {
        if (boardSpace.GetTerrainType() == TerrainType.Land) {
            interactions.Add(new SimpleUIActionAsync("Set City Type", async () =>
            {
                await SetCityType(boardSpace.ID, onCompletion:  () => OpenTileContextWindow(tileCoordinate, position));
                return null;
            }));
            interactions.Add(new SimpleUIActionAsync("Set Resource Count", async () =>
            {
                await SetResources(boardSpace.ID, onCompletion: () => OpenTileContextWindow(tileCoordinate, position));
                return null;
            }));
            interactions.Add(new SimpleUIActionAsync("Set City Position", async () =>
            {
                await SetCityTilePosition(tileCoordinate, boardSpace.ID);
                return null;
            }));
            interactions.Add(new SimpleUIActionAsync("Set Resources Position", async () =>
            {
                await SetResourceTilePosition(tileCoordinate, boardSpace.ID);
                return null;
            }));
        }
    }
}