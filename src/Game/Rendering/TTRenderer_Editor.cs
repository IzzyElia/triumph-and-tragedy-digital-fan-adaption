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
using TT2026.libraries.NetworkedBoardGameEntitySystem.SyncedDataTypes;

namespace TT2026.Game.Rendering;

public partial class TTRenderer_Editor : TTRenderer
{
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
    private async Task<int> CreateEntity<T>() where T : GameEntity
    {
        NetworkResponse response = await GameState.Client.SendRequestAwaitCallback(
            GameState.Client.ConnectedServer,
            new EditorPacket(
                entityExists: true,
                entityId: -2, // Server assigns id to new entity
                gameStepId: 0,
                entityType: typeof(T),
                variableName: null,
                variableValue: null
            ));
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
    private void DeleteEntity(int entityId)
    {
        GameState.Client.SendRequest(
            GameState.Client.ConnectedServer,
            new EditorPacket(
                entityExists: false,
                entityId: entityId, // null
                gameStepId: -1,
                entityType: null,
                variableName: null,
                variableValue: null
            ));
    }
    private void EditValue(int entityId, string variableName, string variableValue)
    {
        GameState.Client.SendRequest(
            GameState.Client.ConnectedServer,
            new EditorPacket(
                entityExists: true,
                entityId: entityId, // null
                gameStepId: 0,
                entityType: null, // Ignored
                variableName: variableName,
                variableValue: variableValue
            ));
    }
    RandomNumberGenerator _random = new();
    protected override void OnTileClicked(int x, int y)
    {
        base.OnTileClicked(x, y);
        if (GameState.Client.ConnectedServer is null) {OnNotConnected(); return;}

        List<IUIInteraction> interactions = new();
        TileOwnershipBehavior tileOwnership = GameState.GetGameBehavior<TileOwnershipBehavior>();
        if (tileOwnership is null) {Logger.Error($"The GameState needs to have a TileOwnershipBehavior behavior active for editor functionality to work"); return;}

        int tileId = GetTileId(new GenericCoordinate2d(x, y));
        var boardSpaceId = tileOwnership.GetOwnerOfTile(tileId);
        if (boardSpaceId == -1) 
            interactions.Add(new SimpleUIActionAsync("Create Space", async () =>
            {
                try
                {
                    boardSpaceId = await CreateEntity<BoardSpace>();
                    Logger.Log($"Created Space: {boardSpaceId}, setting up properties");
                    tileOwnership.SetOwnerOfTile(tileId, boardSpaceId);
                    EditValue(tileOwnership.ID, "TileOwnership", tileOwnership.TileOwnership.SerializeData());
                }
                catch (Exception e)
                {
                    Logger.Error(e.ToString());
                }
                
                return null;
            }));
        else
        {
            string serializedColor = new SyncedColor(null, null,
                new Color(_random.Randf(), _random.Randf(), _random.Randf())).SerializeData();
            interactions.Add(new SimpleUIAction("Randomize Color", () => EditValue(boardSpaceId, "Color", serializedColor)));
            interactions.Add(new SimpleUIActionAsync("Rename", async () =>
            {
                try
                {
                    object result = await IzzysUIController.OpenPopupAndGetResult(new PopupInfo()
                    {
                        PopupType = PopupType.Text
                    });
                    EditValue(boardSpaceId, nameof(BoardSpace.Name), (string)result);
                }
                catch (OperationCanceledException)
                {
                    Logger.Log($"Rename Cancelled");
                }
                
                return null;
            }));
        }

        BoardSpace boardSpace = GameState.GetEntity<BoardSpace>(boardSpaceId);
        string title = boardSpace is null ? $"Tile {x}, {y}" : $"{boardSpace.Name.Value} ({boardSpace.ID})";
        IzzysUIController.OpenContextWindow(new ContextWindowInfo(
            header: title,
            interactions: interactions
            ));
    }
}