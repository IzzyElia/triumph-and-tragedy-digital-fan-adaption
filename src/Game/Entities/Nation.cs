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
    public SyncedString Name;
    
    public SyncedColor Color;
    public string LinkName => Name.Value;

    public Nation()
    {
        Name = new SyncedString(this, nameof(Name), "Unnamed Country");
        Color = new SyncedColor(this, nameof(Color), Colors.DeepPink);
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
                await editor.EditValue(ID, nameof(Name),  (string)popupResponse);
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