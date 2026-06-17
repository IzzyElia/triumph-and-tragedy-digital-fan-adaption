using System;
using System.Collections.Generic;
using System.Reflection;

namespace TT2026.libraries.Izzy
{
    /// <summary>
    /// Maps all types derived from <typeparamref name="T"/> in such a way that a reference to the type
    /// can be communicated using either a byte (if < 255 unique types) or a short (if > 255 types)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class IndexedTypeMap<T>
    {
        Type[] typeMap;
        Dictionary<Type, short> typeIDs = new Dictionary<Type, short>();

        public IndexedTypeMap()
        {
            Recalculate();
        }
        /// <summary>
        /// Recalculate the type map. Call this if new types have been injected since compile time
        /// </summary>
        public void Recalculate ()
        {
            SortedList<string, Type> sortedTypes = new SortedList<string, Type>();
            foreach (Type type in Assembly.GetExecutingAssembly().DefinedTypes)
            {
                if (typeof(T).IsAssignableFrom(type))
                {
                    sortedTypes.Add(type.FullName, type);
                }
            }
            int count = sortedTypes.Count;
            if (count > byte.MaxValue)
                DynamicLogger.LogWarning($"There are over {byte.MaxValue} {typeof(T).Name}'s defined ({count}). A short may be needed instead when sending {typeof(T).Name} references over the network");
            short i = 0;
            typeMap = new Type[count];
            typeIDs.Clear();
            foreach (Type type in sortedTypes.Values)
            {
                DynamicLogger.Log($"Added player action '{type.Name}' with reference index #{i}", context:"IndexedTypeMap");
                typeMap[i] = type;
                typeIDs.Add(type, i);
                i += 1;
            }
        }
        public Type GetWithByte(byte index)
        {
            if (index >= typeMap.Length)
            {
                throw new IndexOutOfRangeException($"No {typeof(T).Name} mapped to {index}");
            }
            return typeMap[index];
        }
        public Type GetWithShort(short index)
        {
            if (index >= typeMap.Length)
            {
                throw new IndexOutOfRangeException($"No {typeof(T).Name} mapped to {index}");
            }
            return typeMap[index];
        }
        public byte TypeIDAsByte(Type type)
        {
            if (typeIDs.TryGetValue(type, out short id))
            {

                if (id > byte.MaxValue)
                    throw new InvalidOperationException($"The id of the specified type cannot be held in a byte. Consider using GetIDAsShort() instead");
                return (byte)id;
            }
            else
            {
                throw new ArgumentException("The given type is not a registered player action. Is it in the same assembly as PlayerActionManager and does it derive from IPlayerAction?");
            }
        }
        public short TypeIDAsShort(Type type)
        {
            if (typeIDs.TryGetValue(type, out short id))
            {

                return id;
            }
            else
            {
                throw new ArgumentException("The given type is not a registered player action. Is it in the same assembly as PlayerActionManager and does it derive from IPlayerAction?");
            }
        }
    }
}
