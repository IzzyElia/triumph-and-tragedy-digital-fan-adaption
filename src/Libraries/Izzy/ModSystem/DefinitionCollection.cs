using System;
using System.Collections.Generic;

namespace TT2026.libraries.Izzy.ModSystem
{
    public interface IDefinitionCollection
    {
        public Type Type { get; }
    }
    public class DefinitionCollection<T> : IDefinitionCollection where T : DefinitionType, new()
    {
        const string debuggingLogContext = "DefinitionCollection_debugging";

        bool _firstLoad = true;
        T[] _definitionMap;
        Dictionary<string, T> _definitionMapByName = new Dictionary<string, T>();
        string _definitionKey;
        T _fallbackDefinition;
        public Type Type { get; private set; }
        public bool StateLocked { get; private set; } = false;
        public DefinitionCollection(string typeKey)
        {
            Type = typeof(T);
            DynamicLogger.Log($"Registering definition type {Type.Name} under the key {typeKey}");
            _definitionKey = typeKey;
            DefinitionCollectionMetadata.RegisterDefinitionCollection(this, typeof(T));
            ModHandler.OnUnloadingMods += Unload;
            ModHandler.RebuildDynamicDefinitions += Rebuild;
            if (ModHandler.Loaded)
                Rebuild();
        }
        void Unload ()
        {
            StateLocked = false;
        }
        void Rebuild ()
        {
            if (StateLocked) return; // Rebuild was already triggered (through Ensure())
            
            T fallback = new T();
            fallback.SetupFallback();
            _fallbackDefinition = fallback;
            GameData[] definitions = GameData.GetAllOfType(_definitionKey);
            _definitionMap = new T[definitions.Length];
            _definitionMapByName.Clear();
            if (_firstLoad)
            {
                for (int i = 0; i < definitions.Length; i++)
                {
                    GameData definition = definitions[i];
                    T definitionObj = new T();
                    definitionObj.DoSetup(definition, i);
                    _definitionMap[i] = definitionObj;
                    _definitionMapByName.Add(definitionObj.Name, definitionObj);
                    DynamicLogger.Log($"Loaded {typeof(T).Name} {definition.Name} with the id {i}", context:debuggingLogContext);
                }
            }
            else // If reloading data, take care to preserve existing indices
            {
                Dictionary<string, int> previousIndices = new Dictionary<string, int>();
                Queue<int> freeIndices = new Queue<int>();
                foreach (T definitionObj in _definitionMap)
                {
                    previousIndices.Add(definitionObj.Name, definitionObj.id);
                }
                for (int i = 0; i < definitions.Length; i++)
                {
                    GameData definition = definitions[i];
                    if (previousIndices.TryGetValue(definition.Name, out int id))
                    {
                        T definitionObj = new T();
                        definitionObj.DoSetup(definition, id);
                        _definitionMap[id] = definitionObj;
                        _definitionMapByName.Add(definitionObj.Name, definitionObj);
                        DynamicLogger.Log($"Loaded {typeof(T).Name} {definition.Name} with the id {id}", context: debuggingLogContext);
                    }
                    else
                    {
                        freeIndices.Enqueue(i);
                    }
                }
                for (int i = 0; i < definitions.Length; i++)
                {
                    GameData definition = definitions[i];
                    int id = freeIndices.Dequeue();
                    T definitionObj = new T();
                    definitionObj.DoSetup(definition, id);
                    _definitionMap[id] = definitionObj;
                    _definitionMapByName.Add(definitionObj.Name, definitionObj);
                    DynamicLogger.Log($"Loaded {typeof(T).Name} {definition.Name} with the id {id}", context: debuggingLogContext);
                }
            }
            OnBuilt();
            StateLocked = true;
        }

        public void Ensure()
        {
            if (!StateLocked) Rebuild();
        }
        protected virtual void OnBuilt() {}
        public T Get (int index)
        {
            if (index < 0 || index >= _definitionMap.Length)
                throw new ArgumentException($"No {_definitionKey} with id {index}");
            else
                return _definitionMap[index];
        }
        public T Get (string name)
        {
            if (_definitionMapByName.TryGetValue(name, out T definitionObj))
            {
                return definitionObj;
            }
            else
            {
                DynamicLogger.LogWarning($"No definition with key {name} defined for type {typeof(T).Name}. Using fallback");
                return _fallbackDefinition;
            }
        }
        public bool TryGet (string name, out T o)
        {
            if (name is null)
            {
                o = null;
                return false;
            }
            return _definitionMapByName.TryGetValue(name, out o);
        }
        public T[] All
        {
            get => _definitionMap;
        }

        /*
        public int AddDynamic(T generatedDefinitionType)
        {
            if (_definitionMapByName.ContainsKey(generatedDefinitionType.Name)) throw new InvalidOperationException($"{typeof(T).Name} Definitions already contains a definition named {generatedDefinitionType}");
            T[] newDefinitionMap = new T[_definitionMap.Length + 1];
            _definitionMap.CopyTo(newDefinitionMap, 0);
            newDefinitionMap[^1] = generatedDefinitionType;
            _definitionMap = newDefinitionMap;
            _definitionMapByName.Add(generatedDefinitionType.Name, generatedDefinitionType);
            _generatedDefinitionTypes.Add(generatedDefinitionType);
            return _definitionMap.Length - 1;
        }
        */
    }
}
