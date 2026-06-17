using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using TT2026.libraries.Izzy.ID_System;
using TT2026.libraries.Izzy.UnitTesting;
using TT2026.libraries.Izzy.Utils;

namespace TT2026.libraries.Izzy.Serialization;

/// <summary>
/// 
/// </summary>
/// <remarks>
/// Only fields are serialized
/// 
/// OnDeserializing is called on all objects before OnDeserialized is called on any
/// 
/// Objects are recreated using their parameterless constructor.
///     OnDeserializing is called immediately after construction
/// </remarks>
public static class Serializer
{
    const string ShorthandBlockName = "Type Shorthands";
    private const string ReferenceIdentifier = "!reference!";
    private const string PrimitiveIdentifier = "!primitive!";
    private const string BlockIdentifier = "!block!";
    private const string ArrayIdentifier = "!array!";
    private const string NullValue = "!NULL!";
    public static BlockSettings DefaultFormatting = new BlockSettings()
    {
        BlockCloser = '>',
        BlockOpener = '<',
        BlockHeaderFinalizer = ':',
        ArrayOpener = '[',
        ArrayCloser = ']',
        ArraySeperator = ',',
        ItemSeperator = ';',
        SetOperator = '=',
        StringWrapper = '"',
        EscapeChar = '\\',
        customSpecialCharacters = new []
        {
            '(',
            ')',
        }
            
    };
    public static string Serialize(object rootObject)
    {

        string serialized = string.Empty;
        SerializationProcessData serializationData = new();
        MapObjectGraphRecursively(rootObject, serializationData);

        foreach (Type type in new List<Type>(serializationData.MappedTypes))
        {
            if (type.IsConstructedGenericType)
                RecursivelyMapGenericParameterReferencedTypes(type, serializationData);
        }
            
        // Map the assembly qualified names of all types serialized to their respective shorthands
        ConstructTypeNamesDictionary(serializationData);

        Block root = new Block(DefaultFormatting);
            
        root.AddKeyedEntry("root", serializationData.ObjectIDSystem.IdOf(rootObject).ToString(), null);

        root.AddChild(ShorthandBlockName, SerializeTypeNamesDictionary(serializationData.TypeNames));
        foreach ((int id, object obj) in serializationData.ObjectIDSystem.GetAllMappedObjects())
        {
            root.AddChild(id.ToString(), SerializeObject(obj, serializationData));
        }

        return root.BuildStringRecursively(0, DefaultFormatting);
    }

        
    private static void RecursivelyMapGenericParameterReferencedTypes(Type type, SerializationProcessData serializationData)
    {
            
        Type[] genericArguments = type.GetGenericArguments();
        foreach (Type genericArgument in genericArguments)
        {
            serializationData.MappedTypes.Add(genericArgument);
            if (genericArgument.IsConstructedGenericType && !serializationData.MappedTypes.Contains(genericArgument))
                RecursivelyMapGenericParameterReferencedTypes(genericArgument, serializationData);
        }
    }
    /// <summary>
    /// Must be called after mapping types (MapObjectGraphRecursively)
    /// </summary>
    /// <param name="serializationData"></param>
    /// <exception cref="InvalidOperationException"></exception>
    private static void ConstructTypeNamesDictionary(SerializationProcessData serializationData)
    {
        Queue<Type> genericTypes = new();
        foreach (Type type in serializationData.MappedTypes)
        {
            string assemblyQualifiedName = type.AssemblyQualifiedName;
            if (assemblyQualifiedName == null)
                throw new InvalidOperationException("The assembly qualified name of the type is null. This shouldn't happen, but could be related to the type being a dynamic or generic type");
            string shorthand = null;
            SerializedAsAttribute serializedAs = type.GetCustomAttribute<SerializedAsAttribute>();
            if (serializedAs != null)
            {
                shorthand = serializedAs.SerializedShorthand;
            }
            else if (type.IsConstructedGenericType)
            {
                serializationData.TypeNameOverrides.TryGetValue(type.GetGenericTypeDefinition(), out shorthand);
            }
            else
            {
                serializationData.TypeNameOverrides.TryGetValue(type, out shorthand);
            }

            // If the type has the serialized attribute OR it is a constructed generic type of a built in type with a name override
            if (shorthand != null)
            {
                if (type.IsConstructedGenericType)
                {
                    genericTypes.Enqueue(type);
                    Type baseType = type.GetGenericTypeDefinition();
                    serializationData.TypeNames.TryAdd(baseType, shorthand);
                }
                else
                {
                    serializationData.TypeNames.Add(type, shorthand);
                }
            }
            else
            {
                serializationData.TypeNames.TryAdd(type, assemblyQualifiedName);
            }
        }

        // At this point all base types should be in the typeNames dictionary
        while (genericTypes.TryDequeue(out Type constructedGenericType))
        {
            string typeShorthand = ConstructGenericTypeShorthand(constructedGenericType, serializationData);
            serializationData.TypeNames.Add(constructedGenericType, typeShorthand);
        }
    }


