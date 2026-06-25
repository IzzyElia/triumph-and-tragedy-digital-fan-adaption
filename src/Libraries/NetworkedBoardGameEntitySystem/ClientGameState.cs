using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TT2026.libraries.LiteNetLib_2._1._4.LiteNetLib;
using TT2026.Libraries.NetworkedBoardGameEntitySystem;
using TT2026.Libraries.NetworkedBoardGameEntitySystem.Actions;
using TT2026.libraries.NetworkedBoardGameEntitySystem.Networking;
using TT2026.Libraries.NetworkedBoardGameEntitySystem.Networking;
using TT2026.Libraries.NetworkedBoardGameEntitySystem.Networking.PacketTypes;
using TT2026.libraries.NetworkedBoardGameEntitySystem.Rendering;

namespace TT2026.libraries.NetworkedBoardGameEntitySystem;

public class ClientGameState : GameState
{
    public override bool IsServerSide => false;
    public Client Client;
    public override NetworkPeer NetworkManager => Client;
    public override GameRenderer Renderer => Client.Renderer;
    public int PlayerId = -1;

    public ClientGameState(Client client)
    {
        Client = client;
    }

    public async Task<bool> TrySetPlayerSlot(int desiredPlayerId)
    {
        if (Client.ConnectedServer is null) return false;
        NetworkResponse response = await Client.SendRequestAwaitCallback(Client.ConnectedServer,
            new RequestPlayerPositionPacket()
            {
                RequestedPlayerId = desiredPlayerId,
            });
        if (response.Error == NetworkResponseError.None)
        {
            PlayerId = desiredPlayerId;
            Renderer?.EntitiesChanged.Add(Constants.GameStateChangePlayerSignalId);
            Logger.Log($"Successfully set client to player {desiredPlayerId}");
            return true;
        } 
        Logger.Log($"Failed to set client to player {desiredPlayerId}");
        return false;
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

    public IEnumerable<IPlayerAction> GetPlayerActions(int playerId, IPlayerAction currentlyContemplatedAction)
    {
        // If the player is partially through doing an action (ex placing units),
        //  only show anything that graphs out from it
        // Otherwise, iterate the GameBehaviors and collect every root action they
        //  allow
        if (currentlyContemplatedAction is not null)
        {
            foreach (var next in currentlyContemplatedAction.Next(this))
            {
                yield return next;
            }
        }
        else
        {
            foreach (var activeGameBehavior in GameBehaviors.Values)
            {
                foreach (var action in activeGameBehavior.GetPotentialActions(playerId))
                {
                    yield return action;
                }
            }
        }
    }
    public async Task<bool> AttemptAction(IPlayerAction action)
    {
        // Final validation to prevent any weirdness. This should already have been done
        //  of course the server revalidates on their end as well. This just
        //  makes it clear which end the issue is on if it fails for some reason
        if ((action.Validate(this) != ActionValidationResult.Valid)) throw new ArgumentException($"Attempted to send an illegal action");
        var packet = new PlayerActionPacket(action);
        if (Client.ConnectedServer is null) return false;
        var response = await Client.SendRequestAwaitCallback(Client.ConnectedServer, packet);
        return response.Error == NetworkResponseError.None;
    }
}