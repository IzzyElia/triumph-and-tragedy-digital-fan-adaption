using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TT2026.NetworkedBoardGameEntitySystem;

public abstract class GameState
{
    public readonly object Mutex = new object();
    public static Dictionary<string, Type> KnownTypes = new();
    public static void LoadEntityTypesFromCurrentAssembly()
    {
        Logger.Log("Loading game entity types");
        foreach (var entityType in Assembly.GetCallingAssembly().GetTypes().Where(x => x.IsSubclassOf(typeof(GameEntity)) && !x.IsAbstract))
        {
            if (KnownTypes.ContainsKey(entityType.FullName)) throw new InvalidOperationException($"Duplicate game entity type: {entityType.FullName}");
            KnownTypes.Add(entityType.FullName, entityType);
        }
    }
    
    public abstract bool IsServerSide { get; }
    /// <summary>
    /// The ID of the specific step in the game's history that we are currently at. Tthis value should be ticked up with <see cref="AdvanceGamePhaseTicker"/> anytime the game progresses in any way
    /// </summary>
    public int GameStepID { get; protected set; } = 0;
    public Dictionary<int, GameEntity> EntitiesById = new Dictionary<int, GameEntity>();
}