    private static string ConstructGenericTypeShorthand(Type type, SerializationProcessData serializationData)
    {
        string constructedGenericTypeString = serializationData.TypeNames[type.GetGenericTypeDefinition()] + "(";
        Type[] genericArguments = type.GetGenericArguments();
        for (int i = 0; i < genericArguments.Length; i++)
        {
            Type genericArgument = genericArguments[i];
            if (genericArgument.IsConstructedGenericType)
            {
                constructedGenericTypeString += ConstructGenericTypeShorthand(genericArgument, serializationData);
            }
            else
            {
                constructedGenericTypeString += serializationData.TypeNames[genericArgument];
            }

            if (i != genericArguments.Length - 1)
                constructedGenericTypeString += ", ";
        }

        constructedGenericTypeString += ")";
        return constructedGenericTypeString;
    }

    private static Block SerializeTypeNamesDictionary(Dictionary<Type, string> typeNames)
    {
        Block typeNamesBlockArray = new (DefaultFormatting);
        foreach (KeyValuePair<Type, string> kvp in typeNames)
        {
            Type type = kvp.Key;
            string shorthand = kvp.Value;
            Type baseType;
            if (type.IsConstructedGenericType)
            {
                baseType = type.GetGenericTypeDefinition();
            }
            else
            {
                baseType = type;
            }
            typeNamesBlockArray.AddKeyedEntry(type.AssemblyQualifiedName, shorthand, null);
        }

        return typeNamesBlockArray;
    }
        
    private static void MapObjectGraphRecursively(object obj, SerializationProcessData serializationData)
    {
        Type objType = obj.GetType();
            
        if (serializationData.ObjectIDSystem.IsIdAssignedTo(obj))
            return;
            
        if (obj is Type)
            throw new NotImplementedException("Serializing a Type() object may work but has not been tested");
        // TODO --------------------------------------------------------------------------------- ^ So test it ^
            
        if (!objType.IsDefined(typeof(SerializableAttribute), false))
            throw new SerializationException($"Attempted to serialize a {objType.FullName} which is not marked as serializable");
            
        serializationData.MappedTypes.Add(objType);

        if (!objType.IsDefined(typeof(SerializeInternallyAttribute), false) && !objType.IsValueType)
        {
            serializationData.ObjectIDSystem.AssignID(obj);
        }
            
        if (obj is ISerializable)
        {
            Type underlyingType = objType.GetGenericTypeDefinition();
            if (underlyingType == typeof(Dictionary<,>))
            {
                IDictionary dictionary = obj as IDictionary;
                foreach (object key in dictionary.Keys)
                {
                    CheckAndMapValue(key, key.GetType(), serializationData);
                }

                foreach (object value in dictionary.Values)
                {
                    CheckAndMapValue(value, value.GetType(), serializationData);   
                }
            }
            else if (underlyingType == typeof(List<>) || underlyingType == typeof(HashSet<>))
            {
                IEnumerable collection = obj as IEnumerable;
                foreach (object value in collection)
                {
                    CheckAndMapValue(value, value.GetType(), serializationData);
                }
            }
            else
            {
                throw new SerializationException($"Serializer does not support the .NET type {objType}");
            }
                
            /*
                SerializationInfo serializationInfo = new SerializationInfo(objType, new FormatterConverter());
                ((ISerializable)obj).GetObjectData(serializationInfo, new StreamingContext());
                foreach (SerializationEntry entry in serializationInfo)
                {
                    if (entry.Value == null)
                        continue;
                    CheckAndMapValue(entry.Value, entry.ObjectType, serializationData);
                }
                */
        }
        else
        {
            // If the object is not an ISerializable, run the generic serialization logic
            Type derivedType = objType;
            while (derivedType != null && derivedType != typeof(object))
            {
                foreach (FieldInfo field in derivedType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    if (field.IsDefined(typeof(NonSerializedAttribute), false))
                        continue;
                    if (field.IsDefined(typeof(CompilerGeneratedAttribute)))
                        continue;
                    object value = field.GetValue(obj);
                    if (value == null)
                        continue;
                    CheckAndMapValue(value, field.FieldType, serializationData);
                }

                derivedType = derivedType.BaseType;
            }
            // TODO do properties serialize ok?
            foreach (PropertyInfo property in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                
                if (property.IsDefined(typeof(NonSerializedAttribute), false))
                    continue;
                if (property.GetMethod == null || property.SetMethod == null)
                    throw new SerializationException($"Serialized properties must have both get and set accessors (missing from {objType.FullName}.{property.Name}())");
                object value = property.GetValue(obj);
                if (value == null)
                    continue;
                CheckAndMapValue(value, property.PropertyType, serializationData);
            } 
        }
    }

