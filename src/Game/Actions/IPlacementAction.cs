using TT2026.Libraries.NetworkedBoardGameEntitySystem.Actions;

namespace TT2026.Game.Actions;

public interface IPlacementAction : IPlayerAction
{
    public int BoardSpaceId { get; }
}