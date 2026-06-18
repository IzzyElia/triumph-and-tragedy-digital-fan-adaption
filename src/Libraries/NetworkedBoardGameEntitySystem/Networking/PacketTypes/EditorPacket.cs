using System;
using TT2026.Libraries.NetworkedBoardGameEntitySystem.Networking;

namespace TT2026.libraries.NetworkedBoardGameEntitySystem.Networking;

public struct EditorPacket(bool entityExists, int gameStepId, int entityId, Type entityType, string variableName, string variableValue, string editorAuthKey) : INetworkRequest
{
    public bool EntityExists { get; set; } = entityExists;
    public int GameStepId { get; set; } = gameStepId;
    public int EntityId { get; set; } = entityId;
    public string EntityTypeName { get; set; } = entityType?.FullName;
    public string VariableName { get; set; } = variableName;
    public string VariableValue { get; set; } = variableValue;
    public string EditorAuthenticationKey { get; set; } = editorAuthKey;

}

public struct EditorPacketResponse (int entityId)
{
    public int EntityId { get; set; } = entityId;
}