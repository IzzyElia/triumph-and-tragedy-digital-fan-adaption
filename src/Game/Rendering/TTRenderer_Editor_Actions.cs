using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using TT2026.Game.Behaviors;
using TT2026.Game.Entities;
using TT2026.libraries.Izzy.Geometry;
using TT2026.libraries.IzzysUI;
using TT2026.Libraries.IzzysUI.Popups;
using TT2026.libraries.NetworkedBoardGameEntitySystem;
using TT2026.Libraries.NetworkedBoardGameEntitySystem;
using TT2026.libraries.NetworkedBoardGameEntitySystem.Networking;
using TT2026.Libraries.NetworkedBoardGameEntitySystem.Networking;
using TT2026.Libraries.NetworkedBoardGameEntitySystem.Saving;
using TT2026.libraries.NetworkedBoardGameEntitySystem.SyncedDataTypes;

namespace TT2026.Game.Rendering;

// All user-triggered editor actions. Each takes an optional onCompletion callback
// (invoked after the action's work completes) so callers — e.g. OpenTileContextWindow —
// can refresh the context window without baking that refresh into the action itself.
public partial class TTRenderer_Editor
{
    public async Task AddToSelected(ICoordinate2d tileCoordinate, int boardSpaceId, Action onCompletion = null)
    {
        await AddBoardSpaceToSelected(tileCoordinate, boardSpaceId);
        onCompletion?.Invoke();
    }

    public void Deselect(Action onCompletion = null)
    {
        _selectedBoardSpaceId = -1;
        onCompletion?.Invoke();
    }

    public void Select(int boardSpaceId, Action onCompletion = null)
    {
        _selectedBoardSpaceId = boardSpaceId;
        onCompletion?.Invoke();
    }

    public async Task SetColor(int boardSpaceId, Action onCompletion = null)
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
            if (color.Equals(Colors.Transparent)) return;
            string serializedColor = new SyncedColor(null, null, color).SerializeData();
            await EditValue(boardSpaceId, nameof(BoardSpace.Color), serializedColor);
            onCompletion?.Invoke();
        }
        catch (OperationCanceledException)
        {

        }
    }

    public async Task PasteColor(int boardSpaceId, Action onCompletion = null)
    {
        try
        {
            if (ClipboardColor.Equals(Colors.Transparent)) return;
            string serializedColor = new SyncedColor(null, null, ClipboardColor).SerializeData();
            await EditValue(boardSpaceId, nameof(BoardSpace.Color), serializedColor);
            onCompletion?.Invoke();
        }
        catch (OperationCanceledException)
        {

        }
    }

    public void NudgeColor(int boardSpaceId, BoardSpace boardSpace, Action onCompletion = null)
    {
        _ = EditValue(boardSpaceId, nameof(boardSpace.Color),
            new SyncedColor(null, null,
                new Color(
                    r: boardSpace.Color.Value.R + ((_random.Randf() - 0.5f) * 0.1f),
                    g: boardSpace.Color.Value.G + ((_random.Randf() - 0.5f) * 0.1f),
                    b: boardSpace.Color.Value.B + ((_random.Randf() - 0.5f) * 0.1f)
                )).SerializeData());
        onCompletion?.Invoke();
    }

    public async Task Rename(int boardSpaceId, Action onCompletion = null)
    {
        try
        {
            object result = await IzzysUIController.OpenPopupAndGetResult(new PopupInfo()
            {
                Header = $"Rename to",
                PopupType = PopupType.Text
            });
            await EditValue(boardSpaceId, nameof(BoardSpace.Name), (string)result);
            onCompletion?.Invoke();
        }
        catch (OperationCanceledException)
        {
            Logger.Log($"Rename Cancelled");
        }
    }

    public async Task SetNation(int boardSpaceId, int nationId, Action onCompletion = null)
    {
        await EditValue(boardSpaceId, nameof(BoardSpace.NationId),
            new SyncedInt(null, null, nationId).SerializeData());
        onCompletion?.Invoke();
    }

    public async Task CreateNation(int boardSpaceId, BoardSpace boardSpace, Action onCompletion = null)
    {
        try
        {
            int nationId = await CreateEntity<Nation>();
            boardSpace.NationId.Value = nationId;
            await EditValue(boardSpaceId, nameof(boardSpace.NationId), boardSpace.NationId.SerializeData());
            onCompletion?.Invoke();
        }
        catch (OperationCanceledException)
        {
            Logger.Log($"Rename Cancelled");
        }
    }

    public async Task CreateSpace(ICoordinate2d tileCoordinate, TileOwnershipBehavior tileOwnership, Action onCompletion = null)
    {
        try
        {
            int boardSpaceId = await CreateEntity<BoardSpace>();
            tileOwnership.SetOwnerOfTile(tileCoordinate, boardSpaceId);
            await EditValue(tileOwnership.ID, "TileOwnership", tileOwnership.TileOwnership.SerializeData());
            onCompletion?.Invoke();
        }
        catch (Exception e)
        {
            Logger.Error(e.ToString());
        }
    }

    private async Task SetTerrain(int boardSpaceId, TerrainType terrainType, Action onCompletion = null)
        {
            BoardSpace boardSpace = GameState.GetEntity<BoardSpace>(boardSpaceId);
            if (boardSpace is null) return;
            await EditValue(boardSpaceId, nameof(BoardSpace.TerrainType),
                new SyncedInt(null, null, (int)terrainType).SerializeData());
            onCompletion?.Invoke();
        }

    private async Task ToggleTileWaterStatus(ICoordinate2d tileCoordinate, Action onCompletion = null)
    {
        int tileId = GetTileId(tileCoordinate);
        var tileOwnership = GameState.GetGameBehavior<TileOwnershipBehavior>();
        if (tileOwnership is null)
        {
            Logger.Log($"Tile ownership behavior not enabled");
            return;
        }
        BoardSpace boardSpace = GameState.GetEntity<BoardSpace>(tileOwnership.GetOwnerOfTile(tileCoordinate));
        if (boardSpace is null)
        {
            Logger.Log($"No board space found for {tileId}");
            return;
        }

        if (boardSpace.WaterTileOverrides.Value.Contains(tileId)) 
            boardSpace.WaterTileOverrides.Value = boardSpace.WaterTileOverrides.Value.Where(x => x != tileId).ToArray();
        else 
            boardSpace.WaterTileOverrides.Value = boardSpace.WaterTileOverrides.Value.Append(tileId).ToArray();

        await EditValue(boardSpace.ID, nameof(boardSpace.WaterTileOverrides), boardSpace.WaterTileOverrides.SerializeData());
        onCompletion?.Invoke();
        return;
    }
    
    // --- Editor menu actions ---

    public void Quicksave(Action onCompletion = null)
    {
        string json = GameStateSaver.SerializeGameStae(GameState);
        GameStateSaver.SaveToFile("scenarios/save.json", json);
        onCompletion?.Invoke();
    }

    public void Quickload(Action onCompletion = null)
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
        onCompletion?.Invoke();
    }

}
