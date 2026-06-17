using System;
using System.Collections.Generic;

namespace TT2026.libraries.Izzy
{
	/// <summary>
	/// A generic class for managing a dictionary with any number of ORDERED values per key
	/// </summary>
	[System.Serializable]
	public class ListDictionary<TKey, TValueType>
	{
		Dictionary<TKey, List<TValueType>> _dictionary;
		public ListDictionary() 
		{
			_dictionary = new Dictionary<TKey, List<TValueType>>();
		}
		public IEnumerable<TValueType> Values
		{
			get
			{
				foreach(List<TValueType> list in _dictionary.Values)
				{
					foreach(TValueType value in list)
					{
						yield return value;
					}
				}
			}
		}
		public void EnsureKey(TKey key)
		{
			if(!_dictionary.ContainsKey(key))
			{
				_dictionary.Add(key, new List<TValueType>());
			}
		}
		public void Add(TKey key, TValueType value)
		{
			if (_dictionary.TryGetValue(key, out List<TValueType> values))
			{
				values.Add(value);
			}
			else
			{
				List<TValueType> newValueSet = new List<TValueType>();
				newValueSet.Add(value);
				_dictionary.Add(key, newValueSet);
			}
		}
		/// <summary>
		/// Adds <paramref name="value"/> to <paramref name="key"/> without checking to make sure the key exists
		/// This is faster than <see cref="Add(TKey, TValueType)"/>. Make sure they key has previously been instantiated with <see cref="EnsureKey(TKey)"/>!
		/// </summary>
		public void Add_CertainOfKey(TKey key, TValueType value)
		{
			try
			{
				_dictionary[key].Add(value);
			}
			catch (KeyNotFoundException)
			{
				throw new KeyNotFoundException($"The key '{key}' is not present in the dictionary");
			}
		}
		public void Remove(TKey key, TValueType value)
		{
			if (_dictionary.TryGetValue(key, out List<TValueType> values))
			{
				//if (values.Contains(value))
					values.Remove(value);
			}
		}
		public void Remove_CertainOfKey (TKey key, TValueType value)
		{
			try
			{
				_dictionary[key].Remove(value);
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
		public void RemoveAllOfValue(TValueType value)
		{
			foreach(KeyValuePair<TKey, List<TValueType>> kvp in _dictionary)
			{
				kvp.Value.Remove(value);
			}
		}
		public void RemoveAllFromKey(TKey key)
		{
			if (_dictionary.ContainsKey(key)) { _dictionary[key].Clear(); }
		}
		public void DestroyKey (TKey key)
        {
			_dictionary.Remove(key);
        }
		public void Clear ()
		{
			_dictionary.Clear();
		}
		public void Clear_KeepKeys ()
		{
			foreach(List<TValueType> list in _dictionary.Values)
			{
				list.Clear();
			}
		}
		public IEnumerable<TValueType> Get_CertainOfKey (TKey key)
		{
			return _dictionary[key];
		}
		public IEnumerable<TValueType> GetUpToSpecifiedNumberOfValues_CertainOfKey (TKey key, int numValues)
		{
			for (int i = 0; i < numValues; i++)
			{
				yield return _dictionary[key][i];
			}
		}
		public IReadOnlyList<TValueType> Get (TKey key)
		{
			if (_dictionary.TryGetValue(key, out List<TValueType> values))
			{
				return values;
			}
			else
			{
				return Array.Empty<TValueType>();
			}
		}
		public int Count(TKey key)
		{
            if (_dictionary.TryGetValue(key, out List<TValueType> values))
            {
                return values.Count;
            }
            else
            {
                return 0;
            }
        }
        public int Count_CertainOfKey(TKey key)
        {
			return _dictionary[key].Count;
        }
        public bool Contains (TKey key, TValueType value)
		{
			if (_dictionary.TryGetValue(key, out List<TValueType> values))
			{
				return values.Contains(value);
			}
			return false;
		}
		public bool Contains_CertainOfKey (TKey key, TValueType value)
		{
			return _dictionary[key].Contains(value);
		}
		public override string ToString()
		{
			string str = $"ListDictionary ({typeof(TKey).Name}, {typeof(TValueType).Name}):\n";
			foreach(TKey key in _dictionary.Keys)
			{
				str += $"\t{key.ToString()}:\n";
				foreach (TValueType value in _dictionary[key])
				{
					str += $"\t\t{value.ToString()}\n";
				}
			}
			return str;
		}
	}
}