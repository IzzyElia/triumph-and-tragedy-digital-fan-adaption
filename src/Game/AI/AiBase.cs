using System.Collections.Generic;
using TT2026.libraries.NetworkedBoardGameEntitySystem;
using TT2026.Libraries.NetworkedBoardGameEntitySystem;
using TT2026.Libraries.NetworkedBoardGameEntitySystem.Actions;

namespace TT2026.Game.AI;

/// <summary>
/// Base for AIs that choose a move by searching the <see cref="IPlayerAction"/> graph.
/// </summary>
public abstract class AiBase
{
    /// <summary>
    /// The AI Chooses an action using a greedy best-first (A*-style) search through the
    /// potential actions graph (generated using <see cref="IPlayerAction.Next"/>).
    /// Uses <see cref="CalculateHeuristic"/> to determine which action paths to examine, and
    /// <see cref="CalculateScore"/> to apply a final score to every legal action.
    /// When the graph has been searched, or <paramref name="maxActionsConsidered"/> has been
    /// reached, the legal action with the highest score is returned (or <c>null</c> if none
    /// were found).
    /// </summary>
    /// <param name="gameState">The client-side game state the AI is examining</param>
    /// <param name="playerId">The player/faction id the AI is choosing a move for</param>
    /// <param name="maxActionsConsidered">The maximum number of actions to examine before making a choice</param>
    public IPlayerAction PickAction(ClientGameState gameState, int playerId, int maxActionsConsidered)
    {
        // Next()/Validate() read synced entity data, which the network thread can mutate
        //  concurrently, so the whole search runs under the gamestate lock.
        lock (gameState.NetworkManager.Mutex)
        {
            // Frontier ordered by heuristic, highest first. PriorityQueue is a min-heap,
            //  so we negate the heuristic to pop the most promising node first.
            var frontier = new PriorityQueue<IPlayerAction, float>();
            foreach (var root in gameState.GetPlayerActions(playerId, null))
            {
                frontier.Enqueue(root, -CalculateHeuristic(root, gameState));
            }

            IPlayerAction bestAction = null;
            float bestScore = 0f;
            bool foundAny = false;
            int considered = 0;

            // The practical action graph is a tree (Next() only ever grows the partial
            //  action), so no visited set is needed; a cyclic graph would instead rely on
            //  maxActionsConsidered to terminate the search.
            // maxActionsConsidered is ignored until at least one legal action is found, so
            //  we always return a playable move when one exists somewhere in the graph.
            while (frontier.Count > 0 && (considered < maxActionsConsidered || !foundAny))
            {
                var node = frontier.Dequeue();
                considered++;

                var result = node.Validate(gameState);

                // Illegal nodes are dead ends: don't score them and don't expand them.
                if (result == ActionValidationResult.Illegal) continue;

                if (result == ActionValidationResult.Valid)
                {
                    float score = CalculateScore(node, gameState);
                    if (!foundAny || score > bestScore)
                    {
                        bestScore = score;
                        bestAction = node;
                        foundAny = true;
                    }
                }

                // Expand both Valid and Incomplete nodes so the search can reach deeper,
                //  higher-scoring legal moves.
                foreach (var child in node.Next(gameState))
                {
                    frontier.Enqueue(child, -CalculateHeuristic(child, gameState));
                }
            }

            // The loop only stops while still failing to find a legal action when the
            //  frontier empties, i.e. the entire graph was searched without success.
            if (!foundAny)
                throw new System.InvalidOperationException(
                    $"No legal action found for player {playerId} after searching the entire action graph");

            return bestAction;
        }
    }

    protected abstract float CalculateHeuristic(IPlayerAction action, GameState gameState);
    protected abstract float CalculateScore(IPlayerAction action, GameState gameState);
}
