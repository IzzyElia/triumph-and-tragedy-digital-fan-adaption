using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TT2026.libraries.Izzy;
using TT2026.Libraries.NetworkedBoardGameEntitySystem.Actions;
using TT2026.libraries.NetworkedBoardGameEntitySystem.Networking;
using TT2026.libraries.NetworkedBoardGameEntitySystem.Rendering;

namespace TT2026.libraries.NetworkedBoardGameEntitySystem;

public abstract class GameState
{
    public static Dictionary<string, Type> KnownEntityTypes = new();
    public static Dictionary<string, Type> KnownBehaviorTypes = new();
    public static Dictionary<string, Type> KnownPlayerActionTypes = new();
    public static HashSet<Assembly> LoadedAssemblies = new();
    public static void LoadTypesFromCurrentAssembly()
    {
        Assembly assembly = Assembly.GetCallingAssembly();
        void LoadTypeInto<T>(Dictionary<string, Type> into)
        {
            foreach (var type in assembly.GetTypes().Where(x => typeof(T).IsAssignableFrom(x) && !x.IsAbstract))
            {
                if (type.FullName is null) continue;
                if (into.ContainsKey(type.FullName)) throw new InvalidOperationException($"Duplicate type {type.FullName}");
                into.Add(type.FullName, type);
                Logger.Log($"Gamestate accepts type {type.FullName}");
            }
        }
        
        if (LoadedAssemblies.Contains(assembly)) {Logger.Log("Duplicate assembly loaded"); return;}
        Logger.Log("Loading game entity types");
        LoadTypeInto<GameEntity>(KnownEntityTypes);
        LoadTypeInto<GameBehavior>(KnownBehaviorTypes);
        LoadTypeInto<IPlayerAction>(KnownPlayerActionTypes);
        LoadedAssemblies.Add(assembly);
    }
    
    public abstract bool IsServerSide { get; }
    public abstract NetworkPeer NetworkManager { get; }
    public abstract GameRenderer Renderer { get; }
    /// <summary>
    /// The ID of the specific step in the game's history that we are currently at. Tthis value should be ticked up with <see cref="AdvanceGamePhaseTicker"/> anytime the game progresses in any way
    /// </summary>
    public int GameStepID { get; set; } = 0;
    public Dictionary<int, GameEntity> EntitiesById = new ();
    public HashsetDictionary<Type, GameEntity> EntitiesByType = new ();
    public Dictionary<Type, GameBehavior> GameBehaviors = new();

    public GameState()
    {
        void RecursivelyAddBaseTypes(Type type)
        {
            EntitiesByType.EnsureKey(type);
            if (type.IsSubclassOf(typeof(GameEntity))) RecursivelyAddBaseTypes(type.BaseType);
        }
        
        EntitiesByType.EnsureKey(typeof(GameEntity));
        foreach (Type type in KnownEntityTypes.Values) RecursivelyAddBaseTypes(type);
    }

    public GameEntity GetEntity(int id)
    {
        if (id == -1) return null;
        return EntitiesById[id];
    }
    
    public T GetEntity<T>(int id) where T : GameEntity
    {
        if (id == -1) return null;
        if (!EntitiesById.TryGetValue(id, out var entity)) return null;
        return entity as T;
    }

    public IEnumerable<T> GetEntitiesOfType<T>() where T : GameEntity
    {
        foreach (var entity in EntitiesByType.Get(typeof(T))) yield return (T)entity;
    }

    public T GetGameBehavior<T>() where T : GameBehavior
    {
        if (GameBehaviors.TryGetValue(typeof(T), out GameBehavior value)) return (T)value;
        else return null;
    }
}