    private static void CheckAndMapValue(object value, Type type, SerializationProcessData serializationData)
    {
        if (value == null) return;
        if (type.IsArray)
        {
            if (value is Array arrayObj)
            {
                foreach (object item in arrayObj)
                {
                    if (item != null && !item.GetType().IsPrimitive && item.GetType() != typeof(string))
                    {
                        MapObjectGraphRecursively(item, serializationData);
                    }
                }
            }
        }
        else if (!type.IsPrimitive && type != typeof(string))
        {
            MapObjectGraphRecursively(value, serializationData);
        }
    }
        
    private static Block SerializeObject(object obj, SerializationProcessData serializationData)
    {
        Block block = new Block(DefaultFormatting);
        Type objType = obj.GetType();

        Type derivedType = objType;
        while (derivedType != null && derivedType != typeof(Object))
        {
            foreach (MethodInfo method in derivedType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            {
                if (method.GetCustomAttribute(typeof(OnSerializingAttribute)) != null)
                {
                    method.Invoke(obj, null);
                }
            }

            derivedType = derivedType.BaseType;
        }



        block.Header = GetTypeShorthand(objType, serializationData);
            
        // If the type impliments ISerializable, it's not compatible with this library.
        // If that's the case, check if it's supported known type. If it is, serialize it manually.
        //      and if it'snot, throw an error
        if (obj is ISerializable)
        {
            Type underlyingType = objType.GetGenericTypeDefinition();
            if (underlyingType == typeof(Dictionary<,>))
            {
                IDictionary dictionary = obj as IDictionary;
                List<object> keys = new ();
                List<object> values = new();
                foreach (DictionaryEntry entry in dictionary)
                {
                    keys.Add(entry.Key);
                    values.Add((entry.Value));
                }
                SerializePropertyOrFieldValue("Keys", keys.ToArray(), block, serializationData);
                SerializePropertyOrFieldValue("Values", values.ToArray(), block, serializationData);
            }
            else if (underlyingType == typeof(List<>))
            {
                IList collection = obj as IList;
                List<object> items = new();
                foreach (object item in collection)
                {
                    items.Add(item);
                }
                SerializePropertyOrFieldValue("Items", items.ToArray(), block, serializationData);
            }
            else if (underlyingType == typeof(HashSet<>))
            {
                IEnumerable collection = obj as IEnumerable;
                List<object> items = new();
                foreach (object item in collection)
                {
                    items.Add(item);
                }
                SerializePropertyOrFieldValue("Items", items.ToArray(), block, serializationData);
            }
            /*
                BlockArray serializationOrder = new BlockArray(defaultFormatting);
                SerializationInfo serializationInfo = new SerializationInfo(objType, new FormatterConverter());
                ((ISerializable)obj).GetObjectData(serializationInfo, new StreamingContext());
                // Write the values stored in the serialization info
                foreach (SerializationEntry entry in serializationInfo)
                {
                    string metadata;
                    if (entry.ObjectType.IsArray)
                    {
                        metadata = ValueTypes.Array.ToString();
                    }
                    else if (entry.ObjectType.IsPrimitive)
                    {
                        metadata = ValueTypes.Float.ToString();
                    }
                    else if (entry.ObjectType.IsValueType)
                    {
                        metadata = ValueTypes.Object.ToString();
                    }
                    else
                    {
                        metadata = referenceIdentifier;
                    }
                    serializationOrder.AddElement(entry.Name, metadata);
                    SerializePropertyOrFieldValue(entry.Name, entry.Value, block, serializationData);
                }
                block.AddArray(serializationOrderIdentifier, serializationOrder);
                return block;
                */
        }
        else // If object does not implement ISerializable, use generic serialization logic
        {
            derivedType = objType;
            while (derivedType != null && derivedType != typeof(Object))
            {
                foreach (var field in derivedType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    if (field.IsDefined(typeof(NonSerializedAttribute), false))
                        continue;
                    if (field.IsDefined(typeof(CompilerGeneratedAttribute)))
                        continue;
                    SerializePropertyOrFieldValue(field.Name, field.GetValue(obj), block, serializationData);
                }

                derivedType = derivedType.BaseType;
            }


            /*
                 The built in serializers do not serialize properties, which makes sense
                 If we did want to serialize properties, we would need another attribute in addition to
                 NonSerialize (ex NonSerializedProperty), since NonSerialized is only applicable to fields
                 
                foreach (var property in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (property.IsDefined(typeof(NonSerializedAttribute), false))
                        continue;
                    // If the property is a value type, write it directly. Otherwise, check
                    SerializePropertyOrFieldValue(property.Name, property.GetValue(obj), block, serializationData);
                }
                */
        }
        return block;
    }

    private static BlockArray SerializeArray(Array array, SerializationProcessData serializationData)
    {
        BlockArray arrayBlock = new BlockArray(DefaultFormatting);
        foreach (object element in array)
        {
            Type elementType = element.GetType();
            if (elementType == typeof(string))
            {
                arrayBlock.AddElement(element as string, null);
            }
            else if (elementType.IsPrimitive)
            {
                byte[] data = BinaryUtils.SerializeUnmanaged_NoCheck(element);
                arrayBlock.AddElement(ByteArrayToString(data), null);
            }
            else if (elementType.IsArray)
            {
                Array subArray = element as Array;
                string subArrayElementTypeShorthand = GetTypeShorthand(elementType.GetElementType(), serializationData);
                arrayBlock.AddElement(SerializeArray(subArray, serializationData), subArrayElementTypeShorthand);
            }
            else if (elementType.IsValueType)
            {
                string elementTypeShorthand = GetTypeShorthand(element.GetType(), serializationData);
                arrayBlock.AddElement(SerializeObject(element, serializationData), elementTypeShorthand);
            }
            else if (elementType.GetCustomAttribute<SerializeInternallyAttribute>() != null)
            {
                // If a reference type with the SerializeInternally attribute
                if (!elementType.IsDefined(typeof(SerializableAttribute), false))
                    throw new SerializationException($"Type {elementType} is not serializable");

                if (serializationData.ObjectIDSystem.IsIdAssignedTo(element))
                    throw new SerializationException($"An instance of {elementType} is flagged with SerializeInternally but contains references outside of the object assigning the SerializeInternally attribute");

                arrayBlock.AddElement(SerializeObject(element, serializationData), null);
            }
            else
            {
                // If a reference
                if (!elementType.IsDefined(typeof(SerializableAttribute), false))
                    throw new SerializationException($"Type {elementType} is not serializable");
            
                IDSystem<object> idSystem = serializationData.ObjectIDSystem;
                int propertyId = idSystem.IdOf(element);
                arrayBlock.AddElement(propertyId.ToString(), ReferenceIdentifier);
            }
        }

        return arrayBlock;
    }
        
    // Should only ever return a string., a Block(), or a BlockArray()
    private static void SerializePropertyOrFieldValue(string propertyName, object propertyValue, Block block, SerializationProcessData serializationData)
    {
            
        if (propertyValue == null)
        {
            block.AddEntry(propertyName, "", NullValue);
            return;
        }

        Type propertyValueType = propertyValue.GetType();
        if (propertyValueType == typeof(string))
        {
            block.AddEntry(propertyName, propertyValue as string, null);
            return;
        }
        // Arrays apply similar logic to non-arrays
        // Changes made to how objects are serialized for one should generally be applied to the other
        else if (propertyValueType.IsArray)
        {
            // If an array
            Array array = propertyValue as Array;
            block.AddArray(propertyName, SerializeArray(array, serializationData));
            return;
        }
        else if (propertyValueType.IsPrimitive)
        {
            // If a primitive
            byte[] data = BinaryUtils.SerializeUnmanaged_NoCheck(propertyValue);
            block.AddEntry(propertyName, ByteArrayToString(data), null);
            return;
        }
        else if (propertyValueType.IsValueType)
        {
            // If a non-primitive struct
            if (!propertyValueType.IsDefined(typeof(SerializableAttribute), false))
                throw new SerializationException($"Type {propertyValueType} is not serializable");

            block.AddChild(propertyName, SerializeObject(propertyValue, serializationData));
            return;
        }
        else if (propertyValueType.GetCustomAttribute<SerializeInternallyAttribute>() != null)
        {
            // If a reference type with the SerializeInternally attribute
            if (!propertyValueType.IsDefined(typeof(SerializableAttribute), false))
                throw new SerializationException($"Type {propertyValueType} is not serializable");

            if (serializationData.ObjectIDSystem.IsIdAssignedTo(propertyValue))
                throw new SerializationException($"An instance of {propertyValueType} is flagged with SerializeInternally but contains references outside of the object assigning the SerializeInternally attribute");

            block.AddChild(propertyName, SerializeObject(propertyValue, serializationData));
            return;
        }
        else
        {
            // If a reference
            if (!propertyValueType.IsDefined(typeof(SerializableAttribute), false))
                throw new SerializationException($"Type {propertyValueType} is not serializable");
                
            IDSystem<object> idSystem = serializationData.ObjectIDSystem;
            int propertyId = idSystem.IdOf(propertyValue);
            block.AddEntry(propertyName, propertyId.ToString(), ReferenceIdentifier);
            return;
        }
    }
    private static string GetTypeShorthand(Type type, SerializationProcessData serializationData)
    {
        string typeShorthand;

        if (serializationData.TypeNames.TryGetValue(type, out typeShorthand))
        {
            return typeShorthand;
        }
        else
        {
            // If no shorthand is defined, use the assembly qualified name
            return type.AssemblyQualifiedName;
        }
    }

    public static object Deserialize(string serialized)
    {
        Block data = new Block(serialized, DefaultFormatting);
        DeserializationProcessData deserializationData = new();
        Block typeInfoBlock;
        try
        {
            typeInfoBlock = data.GetChild(ShorthandBlockName);
        }
        catch (Exception e)
        {
            deserializationData.Exceptions.Add(
                new SerializationException("Unable to find the type shorthand information"));
            typeInfoBlock = new Block(DefaultFormatting);
        }
        foreach ((string assemblyName, BlockEntry entry) in typeInfoBlock.KeyedEntries)
        {
            string shorthand = entry.Value;
            deserializationData.ReverseTypeNames.Add(shorthand, Type.GetType(assemblyName));
        }

        // Create the object graph and initialize all objects. OnDeserializing is called at this step
        FillOutObjectGraph(data, deserializationData);
            
        // For each object, fill out its fields from its serialized state. OnDeserialized is called at this step
        foreach ((string objKey, Block objBlock) in data.Children)
        {
            if (objKey == ShorthandBlockName)
                continue;
            if (objBlock.Header == null)
            {
                deserializationData.Exceptions.Add(new SerializationException($"Object {objKey} has no header"));
                continue;    
            }
            string typeKey = objBlock.Header;
            int id = int.Parse(objKey);
            object obj = deserializationData.ObjectIDSystem.WithID(id);
            try
            {
                DeserializeObject(obj, objBlock, deserializationData);
            }
            catch (SerializationException e)
            {
                deserializationData.Exceptions.Add(e);
            }
        }

        // Call OnDeserializationComplete on all created objects
        foreach ((int id, object obj) in deserializationData.ObjectIDSystem.GetAllMappedObjects())
        {
            Type derivedType = obj.GetType();
            while (derivedType != null && derivedType != typeof(Object))
            {
                foreach (MethodInfo method in derivedType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (method.GetCustomAttribute(typeof(OnDeserializationCompleteAttribute)) != null)
                    {
                        method.Invoke(obj, null);
                    }
                }

                derivedType = derivedType.BaseType;
            }
        }

        if (deserializationData.Exceptions.Count == 1)
        {
            throw deserializationData.Exceptions[0];
        }
        else if (deserializationData.Exceptions.Count > 1)
        {
            foreach (Exception exception in deserializationData.Exceptions)
            {
                DynamicLogger.LogError(exception.ToString());
            }
            throw new SerializationException("Multiple deserialization errors occured");
        }

        string rootStr = data.KeyedEntries["root"].Value;
        int rootID = int.Parse(rootStr);
        return deserializationData.ObjectIDSystem.WithID(rootID);
    }

    private static object DeserializeValue(object serializedValue, Type type, string metaData, DeserializationProcessData deserializationData)
    {
        if (serializedValue is string str)
        {
            if (metaData == ReferenceIdentifier)
            {
                return GetReference(str, deserializationData);
            }
            else if (type == typeof(string))
            {
                return str;
            }
            else
            {
                try
                {
                    byte[] entryData = StringToByteArray(str);
                    return BinaryUtils.DeserializeUnmanaged_NoCheck(entryData, type);
                }
                catch (Exception e)
                {
                    deserializationData.Exceptions.Add(new SerializationException($"Value {str} cannot be deserialized to type {type.FullName}"));
                    return default;
                }

            }
        }
        else if (serializedValue is Block block)
        {
            Type blockType = GetTypeFromName(block.Header, deserializationData.ReverseTypeNames);
            object subObject = InitializeObject(blockType, deserializationData);
            DeserializeObject(subObject, block, deserializationData);
            return subObject;
        }
        else if (serializedValue is BlockArray blockArray)
        {
            return DeserializeArray(blockArray, type, deserializationData);
        }
        else
        {
            throw new InvalidOperationException();
        }
    }
    private static Array DeserializeArray(BlockArray blockArray, Type arrayType, DeserializationProcessData deserializationData)
    {
        Array array = Array.CreateInstance(arrayType, blockArray.Elements.Length);
        for (int i = 0; i < blockArray.Elements.Length; i++)
        {
                
            object element = blockArray.Elements[i].Value;
            string metadata = blockArray.Elements[i].Metadata;
            object deserializedElement = DeserializeValue(element, arrayType, metadata, deserializationData);
            array.SetValue(deserializedElement, i);
        }

        return array;
    }
        
    private static void DeserializeObject(object obj, Block objBlock, DeserializationProcessData deserializationData)
    {
        Type objType = obj.GetType();

        if (obj is ISerializable)
        {
            Type underlyingType = objType.GetGenericTypeDefinition();
            if (underlyingType == typeof(Dictionary<,>))
            {
                IDictionary dictionary = obj as IDictionary;
                BlockArray serializedKeys = objBlock.Arrays["Keys"];
                BlockArray serializedValues = objBlock.Arrays["Values"];
                Type[] genericParameters = objType.GetGenericArguments();
                Type keyType = genericParameters[0];
                Type valueType = genericParameters[1];
                Array keysArray = DeserializeArray(serializedKeys, keyType, deserializationData);
                Array valuesArray = DeserializeArray(serializedValues, valueType, deserializationData);
                for (int i = 0; i < keysArray.Length; i++)
                {
                    dictionary.Add(keysArray.GetValue(i), valuesArray.GetValue(i));
                }
            }
            else if (underlyingType == typeof(HashSet<>))
            {
                BlockArray serializedItems = objBlock.Arrays["Items"];
                Type[] genericParameters = objType.GetGenericArguments();
                Type collectionType = genericParameters[0];
                Array itemsArray = DeserializeArray(serializedItems, collectionType, deserializationData);
                for (int i = 0; i < itemsArray.Length; i++)
                {
                    object value = itemsArray.GetValue(i);
                    objType.GetMethod("Add").Invoke(obj, new[] { value });
                }
            }
            else if (underlyingType == typeof(List<>))
            {
                IList list = obj as IList;
                BlockArray serializedItems = objBlock.Arrays["Items"];
                Type[] genericParameters = objType.GetGenericArguments();
                Type collectionType = genericParameters[0];
                Array itemsArray = DeserializeArray(serializedItems, collectionType, deserializationData);
                for (int i = 0; i < itemsArray.Length; i++)
                {
                    list.Add(itemsArray.GetValue(i));
                }
            }
            else
            {
                throw new SerializationException($"Cannot deserialize unsupported .NET type {objType}");
            }
        }
        else
        {
            Type derivedType = objType;
            while (derivedType != typeof(Object) && derivedType != null)
            {
                foreach (FieldInfo field in derivedType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    if (field.GetCustomAttribute(typeof(NonSerializedAttribute)) != null)
                        continue;
                    if (objBlock.KeyedEntries.TryGetValue(field.Name, out BlockEntry potentialNullEntry))
                    {
                        if (potentialNullEntry.Metadata == NullValue)
                        {
                            field.SetValue(obj, null);
                            continue;
                        }
                    }
                        
                    if (field.IsDefined(typeof(CompilerGeneratedAttribute)))
                        continue;

                    if (field.FieldType == typeof(string))
                    {
                        string valueStr = objBlock.KeyedEntries[field.Name].Value;
                        field.SetValue(obj, valueStr);
                    }
                    else if (field.FieldType.IsArray)
                    {
                        Type arrayType = field.FieldType.GetElementType();
                        BlockArray blockArray = objBlock.Arrays[field.Name];
                        Array deserializedArray = DeserializeArray(blockArray, arrayType, deserializationData);
                        field.SetValue(obj, deserializedArray);
                    }
                    else if (field.FieldType.IsPrimitive)
                    {
                        string valueStr = objBlock.KeyedEntries[field.Name].Value;
                        byte[] data = StringToByteArray(valueStr);
                        field.SetValue(obj, BinaryUtils.DeserializeUnmanaged_NoCheck(data, field.FieldType));
                    }
                    else if (field.FieldType.IsValueType || field.FieldType.IsDefined(typeof(SerializeInternallyAttribute)))
                    {
                        Block block = objBlock.GetChild(field.Name);
                    }
                    else
                    {
                        BlockEntry entry = objBlock.KeyedEntries[field.Name];
                        Type fieldType = field.FieldType;
                        object value = DeserializeValue(entry.Value, field.FieldType, entry.Metadata, deserializationData);
                        field.SetValue(obj, value);
                    }
                } 
                    
                foreach (PropertyInfo property in derivedType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    if (property.GetCustomAttribute(typeof(SerializedPropertyAttribute)) == null)
                        continue;
                    if (objBlock.KeyedEntries.TryGetValue(property.Name, out BlockEntry potentialNullEntry))
                    {
                        if (potentialNullEntry.Metadata == NullValue)
                        {
                            property.SetValue(obj, null);
                            continue;
                        }
                    }
                        
                    if (property.IsDefined(typeof(CompilerGeneratedAttribute)))
                        continue;

                    if (property.PropertyType == typeof(string))
                    {
                        string valueStr = objBlock.KeyedEntries[property.Name].Value;
                        property.SetValue(obj, valueStr);
                    }
                    else if (property.PropertyType.IsArray)
                    {
                        Type arrayType = property.PropertyType.GetElementType();
                        BlockArray blockArray = objBlock.Arrays[property.Name];
                        Array deserializedArray = DeserializeArray(blockArray, arrayType, deserializationData);
                        property.SetValue(obj, deserializedArray);
                    }
                    else if (property.PropertyType.IsPrimitive)
                    {
                        string valueStr = objBlock.KeyedEntries[property.Name].Value;
                        byte[] data = StringToByteArray(valueStr);
                        property.SetValue(obj, BinaryUtils.DeserializeUnmanaged_NoCheck(data, property.PropertyType));
                    }
                    else if (property.PropertyType.IsValueType || property.PropertyType.IsDefined(typeof(SerializeInternallyAttribute)))
                    {
                        Block block = objBlock.GetChild(property.Name);
                    }
                    else
                    {
                        BlockEntry entry = objBlock.KeyedEntries[property.Name];
                        Type fieldType = property.PropertyType;
                        object value = DeserializeValue(entry.Value, property.PropertyType, entry.Metadata, deserializationData);
                        property.SetValue(obj, value);
                    }
                }

                foreach (MethodInfo method in derivedType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    if (method.GetCustomAttribute(typeof(OnDeserializedAttribute)) != null)
                    {
                        method.Invoke(obj, null);
                    }
                }

                derivedType = derivedType.BaseType;
            }
        }
    }

    private static void FillOutObjectGraph(Block data, DeserializationProcessData deserializationData)
    {
        foreach ((string objKey, Block objectBlock) in data.Children)
        {
            if (objKey == ShorthandBlockName)
                continue;
                
            string typeKey = objectBlock.Header;
            object obj;
            Type objectType = GetTypeFromName(typeKey, deserializationData.ReverseTypeNames);
            try
            {
                obj = InitializeObject(objectType, deserializationData);
                foreach (MethodInfo method in objectType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (method.GetCustomAttribute(typeof(OnDeserializingAttribute)) != null)
                    {
                        method.Invoke(obj, null);
                    }
                }
            }
            catch (Exception e)
            {
                deserializationData.Exceptions.Add(e);
                continue;
            }
                
            int id;
            if (!int.TryParse(objKey, out id))
            {
                deserializationData.Exceptions.Add(new SerializationException($"object key {objKey} of type {objectType.FullName} is not a valid integer"));
                continue;
            }
                
            deserializationData.ObjectIDSystem.ManuallyAssignID(obj, id);
        }
    }

    private static object InitializeObject(Type type, DeserializationProcessData deserializationData)
    {
        ConstructorInfo constructorInfo;
        if (type.IsValueType)
        {
            return Activator.CreateInstance(type);
        }
        else
        {
            // Attempt to find a private parameterless constructor
            if (!deserializationData.ParameterlessConstructors.TryGetValue(type, out constructorInfo))
            {
                constructorInfo = type.GetConstructor(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null,
                    Type.EmptyTypes,
                    null);
                deserializationData.ParameterlessConstructors.Add(type, constructorInfo);
            }


            if (constructorInfo != null)
            {
                // If a private parameterless constructor is found, invoke it to create an instance
                return constructorInfo.Invoke(null);
            }
            else
            {
                // No suitable constructor is found
                throw new SerializationException($"No parameterless constructor found for type {type.FullName}");
            }
        }
    }

    private static object GetReference(string refString, DeserializationProcessData deserializationData)
    {
        try
        {
            int id = int.Parse(refString);
            return deserializationData.ObjectIDSystem.WithID(id);
        }
        catch (Exception e)
        {
            throw new SerializationException($"Invalid reference {refString}");
        }
    }
        
    private static Type GetTypeFromName(string name, Dictionary<string, Type> knownSerializedShorthands)
    {
        Type type;
    
        // First, try to get the Type from a defined shorthand
        if (knownSerializedShorthands.TryGetValue(name, out type))
            return type;

        // If the name is not a defined shorthand, treat it as
        // an assembly qualified name and retrieve the type using it
        type = Type.GetType(name);
        if (type != null)
            return type;
    
        // If the type is still not found, then throw an exception
        throw new ArgumentException($"The name {name} does not correspond to any known type, either as a serialized shorthand or an assembly qualified name");
    }
        
    private static string ByteArrayToString(byte[] bytes)
    {
        return Convert.ToBase64String(bytes);
    }

    private static byte[] StringToByteArray(string str)
    {
        return Convert.FromBase64String(str);
    }
        
    private static bool DirectlyImplementsInterface(Type type, Type interfaceType)
    {
        if (!interfaceType.IsInterface)
        {
            throw new ArgumentException("The second parameter must be an interface type.");
        }

        var directlyImplementedInterfaces = type.GetInterfaces()
            .Except(type.BaseType?.GetInterfaces() ?? Type.EmptyTypes);

        return directlyImplementedInterfaces.Contains(interfaceType);
    }
    private static bool ImplementsGenericInterface(Type type, Type genericInterface)
    {
        if (!genericInterface.IsInterface || !genericInterface.IsGenericTypeDefinition)
        {
            throw new ArgumentException("The provided type must be a generic interface type definition.", nameof(genericInterface));
        }

        return type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericInterface);
    }

        
    class SerializationProcessData
    {
        public HashSet<Type> MappedTypes = new();
        public IDSystem<object> ObjectIDSystem = new();
        public Dictionary<Type, string> TypeNames = new();

