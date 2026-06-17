using System;
using TT2026.libraries.LiteNetLib_2._1._4.LiteNetLib;
using TT2026.libraries.NetworkedBoardGameEntitySystem.Networking;
using TT2026.libraries.NetworkedBoardGameEntitySystem.Rendering;

namespace TT2026.libraries.NetworkedBoardGameEntitySystem;

public class ClientGameState : GameState
{
    public override bool IsServerSide => false;
    public Client Client;
    public override NetworkPeer NetworkManager => Client;
    public override GameRenderer Renderer => Client.Renderer;

    public ClientGameState(Client client)
    {
        Client = client;
    }
    
    public EntityUpdatePacketApplyError ApplyEntityUpdatePacket(EntityVariableUpdatePacket updatePacket)
    {
        if (!KnownEntityTypes.TryGetValue(updatePacket.EntityTypeName, out var type))
        {
            return EntityUpdatePacketApplyError.InvalidType;
        }
        if (!EntitiesById.TryGetValue(updatePacket.EntityId, out GameEntity entity))
        {
            entity = (GameEntity)Activator.CreateInstance(type);
            if (entity is null) throw new InvalidOperationException($"Failed to create clientside {type.Name} object");
            entity.Setup(this, id: updatePacket.EntityId);
            if (entity is GameBehavior behavior)
            {
                if (!GameBehaviors.TryGetValue(behavior.GetType(), out GameBehavior behaviorType))
                {
                    GameBehaviors.Add(behavior.GetType(), behavior);
                }
                else
                {
                    GameBehaviors[behavior.GetType()] = behavior;
                }
            }
            Renderer.EntitiesChanged.Add(entity.ID);
        }
        
        if (entity.GetType() != type) return EntityUpdatePacketApplyError.EntityTypeMismatch;
        
        return entity.TryApplyUpdatePacket(updatePacket);
    }
}