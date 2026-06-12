using System.Collections.Generic;

namespace TT2026.NetworkedBoardGameEntitySystem;

public class ServerGameState : GameState
{
    public override bool IsServerSide => true;
    public IServerNetworkInterface NetworkInterface;
    /// <summary>
    /// A list of changes that have happened in the gamestate that still need to be pushed to clients
    /// </summary>
    private int _idTicker;
    
    public ServerGameState(IServerNetworkInterface networkInterface)
    {
        NetworkInterface = networkInterface;
    }
    
    public T InstantiateGameEntity<T>() where T : GameEntity, new()
    {
        GameEntity entity = new T();
        int id = _idTicker++;
        entity.Setup(this, id);
        return (T)entity;
    }
    
    public void AdvanceGamePhaseTicker()
    {
        foreach (var entity in EntitiesById.Values)
        {
            entity.CommitState();
        }
        GameStepID++;
    }
}