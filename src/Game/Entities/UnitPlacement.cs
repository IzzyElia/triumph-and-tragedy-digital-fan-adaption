using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using TT2026.Game.Definitions;
using TT2026.Game.Rendering;
using TT2026.libraries.IzzysUI;
using TT2026.Libraries.IzzysUI.Popups;
using TT2026.libraries.IzzysUI.Tooltips;
using TT2026.libraries.NetworkedBoardGameEntitySystem;
using TT2026.Libraries.NetworkedBoardGameEntitySystem;
using TT2026.libraries.NetworkedBoardGameEntitySystem.SyncedDataTypes;

namespace TT2026.Game.Entities;

/// <summary>
/// Defines initial unit placements on top of (or subtracting from) the base muster value
/// </summary>
public class UnitPlacement : GameEntity, ILinkable
{
    public string LinkName
    {
        get
        {
            string nationAdjective = NationId.Value == -1 ? "Undefined Nation" : GameState.GetEntity<Nation>(NationId.Value).Definition.Value.AdjectiveName;
            string numName = StartingPips.Value.ToString();
            return $"Initial Placement: {numName} {nationAdjective} pips";
        }
    }

    [EntityIdRef] public SyncedInt NationId;
    [EntityIdRef] public SyncedInt BoardSpaceId;
    public SyncedInt StartingPips;
    
    public UnitPlacement()
    {
        NationId = new SyncedInt(this, nameof(NationId), -1);
        BoardSpaceId = new SyncedInt(this, nameof(BoardSpaceId), -1);
        StartingPips = new SyncedInt(this, nameof(StartingPips), 1);
    }
    
        public ContextWindowInfo GetContextWindow()
    {
        List<IUIInteraction> interactions = new List<IUIInteraction>();
        if (GameState.Renderer is TTRenderer_Editor editor)
        {
            interactions.Add(new SimpleUIActionAsync("Set Nation", async () =>
            {
                await TTUtils.PopupSearchEntity<Nation>(GameState, "Nation", x => x.Definition.Value.Name);
                List<string> l = new();
                await editor.EditValue(ID, nameof(NationId),  NationId.SerializeData());
                return null;
            }));
            interactions.Add(new SimpleUIActionAsync("Set Starting Pips", async () =>
            {
                try
                {
                    object result = await IzzysUIController.OpenPopupAndGetResult(new PopupInfo()
                    {
                        Header = "Color",
                        PopupType = PopupType.Text,
                    });
                    string cast = (string)result;
                    if (!int.TryParse(cast, out int value)) return null;
                    string serializedValue = new SyncedInt(null, null, value).SerializeData();
                    await editor.EditValue(ID, nameof(StartingPips), serializedValue);
                }
                catch (OperationCanceledException)
                {
                    
                }
                return null;
            }));
        }

        return new ContextWindowInfo(
            source:this,
            header: LinkName,
            text: "",
            interactions: interactions
            );
    }
}