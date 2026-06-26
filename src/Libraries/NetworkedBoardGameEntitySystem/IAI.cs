using TT2026.Libraries.NetworkedBoardGameEntitySystem.Actions;

namespace TT2026.Libraries.NetworkedBoardGameEntitySystem;

public interface IAI
{
    public IPlayerAction PickPlayerAction();
}