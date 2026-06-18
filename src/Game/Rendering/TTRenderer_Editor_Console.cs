using System;
using System.Threading.Tasks;
using TT2026.Game.Entities;
using TT2026.libraries.IzzysConsole.API;
using TT2026.libraries.IzzysConsole.Internal;
using TT2026.Libraries.NetworkedBoardGameEntitySystem;
using TT2026.libraries.NetworkedBoardGameEntitySystem.SyncedDataTypes;

namespace TT2026.Game.Rendering;

// Less often used commands are implemented through the console
public partial class TTRenderer_Editor
{
    [ConsoleCommand("editor", allowUseInScoping: true)]
    public static TTRenderer_Editor GetEditor()
    {
        foreach (var editor in _editors)
        {
            if (editor.Visible) return editor;
        }
        ConsoleManager.Log("No editor found");
        return null;
    }

    [ConsoleCommand("copy")]
    public void CopyColor()
    {
        BoardSpace boardSpace = _selectedBoardSpaceId == -1 ? null : GameState.GetEntity<BoardSpace>(_selectedBoardSpaceId);
        if (boardSpace == null) return;
        ClipboardColor = boardSpace.Color.Value;
    }

    [ConsoleCommand("water")]
    private void Console_SetToWater() => SetTerrain(TerrainType.Water);

    [ConsoleCommand("land")]
    private void Console_SetToLand() => SetTerrain(TerrainType.Land);
    
    [ConsoleCommand("impassable")]
    private void Console_SetToImpassable() => SetTerrain(TerrainType.Land);

    private async Task SetTerrain(TerrainType terrainType, Action onCompletion = null)
    {
        if (_selectedBoardSpace is null) return;
        await EditValue(_selectedBoardSpaceId, nameof(BoardSpace.TerrainType),
            new SyncedInt(null, null, (int)terrainType).SerializeData());
    }
}