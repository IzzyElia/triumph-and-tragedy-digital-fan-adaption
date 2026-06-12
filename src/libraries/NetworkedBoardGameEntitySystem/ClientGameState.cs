using System;
using System.Net.NetworkInformation;

namespace TT2026.NetworkedBoardGameEntitySystem;

public class ClientGameState : GameState
{
    public override bool IsServerSide => false;
    public IClientNetworkInterface NetworkInterface;

    public ClientGameState(IClientNetworkInterface networkInterface)
    {
        NetworkInterface = networkInterface;
    }
    
    public EntityUpdatePacketApplyError ApplyEntityUpdatePacket(EntityVariableUpdatePacket updatePacket)
    {
        if (!KnownTypes.TryGetValue(updatePacket.EntityTypeName, out var type))
        {
            return EntityUpdatePacketApplyError.InvalidType;
        }
        if (!EntitiesById.TryGetValue(updatePacket.EntityId, out GameEntity entity))
        {
            entity = (GameEntity)Activator.CreateInstance(type);
            if (entity is null) throw new InvalidOperationException($"Failed to create clientside {type.Name} object");
            entity.Setup(this, id: updatePacket.EntityId);
        }
        
        if (entity.GetType() != type) return EntityUpdatePacketApplyError.EntityTypeMismatch;
        
        return entity.TryApplyUpdatePacket(updatePacket);
    }
}