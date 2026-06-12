namespace TT2026.NetworkedBoardGameEntitySystem;

public interface IServerNetworkInterface
{
    public void PushVariableChange(EntityVariableUpdatePacket entityVariableUpdatePacket);
}