        public Dictionary<Type, string> TypeNameOverrides = new()
        {
            { typeof(bool), "Boolean" },
            { typeof(byte), "Byte" },
            { typeof(sbyte), "SByte" },
            { typeof(char), "Char" },
            { typeof(decimal), "Decimal" },
            { typeof(double), "Double" },
            { typeof(float), "Single" },
            { typeof(int), "Int32" },
            { typeof(uint), "UInt32" },
            { typeof(long), "Int64" },
            { typeof(ulong), "UInt64" },
            { typeof(short), "Int16" },
            { typeof(ushort), "UInt16" },
            { typeof(object), "Object" },
            { typeof(string), "String" },
            { typeof(Dictionary<,>), "Dictionary" }, // Generic Dictionary
            { typeof(HashSet<>), "HashSet" }, // Generic HashSet
            { typeof(List<>), "List" } // Generic List
        };
    }

    class DeserializationProcessData
    {
        public IDSystem<object> ObjectIDSystem = new();
        public Dictionary<string, Type> ReverseTypeNames = new();
        public Dictionary<Type, ConstructorInfo> ParameterlessConstructors = new();
        public List<Exception> Exceptions = new();
    }
        
    public enum ValueTypes
    {
        Bool,
        Byte,
        SByte,
        Char,
        Decimal,
        Double,
        Float,
        Int,
        UInt,
        Long,
        ULong,
        Short,
        UShort,
        String,
        Array,
        Reference,
        Object
    }

