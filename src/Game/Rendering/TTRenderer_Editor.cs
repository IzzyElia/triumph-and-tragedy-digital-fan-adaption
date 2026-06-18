using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Godot;
using TT2026.Game.Behaviors;
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

    private void OnNotConnected()
    {
        IzzysUIController.OpenContextWindow(new ContextWindowInfo(
            header: $"Disconnected from Server",
            interactions: [new SimpleUIAction("Reconnect", () =>
            {
                GameState.Client.Reconnect();
            })]
        ));
    }
    public async Task<int> CreateEntity<T>() where T : GameEntity
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
                    entityType: typeof(T),
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
            await AddBoardSpaceToSelected(tileCoordinate, _selectedBoardSpaceId);
            OpenTileContextWindow(tileCoordinate, position);
            return null;
        }));
        BoardSpace boardSpace = GameState.GetEntity<BoardSpace>(boardSpaceId);
        BoardSpace selectedBoardSpace = GameState.GetEntity<BoardSpace>(_selectedBoardSpaceId);
        string title = boardSpace is null ? $"Unassigned Tile ({tileCoordinate.x}, {tileCoordinate.y})" : $"{boardSpace.Name.Value} ({boardSpace.ID})";
        if (boardSpaceId != -1)
        {
            if (_selectedBoardSpaceId == boardSpaceId) interactions.Add(new SimpleUIAction("Deselect", () => { _selectedBoardSpaceId = -1; OpenTileContextWindow(tileCoordinate, position); }));
            else interactions.Add(new SimpleUIAction("Select", () => { _selectedBoardSpaceId = boardSpaceId; OpenTileContextWindow(tileCoordinate, position); }));
            interactions.Add(new SimpleUIActionAsync("Set Color", async () =>
            {
                try
                {
                    object result = await IzzysUIController.OpenPopupAndGetResult(new PopupInfo()
                    {
                        Header = "Color",
                        PopupType = PopupType.Text,
                    });
                    string cast = (string)result;
                    Color color = Color.FromString(cast, Colors.Transparent);
                    color.A = 1f;
                    if (color.Equals(Colors.Transparent)) return null;
                    string serializedColor = new SyncedColor(null, null, color).SerializeData();
                    await EditValue(boardSpaceId, nameof(BoardSpace.Color), serializedColor);
                    OpenTileContextWindow(tileCoordinate, position);
                }
                catch (OperationCanceledException)
                {
                    
                }
                return null;
            }));
            interactions.Add(new SimpleUIActionAsync("Paste Color", async () =>
            {
                try
                {
                    if (ClipboardColor.Equals(Colors.Transparent)) return null;
                    string serializedColor = new SyncedColor(null, null, ClipboardColor).SerializeData();
                    await EditValue(boardSpaceId, nameof(BoardSpace.Color), serializedColor);
                    OpenTileContextWindow(tileCoordinate, position);
                }
                catch (OperationCanceledException)
                {
                    
                }
                return null;
            }));
            interactions.Add(
                new SimpleUIAction("Nudge Color", () => EditValue(boardSpaceId, nameof(boardSpace.Color), 
                new SyncedColor(null, null, 
                new Color(
                    r: boardSpace.Color.Value.R + ((_random.Randf() - 0.5f) * 0.1f),
                    g: boardSpace.Color.Value.G + ((_random.Randf() - 0.5f) * 0.1f),
                    b: boardSpace.Color.Value.B + ((_random.Randf() - 0.5f) * 0.1f)
                )).SerializeData()
            )));
            interactions.Add(new SimpleUIActionAsync("Rename", async () =>
            {
                try
                {
                    object result = await IzzysUIController.OpenPopupAndGetResult(new PopupInfo()
                    {
                        Header = $"Rename to",
                        PopupType = PopupType.Text
                    });
                    await EditValue(boardSpaceId, nameof(BoardSpace.Name), (string)result);
                    OpenTileContextWindow(tileCoordinate, position);
                }
                catch (OperationCanceledException)
                {
                    Logger.Log($"Rename Cancelled");
                }
                
                return null;
            }));

            if (selectedBoardSpace?.Nation is not null)
            {
                interactions.Add(new SimpleUIActionAsync($"Set Nation to {selectedBoardSpace.Nation.Name.Value}", async () =>
                {
                    await EditValue(boardSpaceId, nameof(boardSpace.NationId),
                        new SyncedInt(null, null, selectedBoardSpace.Nation.ID).SerializeData());
                    OpenTileContextWindow(tileCoordinate, position);
                    return null;
                }));
            }
            if (boardSpace?.Nation is null)
            {
                interactions.Add(new SimpleUIActionAsync("Create Nation", async () =>
                {
                    try
                    {
                        int nationId = await CreateEntity<Nation>();
                        boardSpace.NationId.Value = nationId;
                        await EditValue(boardSpaceId, nameof(boardSpace.NationId), boardSpace.NationId.SerializeData());
                        OpenTileContextWindow(tileCoordinate, position);
                    }
                    catch (OperationCanceledException)
                    {
                        Logger.Log($"Rename Cancelled");
                    }
                
                    return null;
                }));
            }

            if (boardSpace.Nation is not null)
            {
                interactions.Add(new Link<Nation>(boardSpace.Nation));
            }
        }
        
        interactions.Add(new SimpleUIActionAsync("Create Space", async () =>
        {
            try
            {
                boardSpaceId = await CreateEntity<BoardSpace>();
                tileOwnership.SetOwnerOfTile(tileCoordinate, boardSpaceId);
                await EditValue(tileOwnership.ID, "TileOwnership", tileOwnership.TileOwnership.SerializeData());
                OpenTileContextWindow(tileCoordinate, position);
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
            
            return null;
        }));
        
        IzzysUIController.OpenContextWindow(new ContextWindowInfo(
            header: title,
            interactions: interactions
            ), position);
    }

    private void OpenMenuContextWindow()
    {
        if (GameState.Client.ConnectedServer is null) {OnNotConnected(); return;}

        
        var interactions = new List<IUIInteraction>();
        interactions.Add(new SimpleUIAction("Quicksave", () =>
        {
            string json = GameStateSaver.SerializeGameStae(GameState);
            GameStateSaver.SaveToFile("scenarios/save.json", json);
        }));
        interactions.Add(new SimpleUIAction("Quickload", () =>
        {
            string json = GameStateSaver.LoadFromFile("scenarios/save.json");
            string[] chunks = Utils.ChunkString(json, 1100);
            for (int i = 0; i < chunks.Length; i++)
            {
                string chunk = chunks[i];
                Client.SendRequest(Client.ConnectedServer, new ImportGameStatePacket(
                    part: i,
                    numParts: chunks.Length,
                    json: chunk,
                    editorAuthKey: editorAuthKey
                    ));
            }
        }));
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
}