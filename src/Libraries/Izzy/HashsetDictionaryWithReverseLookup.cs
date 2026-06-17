using System;
using System.Collections.Generic;
using TT2026.libraries.Izzy.Serialization;
using TT2026.libraries.Izzy.UnitTesting;

namespace TT2026.libraries.Izzy;

/// <summary>
/// A generic class for managing a dictionary with any number of values per key, where each value can only be associated with a single key
/// </summary>
[Serializable]
[SerializedAs("HashsetDictionaryWithReverseLookup")]
public class AssociationCollection<TKey, TValueType>
{
	[SerializeInternally] Dictionary<TKey, HashSet<TValueType>> _dict = new();
	[SerializeInternally] Dictionary<TValueType, TKey> _reverseDict = new();
	
	public IEnumerable<TValueType> Values
	{
		get
		{
			foreach(HashSet<TValueType> hashSet in _dict.Values)
			{
				foreach(TValueType value in hashSet)
				{
					yield return value;
				}
			}
		}
	}
	public IEnumerable<TKey> Keys => _dict.Keys;

	public void EnsureKey(TKey key)
	{
		if(!_dict.ContainsKey(key))
		{
			_dict.Add(key, new HashSet<TValueType>());
		}
	}
	
	public void Set(TKey key, TValueType value)
	{
		if (_reverseDict.TryGetValue(value, out var oldKey))
		{
			if (EqualityComparer<TKey>.Default.Equals(oldKey, key)) return;
			_dict[oldKey].Remove(value);
			if (_dict[oldKey].Count == 0) _dict.Remove(oldKey);
			_reverseDict[value] = key;
		}
		else
		{
			_reverseDict.Add(value, key);
		}
		
		if (_dict.TryGetValue(key, out HashSet<TValueType> values))
		{
			values.Add(value);
		}
		else
		{
			HashSet<TValueType> newValueSet = new HashSet<TValueType>();
			newValueSet.Add(value);
			_dict.Add(key, newValueSet);
		}
	}
	
	public void Remove(TValueType value)
	{
		if (_reverseDict.TryGetValue(value, out TKey key))
		{
			HashSet<TValueType> values = _dict[key];
			values.Remove(value);
			_reverseDict.Remove(value);
			if (values.Count == 0) 
			{
				_dict.Remove(key);
			}
		}
	}
	
	public void Clear ()
	{
		_dict.Clear();
		_reverseDict.Clear();
	}
	
		
	public IEnumerable<TValueType> GetValuesOfKey (TKey key)
	{
		if (_dict.TryGetValue(key, out HashSet<TValueType> values))
		{
			return values;
		}
		else
		{
			return Array.Empty<TValueType>();
		}
	}

	public TKey GetKeyOfValue(TValueType value, TKey fallback)
	{
		if (_reverseDict.TryGetValue(value, out TKey key))
		{
			return key;
		}
		else 
		{
			return fallback;
		}
	}
	
	public int Count
	{
		get
		{
			return _reverseDict.Count;
		}
	}

	public int CountValuesOfKey(TKey key)
	{
		if (_dict.TryGetValue(key, out HashSet<TValueType> values))
		{
			return values.Count;
		}

		return 0;
	}
	
	public bool Contains (TValueType value)
	{
		return _reverseDict.ContainsKey(value);
	}
}


# if DEBUG
public class AssociationCollectionTests
{
	
}

#endif