    class TypeWithQualifiers
    {
        public Type TypeWithoutGenerics { get; private set; }
        public Type[] Generics { get; private set; }
        public Type TypeWithGenerics { get; private set; }
        public bool IsGeneric { get; private set; }
            
            

        public TypeWithQualifiers(Type type, params Type[] genericParameters)
        {
            if (type.IsConstructedGenericType)
            {
                if (genericParameters.Length > 0)
                    throw new ArgumentException(
                        "Type is a constructed generic type. Generic parameters should not be provided");
                this.TypeWithoutGenerics = type.GetGenericTypeDefinition();
                this.Generics = type.GetGenericArguments();
                this.TypeWithGenerics = type;
                this.IsGeneric = true;
            }
            else if (type.IsGenericTypeDefinition)
            {
                this.TypeWithoutGenerics = type;
                this.Generics = genericParameters;
                this.IsGeneric = true;
                try
                {
                    this.TypeWithGenerics = type.MakeGenericType((genericParameters));
                }
                catch (ArgumentException e)
                {
                    throw new ArgumentException($"Incorrect number of generic parameters for type {TypeWithoutGenerics}");
                }
            }
            else
            {
                this.TypeWithoutGenerics = type;
                this.TypeWithGenerics = type;
                this.Generics = new Type[0];
                this.IsGeneric = false;
            }
        }

