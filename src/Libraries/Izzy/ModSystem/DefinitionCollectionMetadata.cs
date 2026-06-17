using System;
using System.Collections.Generic;

namespace TT2026.libraries.Izzy.ModSystem
{
    public static class DefinitionCollectionMetadata
    {
        private static Dictionary<string, IDefinitionCollection> _definitionCollectionMap = new();
        public static void RegisterDefinitionCollection(IDefinitionCollection definitionCollection, Type type)
        {
            if (!_definitionCollectionMap.TryAdd(type.Name, definitionCollection))
                throw new InvalidOperationException(
                    $"Multiple attempts to initialize a definition collection under the type name '{type.Name}'");
            DynamicLogger.Log($"Registered definition type {type.Name}");
        }

        public static DefinitionCollection<T> GetDefinitionCollection<T>() where T : DefinitionType, new()
        {
            if (_definitionCollectionMap.TryGetValue(typeof(T).Name, out IDefinitionCollection definitionCollection))
            {
                return (DefinitionCollection<T>)definitionCollection;
            }
            else
            {
                throw new InvalidOperationException($"No definition collection defined for the type '{typeof(T).Name}'");
            }
            
        }
    }
}