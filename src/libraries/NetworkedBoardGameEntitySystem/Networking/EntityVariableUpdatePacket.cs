namespace TT2026.NetworkedBoardGameEntitySystem;

public struct EntityVariableUpdatePacket(int gameStepID, GameEntity entity, string variableName, string variableValue)
{
    public int GameStepId { get; set; } = gameStepID;
    public int EntityId { get; set; } = entity.ID;
    public string EntityTypeName { get; set; } = entity.GetType().FullName;
    public string VariableName { get; set; } = variableName;
    public string VariableValue { get; set; } = variableValue;
}