        public static implicit operator Type(TypeWithQualifiers typeWithqualifiers) => typeWithqualifiers.TypeWithGenerics;
    }
}

internal class SerializationTests
{
    [Test]
    static TestResult TestSerialization()
    {
        string serialized = Serializer.Serialize(new Master());
        return new TestResult(true, serialized);
    }

    interface ITestInterface
    {
            
    }
    [Serializable]
    class Master
    {
        private TestClass testClass1;
        public TestClass TestClass2;
        public TestClass[] TestClassArray;

        public Master()
        {
            testClass1 = new TestClass(true, "one");
            TestClass2 = new TestClass(false, "two");
            TestClassArray = new TestClass[3];
            TestClassArray[0] = new TestClass(true, "Test1");
            TestClassArray[1] = new TestClass(false, "Test2");
            TestClassArray[2] = new TestClass(true, "Test3");
        }
    }
    [Serializable]
    class TestClass : ITestInterface
    {
        private bool vibes = true;
        public bool Vibes => vibes;
        private TestStruct shoppingList;
        private ITestInterface shoppingListAsInterface;
        public int Carrots => shoppingList.carrots;

        public TestClass(bool vibes, string name)
        {
            this.vibes = vibes;
            this.shoppingList = new TestStruct(RandFi.Int(), RandFi.Float(), name);
            this.shoppingListAsInterface = new TestStruct(RandFi.Int(), RandFi.Float(), name);
        }
    }
    [Serializable]
    struct TestStruct : ITestInterface
    {
        public int carrots { get; private set; }
        private float sodaLbs;
        private string name;

        public TestStruct(int carrots, float sodaLbs, string name)
        {
            this.carrots = carrots;
            this.sodaLbs = sodaLbs;
            this.name = name;
        }
    }
}