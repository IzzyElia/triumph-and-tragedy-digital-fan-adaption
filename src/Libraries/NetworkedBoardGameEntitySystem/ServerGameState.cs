using System;
using System.Collections.Generic;
using TT2026.libraries.NetworkedBoardGameEntitySystem.Networking;
using TT2026.Libraries.NetworkedBoardGameEntitySystem.Networking;
using TT2026.libraries.NetworkedBoardGameEntitySystem.Rendering;

namespace TT2026.libraries.NetworkedBoardGameEntitySystem;

public class ServerGameState : GameState
{
    public override bool IsServerSide => true;
    public Server Server;
    public override NetworkPeer NetworkManager => Server;
    public override GameRenderer Renderer => null;

    /// <summary>
    /// A list of changes that have happened in the gamestate that still need to be pushed to clients
    /// </summary>
    private int _idTicker;
    
    public ServerGameState(Server server)
    {
        Server = server;
    }

    public void SetEnabledGameBehaviors(params Type[] behaviors)
    {
        lock (Server.Mutex)
        {
            Logger.Warn($"Setting game behaviors. This will override existing data stored in current game behaviors");
            GameBehaviors.Clear();
            foreach (var behaviorType in behaviors)
            {
                GameBehavior gameBehavior;
                try
                {
                    gameBehavior = (GameBehavior)InstantiateGameEntity(behaviorType);
                    GameBehaviors.Add(behaviorType, gameBehavior);
                }
                catch (InvalidCastException)
                {
                    Logger.Error($"Failed to enable {behaviorType.Name} on the server because does not derive from {nameof(GameBehavior)}");
                    continue;
                }
            }
        }
    }
    
    public T InstantiateGameEntity<T>() where T : GameEntity, new()
    {
        GameEntity entity = new T();
        int id = _idTicker++;
        entity.Setup(this, id);
        entity.CommitState();
        return (T)entity;
    }
    
    public GameEntity InstantiateGameEntity(Type entityType, int id = -2)
    {
        if (!typeof(GameEntity).IsAssignableFrom(entityType)) throw new ArgumentException($"The type {entityType} is not assignable to {typeof(GameEntity)}");
        GameEntity entity = Activator.CreateInstance(entityType) as GameEntity;
        if (id == -2) id = _idTicker++;
        else if (id < 0) throw new ArgumentException($"Invalid game entity id {id}");
        else if (EntitiesById.ContainsKey(id)) throw new InvalidOperationException($"Entity with id {id} already exists");
        entity.Setup(this, id);
        entity.CommitState();
        return entity;
    }
    
    /// <summary>Because each entity also stores its own history, entities should never be destroyed in any way, only flagged as being in a dead state</summary>
    public void DestroyGameEntity () {}
    
    public void AdvanceGamePhaseTicker()
    {
        foreach (var entity in EntitiesById.Values)
        {
            entity.CommitState();
        }
        GameStepID++;
        Server.PushUpdate(new SetStepPacket(GameStepID));
    }

    public EditorPacketResponse HandleEditorPacket(EditorPacket editorPacket)
    {
        if (!editorPacket.EntityExists)
        {
            if (editorPacket.EntityId >= 0)
            {
                EntitiesById.Remove(editorPacket.EntityId);
                return new EditorPacketResponse(entityId: -1);
            }
            else throw new ArgumentException($"Deleting an entity ({nameof(EditorPacket)}.{nameof(editorPacket.EntityExists)}) requires providing an entity id");
        }

        GameEntity entity;
        if (editorPacket.EntityId == -2 || !EntitiesById.TryGetValue(editorPacket.EntityId, out entity))
        {
            Type type;
            try
            {
                type = KnownEntityTypes[editorPacket.EntityTypeName];
            }
            catch (KeyNotFoundException)
            {
                throw new BadPacketException($"Invalid entity type {editorPacket.EntityTypeName}");
            }
            
            entity = InstantiateGameEntity(type, editorPacket.EntityId);
        }
        
        if (editorPacket.VariableName is not null)
            entity.ForceSetValue(editorPacket.VariableName, editorPacket.VariableValue, gameStepId:0);
        
        Logger.Log($"Successfully applied edit");
        return new EditorPacketResponse(entityId: entity.ID);
    }
}