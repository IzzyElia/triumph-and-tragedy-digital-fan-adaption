using System;
using System.Collections.Generic;
using TT2026.Game.Rendering;
using TT2026.libraries.IzzysUI;
using TT2026.Libraries.IzzysUI.Popups;
using TT2026.libraries.IzzysUI.Tooltips;
using TT2026.libraries.NetworkedBoardGameEntitySystem;
using TT2026.libraries.NetworkedBoardGameEntitySystem.SyncedDataTypes;

namespace TT2026.Game.Definitions;

public class UnitType : GameEntity, IDefinition
{
    public SyncedObject<UnitTypeDefinition> Definition { get; set; }
    public ISyncedObject DefinitionAccessor => Definition;
    public string LinkName => Definition.Value.Name is null || Definition.Value.Name.Length == 0 ? "Unnamed Unit Type" : Definition.Value.Name;
    
    public UnitType()
    {
        Definition = new SyncedObject<UnitTypeDefinition>(this, nameof(Definition), default);
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

public struct UnitTypeDefinition : ISyncable
{
    public string Name { get; set; }
    public string PluralName { get; set; }
    public string IconUid { get; set; }
    public int UnitClassId { get; set; }
    public int AirAttack { get; set; }
    public int GroundAttack { get; set; }
    public int SeaAttack { get; set; }
    public int SubAttack { get; set; }
    public int BaseMovement { get; set; }
    public bool AllowRebasing { get; set; }
    public bool IsConvoy { get; set; }
    public bool AllowPlacementWithoutSupply { get; set; }
    public bool AllowPlacementInOccupiedTerritory { get; set; }
    public int MaxPerTile { get; set; }
    public bool MayBePlaced { get; set; }
}

public enum UnitClass
{
    Undefined,
    Air,
    Ground,
    Sea,
    Submarine,
}