using System;
using System.Collections.Generic;
using Godot;
using TT2026.Game.Rendering;
using TT2026.libraries.IzzysUI;
using TT2026.Libraries.IzzysUI.Popups;
using TT2026.libraries.IzzysUI.Tooltips;
using TT2026.libraries.NetworkedBoardGameEntitySystem;
using TT2026.libraries.NetworkedBoardGameEntitySystem.SyncedDataTypes;

namespace TT2026.Game.Entities;

public class Nation : GameEntity, IContextWindowCreator, ILinkable
{
    public SyncedColor Color;
    public SyncedInt FactionId;
    public SyncedObject<NationDefinition> Definition;
    public string LinkName => Definition.Value.Name;

    public Nation()
    {
        Color = new SyncedColor(this, nameof(Color), Colors.DeepPink);
        FactionId = new SyncedInt(this, nameof(FactionId), 0);
        Definition = new SyncedObject<NationDefinition>(this, nameof(Definition), new NationDefinition());
    }
    public ContextWindowInfo GetContextWindow()
    {
        List<IUIInteraction> interactions = new List<IUIInteraction>();
        if (GameState.Renderer is TTRenderer_Editor editor)
        {
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
        }

        return new ContextWindowInfo(
            source:this,
            header: LinkName,
            text: "",
            interactions: interactions
            );
    }
}

public struct NationDefinition : ISyncable
{
    public NationDefinition()
    {
    }

    public struct InitialUnitDefinition
    {
        public int BoardSpaceId { get; set; }
        public int StartingPips { get; set; }
    }

    public string Name { get; set; } = "Unnamed Country";
    public string AdjectiveName { get; set; } = "MISSING ADJECTIVE";
    public string Tag { get; set; } = "MISSING TAG";
    public int[] Cores { get; set; } = Array.Empty<int>();
    public InitialUnitDefinition[] InitialUnits { get; set; } = Array.Empty<InitialUnitDefinition>();
}