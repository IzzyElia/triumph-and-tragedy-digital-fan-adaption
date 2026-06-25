using System.Collections.Generic;
using TT2026.libraries.NetworkedBoardGameEntitySystem;
using TT2026.libraries.NetworkedBoardGameEntitySystem.SyncedDataTypes;

namespace TT2026.Libraries.NetworkedBoardGameEntitySystem.Actions;

/// <summary>
/// This represents an option, or partially filled out option, the player may take, with methods for
/// tracking farther down or back up the graph of possibilities
/// 
/// For example, if the implementer represents an action to select which dice might be used,
/// the current object would represent a partial selection of dice, <see cref="Next"/> might represent every
/// dice combination that could be legally created by adding a single extra die, and <see cref="From"/> would
/// be the previous partial dice selection that the current partial selection was created from
/// 
/// The intention is to create a graph of possible actions and a traceable path ofhow the user reached
/// their current uncommitted plan. This allows for easy undoing of uncommitted actions, and creates
/// a graph that an AI can search using a graph search algorithm to choose its preferred action
/// without needing to examine every possibility
/// </summary>
public interface IPlayerAction : ISyncable
{
    /// <summary>
    /// Returns the action that the current action was derived from.
    /// Implementers should mark their implementation with [JsonIgnore]
    /// </summary>
    public IPlayerAction From { get; }
    /// <summary>
    /// Returns a collection of actions that can be legally derived from the current one
    /// </summary>
    public IEnumerable<IPlayerAction> Next(GameState gameState);
    /// <summary>
    /// Returns whether the action is valid, invalid,
    /// or invalid but will be valid farther down the graph
    /// </summary>
    public ActionValidationResult Validate(GameState gameState);
    /// <summary>
    /// Executes the action by modifying the target gamestate
    /// </summary>
    public void ExecuteOn(ServerGameState gameState);
    
    /// <summary>
    /// Describes this step in the graph. Used in UI contexts
    /// </summary>
    public string StepDescription(GameState gameState);
    
    /// <summary>
    /// Lists out entities worth contextually highlighting because they relate to this action.
    /// For example, a tile with units that could be moved
    /// </summary>
    public IEnumerable<int> HighlightEntities(GameState gameState);

    /// <summary>
    /// Returns true if the action is a partial duplicate of any of the other
    /// actions in <paramref name="potentialActions"></paramref>. Useful for UI
    /// </summary>
    bool DuplicatesWith(IEnumerable<IPlayerAction> potentialActions);
}

public enum ActionValidationResult
{
    /// <summary>
    /// The action may legally be executed in its current form
    /// </summary>
    Valid,
    /// <summary>
    /// The action is illegal, but farther down the graph there are legal options
    /// </summary>
    Incomplete,
    /// <summary>
    /// The action is illegal and there are no legal options anywhere farther down the graph
    /// </summary>
    Illegal,
}