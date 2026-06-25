using TT2026.libraries.IzzysUI.Tooltips;
using TT2026.libraries.NetworkedBoardGameEntitySystem;
using TT2026.libraries.NetworkedBoardGameEntitySystem.SyncedDataTypes;

namespace TT2026.Game.Definitions;

public interface IDefinition : ILinkable, IGameEntity
{
    public ISyncedObject DefinitionAccessor { get; }
}