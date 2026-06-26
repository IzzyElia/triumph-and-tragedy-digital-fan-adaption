using System;
using TT2026.libraries.NetworkedBoardGameEntitySystem;
using TT2026.Libraries.NetworkedBoardGameEntitySystem.Actions;

namespace TT2026.Game.AI;

/// <summary>
/// An AI that assigns completely random heuristics and scores, so its search wanders the
/// action graph and picks an arbitrary legal action. Designed for debugging by forcing the
/// game to play random legal actions.
/// </summary>
public class AiRandom : AiBase
{
    private readonly Random _random = new Random();
    protected override float CalculateHeuristic(IPlayerAction action, GameState gameState) => (float)_random.NextDouble();
    protected override float CalculateScore(IPlayerAction action, GameState gameState) => (float)_random.NextDouble();
}
