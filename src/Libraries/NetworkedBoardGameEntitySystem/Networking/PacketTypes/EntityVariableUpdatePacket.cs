using TT2026.Libraries.NetworkedBoardGameEntitySystem.Networking;

namespace TT2026.libraries.NetworkedBoardGameEntitySystem.Networking;

public struct EntityVariableUpdatePacket(int gameStepId, GameEntity entity, string variableName, string variableValue)
: INetworkRequest
{
    public int GameStepId { get; set; } = gameStepId;
    public int EntityId { get; set; } = entity.ID;
    public string EntityTypeName { get; set; } = entity.GetType().FullName;
    public string VariableName { get; set; } = variableName;
    public string VariableValue { get; set; } = variableValue;
}

public enum EntityUpdatePacketApplyError
{
    AllGroovy,
    InvalidType,
    EntityTypeMismatch,
    InvalidVariableName,
    InvalidVariableValue,
}