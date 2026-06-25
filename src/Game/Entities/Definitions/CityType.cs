using System;
using System.Collections.Generic;
using TT2026.Game.Rendering;
using TT2026.libraries.IzzysUI.Tooltips;
using TT2026.libraries.NetworkedBoardGameEntitySystem;
using TT2026.libraries.NetworkedBoardGameEntitySystem.SyncedDataTypes;

namespace TT2026.Game.Definitions;

public class CityType : GameEntity, IDefinition
{
    public SyncedObject<CityTypeDefinition> Definition { get; set; }
    public ISyncedObject DefinitionAccessor => Definition;
    public string LinkName => Definition.Value.Name is null || Definition.Value.Name.Length == 0 ? "Unnamed City Type" : Definition.Value.Name;
    
    public CityType()
    {
        Definition = new SyncedObject<CityTypeDefinition>(this, nameof(Definition), default);
    }

    public ContextWindowInfo GetContextWindow()
    {
        if (GameState.Renderer is TTRenderer_Editor editor)
        {
            List<IUIInteraction> interactions = new();
            TTUtils.GenerateEditorActionsForDefinitionEntity(this, editor, ref interactions);
            return new ContextWindowInfo(
                source: this,
                header: $"{Definition.Value.Name}",
                interactions: interactions);
        }
        else throw new NotImplementedException();
    }
}

public struct CityTypeDefinition : ISyncable
{
    public string Name { get; set; }
    public int Population { get; set; }
    public int Muster { get; set; }
    public string IconUid { get; set; }
    public float IconScale { get; set; }
}