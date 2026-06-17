using System;
using System.Collections.Generic;
using TT2026.libraries.Izzy.Serialization;

namespace TT2026.libraries.Izzy.ID_System;

/// <summary>
/// Represents a system for uniquely identifying objects of type <typeparamref name="T"/> by assigning them integer IDs.
/// </summary>
[Serializable]
[SerializedAs("IDSystem")]
public class IDSystem<T> where T : class
{
    // Maps integer IDs to objects.
    private Dictionary<int, T> idMap = new Dictionary<int, T>();
    // Maps objects to their assigned integer IDs.
    private Dictionary<T, int> reverseIdMap = new Dictionary<T, int>();
    // Stores all used IDs to prevent reassignment.
    private HashSet<int> usedIDs = new HashSet<int>();
    // Holds the next available ID for assignment.
    private int nextAvailableID = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="IDSystem{T}"/> class.
    /// </summary>
    public IDSystem()
    {

    }

    /// <summary>
    /// Clears all assignments and resets the IDSystem to its initial state.
    /// </summary>
    public void Flush()
    {
        usedIDs.Clear();
        idMap.Clear();
        reverseIdMap.Clear();
        nextAvailableID = 0;
    }

    /// <summary>
    /// Automatically assigns an available ID to the given object.
    /// </summary>
    /// <param name="obj">The object to assign an ID to.</param>
    /// <returns>The assigned ID.</returns>
    public int AssignID(T obj) => ManuallyAssignID(obj, nextAvailableID);

    public int GetOrAssignID(T obj)
    {
        if (IsIdAssignedTo(obj))
        {
            return IdOf(obj);
        }
        else
        {
            return AssignID(obj);
        }
    }

    /// <summary>
    /// Force-assigns a specified ID to the given object
    /// </summary>
    /// <param name="obj">The object to assign an ID to.</param>
    /// <param name="id">The ID to assign.</param>
    /// <returns>The assigned ID.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the provided ID is already in use.</exception>
    public int ManuallyAssignID(T obj, int id)
    {
        lock (this)
        {
            if (usedIDs.Contains(id)) throw new InvalidOperationException("ID Already in use");
            usedIDs.Add(id);
            idMap.Add(id, obj);
            reverseIdMap.Add(obj, id);
            if (id >= nextAvailableID)
                nextAvailableID = id + 1;
        }
        return id;
    }

    /// <summary>
    /// Unassigns the provided ID, removing its association with any object.
    /// </summary>
    /// <param name="id">The ID to unassign.</param>
    public void UnassignID(int id)
    {
        lock (this)
        {
            reverseIdMap.Remove(idMap[id]);
            usedIDs.Remove(id);
            idMap.Remove(id);
        }
    }

    /// <summary>
    /// Gets the object associated with the provided ID.
    /// </summary>
    /// <param name="id">The ID to get the associated object of.</param>
    /// <returns>The associated object.</returns>
    public T WithID(int id)
    {
        return idMap[id];
    }

    /// <summary>
    /// Gets the ID assigned to the provided object.
    /// </summary>
    /// <param name="obj">The object to get the assigned ID of.</param>
    /// <returns>The assigned ID.</returns>
    public int IdOf(T obj)
    {
        try
        {
            return reverseIdMap[obj];
        }
        catch (KeyNotFoundException e)
        {
            throw new InvalidOperationException("No ID has been assigned to the object");
        }
    }

    public (int, T)[] GetAllMappedObjects()
    {
        (int, T)[] array = new (int, T)[idMap.Keys.Count];
        int i = 0;
        foreach (int id in idMap.Keys)
        {
            array[i] = (id, WithID(id));
            i++;
        }

        return array;
    }
    public bool IsIdAssignedTo(T obj)
    {
        return reverseIdMap.ContainsKey(obj);
    }

    /// <summary>
    /// Gets the object associated with the provided ID, or null if no such object exists.
    /// </summary>
    /// <param name="id">The ID to get the associated object of.</param>
    /// <returns>The associated object, or null if no such object exists.</returns>
    public T WithIdOrNull(int id)
    {
        if (idMap.TryGetValue(id, out T value))
            return value;
        else
            return null;
    }
}