using System.Collections.Generic;

namespace IzzysConsole.Utils
{
	/// <summary>
	/// A generic class for managing a dictionary that stores a collection of values in each key
	/// </summary>
	[System.Serializable]
	public class HashsetDictionary<TKey, TValueType>
	{
		Dictionary<TKey, HashSet<TValueType>> dictionary;
		public HashsetDictionary() 
		{
			dictionary = new Dictionary<TKey, HashSet<TValueType>>();
		}
		public IReadOnlyCollection<TValueType> Values
		{
			get
			{
				List<TValueType> values = new List<TValueType>();
				foreach(HashSet<TValueType> hashSet in dictionary.Values)
				{
					foreach(TValueType value in hashSet)
					{
						values.Add(value);
					}
				}
				return values.ToArray();
			}
		}
		public IReadOnlyCollection<TKey> Keys => dictionary.Keys;
		public void EnsureKey(TKey key)
		{
			if(!dictionary.ContainsKey(key))
			{
				dictionary.Add(key, new HashSet<TValueType>());
			}
		}
		public void Add(TKey key, TValueType value)
		{
			if (dictionary.TryGetValue(key, out HashSet<TValueType> values))
			{
				values.Add(value);
			}
			else
			{
				HashSet<TValueType> newValueSet = new HashSet<TValueType>();
				newValueSet.Add(value);
				dictionary.Add(key, newValueSet);
			}
		}
		/// <summary>
		/// Adds <paramref name="value"/> to <paramref name="key"/> without checking to make sure the key exists
		/// </summary>
		public void Add_CertainOfKey(TKey key, TValueType value)
		{
			try
			{
				dictionary[key].Add(value);
			}
			catch (KeyNotFoundException)
			{
				throw new KeyNotFoundException($"The key '{key}' is not present in the dictionary");
			}
		}
		public void Remove(TKey key, TValueType value)
		{
			if (dictionary.TryGetValue(key, out HashSet<TValueType> values))
			{
				//if (values.Contains(value))
					values.Remove(value);
			}
		}
		public void Remove_CertainOfKey (TKey key, TValueType value)
		{
			try
			{
				dictionary[key].Remove(value);
			}
			catch (KeyNotFoundException)
			{
				throw new KeyNotFoundException($"Either the key '{key}' or the value '{value}' is not present in the dictionary");
			}
			
		}
		/// <summary>
		/// Removes all instances of the value 'value' in the dictionary
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		public void RemoveAllOfValueFromAllKeys(TValueType value)
		{
			foreach(KeyValuePair<TKey, HashSet<TValueType>> kvp in dictionary)
			{
				kvp.Value.Remove(value);
			}
		}
		public void ClearKey(TKey key)
		{
			if (dictionary.ContainsKey(key)) { dictionary[key].Clear(); }
		}
		public void DestroyKey (TKey key)
        {
			dictionary.Remove(key);
        }
		public void Clear ()
		{
			dictionary.Clear();
		}
		public void Clear_KeepKeys ()
		{
			foreach(HashSet<TValueType> hashset in dictionary.Values)
			{
				hashset.Clear();
			}
		}
		public IReadOnlyCollection<TValueType> Get_CertainOfKey (TKey key)
		{
			return dictionary[key];
		}
		public IReadOnlyCollection<TValueType> Get (TKey key)
		{
			if (dictionary.TryGetValue(key, out HashSet<TValueType> values))
			{
				return values;
			}
			else
			{
				return new TValueType[0];
			}
		}
		public bool Contains (TKey key, TValueType value)
		{
			if (dictionary.TryGetValue(key, out HashSet<TValueType> values))
			{
				return values.Contains(value);
			}
			return false;
		}

		public bool Contains_CertainOfKey (TKey key, TValueType value)
		{
			return dictionary[key].Contains(value);
		}

		public override string ToString()
		{
			string str = $"Hashset Dictionary<{typeof(TKey).Name}, {typeof(TValueType).Name}>:\n";
			foreach(TKey key in dictionary.Keys)
			{
				str += $"\t{key.ToString()}:\n";
				foreach (TValueType value in dictionary[key])
				{
					str += $"\t\t{value.ToString()}\n";
				}
			}
			return str;
		}
	}
}