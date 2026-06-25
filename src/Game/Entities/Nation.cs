using System;
using System.Collections.Generic;
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

public class Nation : GameEntity, ILinkable, IDefinition
{
    public SyncedColor Color;
    [EntityIdRef] public SyncedInt FactionId;
    [EntityIdRef] public SyncedInt OccupiedById;
    [EntityIdRef] public SyncedInt ColonialOverlordId;
    public SyncedBool IsFactionMajorPower;
    public SyncedObject<NationDefinition> Definition;
    public ISyncedObject DefinitionAccessor => Definition;
    public string LinkName => Definition.Value.Name;

    public Nation()
    {
        Color = new SyncedColor(this, nameof(Color), Colors.DeepPink);
        FactionId = new SyncedInt(this, nameof(FactionId), -1);
        ColonialOverlordId = new SyncedInt(this, nameof(ColonialOverlordId), -1);
        OccupiedById = new SyncedInt(this, nameof(OccupiedById), -1);
        IsFactionMajorPower = new SyncedBool(this, nameof(IsFactionMajorPower), false);
        Definition = new SyncedObject<NationDefinition>(this, nameof(Definition), new NationDefinition());
    }
    
    public Nation ColonialOverlord => ColonialOverlordId.Value == -1 ? null : GameState.GetEntity<Nation>(ColonialOverlordId.Value);
    public Nation Occupier => OccupiedById.Value == -1 ? this : GameState.GetEntity<Nation>(OccupiedById.Value);

    public IEnumerable<BoardSpace> GetControlledBoardSpaces(bool includeColonies = true, bool includeOccupied = true, bool includeOccupiedByEnemy = false)
    {
        foreach (var boardSpace in GameState.GetEntitiesOfType<BoardSpace>())
        {
            if (boardSpace.OccupierNation == this && !includeOccupiedByEnemy) continue;
            if (boardSpace.NationId.Value == ID) yield return boardSpace;
            else if (includeColonies && boardSpace.OwnerNation?.ColonialOverlord == this) yield return boardSpace;
            else if (includeOccupied && boardSpace.OccupierNation == this) yield return boardSpace;
        }
    }
    public ContextWindowInfo GetContextWindow()
    {
        if (GameState.Renderer is TTRenderer_Editor editor)
        {
            List<IUIInteraction> interactions = new();
            TTUtils.GenerateEditorActionsForDefinitionEntity(this, editor, ref interactions);
            interactions.Add(new SimpleUIActionAsync("Rename", async () =>
            {
                object popupResponse = await IzzysUIController.OpenPopupAndGetResult(new PopupInfo()
                {
                    Header = "Country Name",
                    PopupType = PopupType.Text
                });
                var response = (string)popupResponse;
                if (response is null || response.Length == 0) return null;
                var def = Definition.Value; def.Name = response; Definition.Value = def;
                await editor.EditValue(ID, nameof(Definition),  Definition.SerializeData());
                return null;
            }));
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
                    Color color = Godot.Color.FromString(cast, Colors.Transparent);
                    color.A = 1f;
                    if (color.Equals(Colors.Transparent)) return null;
                    string serializedColor = new SyncedColor(null, null, color).SerializeData();
                    await editor.EditValue(ID, nameof(Color), serializedColor);
                }
                catch (OperationCanceledException)
                {
                    
                }
                return null;
            }));
            return new ContextWindowInfo(
                source:this,
                header: LinkName,
                text: "",
                interactions: interactions
            );
        }

        return null;
    }
}

public struct NationDefinition : ISyncable
{
    public NationDefinition()
    {
    }

    public string Name { get; set; } = "Unnamed Country";
    public string AdjectiveName { get; set; } = "MISSING ADJECTIVE";
    public string Tag { get; set; } = "MISSING TAG";
    public int MaxUnitPips { get; set; } = 3;
    [EntityIdRef] public int[] Cores { get; set; } = Array.Empty<int>();
}