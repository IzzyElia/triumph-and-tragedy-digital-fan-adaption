using System;
using System.Collections.Generic;

namespace TT2026.libraries.Izzy.DataPooling
{
    [Serializable]
    public class DataPool<T> : IDataPool
    {

        private T[] data;
        private List<int> freedIndexes = new List<int>();
        private int nextIndex = 0;
        public DataPool() : this(4096) { }
        public DataPool(int startingSize) 
        {
            this.data = new T[startingSize];
        }
        public T this[int index]
        {
            get
            {
                return GetData(index);
            }
            set
            {
                SetData(index, value);
            }
        }
        public T GetData (int index)
        {
            if (index >= data.Length)
                return default(T);
            else
                return data[index];
        }
        public void SetData(int index, T value)
        {
#if DEBUG
            if (index > 268435456)
                DynamicLogger.LogWarning($"Very large index used in data pooling - do you need this much?");
            if (freedIndexes.Contains(index))
                DynamicLogger.LogWarning("Writing to an index that has been declared as free");
#endif


            while (data.Length <= index) // This could be optimized. How do you find the inverse of the doubling function: i/(2^x)?
            {
                Grow(1);
            }
            data[index] = value;
        }
        public int EnsureIndexIsReserved(int index)
        {
            if (freedIndexes.Contains(index))
            {
                freedIndexes.Remove(index);
            }
            if (index > nextIndex)
            {
                for (int i = nextIndex; i < index; i++)
                {
                    if (!freedIndexes.Contains(i))
                        freedIndexes.Add(i);
                }
                nextIndex = index + 1;
            }
            return index;
        }
        public int ReserveNewIndex()
        {
            int index;
            if (freedIndexes.Count > 0)
            {
                index = freedIndexes[freedIndexes.Count - 1];
                freedIndexes.RemoveAt(freedIndexes.Count - 1);
            }
            else
            {
                index = nextIndex++;
            }
            return index;
        }
        /// <summary>
        /// Marks an index as being unused and available for reuse
        /// </summary>
        /// <param name="index"></param>
        public void FreeIndex (int index)
        {
            freedIndexes.Add(index);
        }
        void Grow (int timesToDouble)
        {
            T[] newData = new T[doubleToX(data.Length, timesToDouble)];
            data.CopyTo(newData, 0 );
            data = newData;
        }
        public KeyValuePair<int, object>[] GetAllEntries ()
        {
            KeyValuePair<int, object>[] objects = new KeyValuePair<int, object>[nextIndex];
            for (int i = 0; i < nextIndex; i++)
            {
                objects[i] = new KeyValuePair<int, object>(i, data[i]);
            }
            return objects;
        }

        public string DataToString ()
        {
            string str = $"{typeof(T).Name}:\n";
            for (int i = 0; i < data.Length; i++)
            {
                if (i >= nextIndex)
                    break;
                str += $"[{i}] {data[i]}\n";
                if (typeof(T).IsArray)
                {
                    Array array = data[i] as Array;
                    for (int i2 = 0; i2 < array.Length; i2++)
                    {
                        str += $"_{i2} = {array.GetValue(i2).ToString()}\n";
                    }
                }
            }
            return str;
        }


        // Utility Functions
        private int doubleToX(int i, int x) => i * (2 ^ x);
    }
    public interface IDataPool : IDataDisplayer
    {
        public int ReserveNewIndex();
        public void FreeIndex(int index);
        public KeyValuePair<int, object>[] GetAllEntries();
    }
}
