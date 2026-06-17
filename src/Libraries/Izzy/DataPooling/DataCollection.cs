using System;
using System.Collections.Generic;

namespace TT2026.libraries.Izzy.DataPooling
{
    [Serializable]
    public class DataCollection : IDataDisplayer
    {
        Dictionary<Guid, IDataPool> data = new Dictionary<Guid, IDataPool>();

        DataPool<T> GetOrCreateDataPoolOfType<T>()
        {
            DataPool<T> dataPool;
            if (data.TryGetValue(typeof(T).GUID, out IDataPool dataPoolInterface))
            {
                dataPool = (DataPool<T>)dataPoolInterface;
            }
            else
            {
                dataPool = new DataPool<T>();
                data.Add(typeof(T).GUID, dataPool);
            }
            return dataPool;
        }
        IDataPool GetDataPoolOfType(Type type)
        {
            if (data.TryGetValue(type.GUID, out IDataPool dataPoolInterface))
            {
                return dataPoolInterface;
            }
            else
            {
                throw new InvalidOperationException($"No data pool exists of type '{type}'");
            }
        }
        public void Flush()
        {
            data.Clear();
        }
        public void WriteData<T> (int index, T value)
        {
            DataPool<T> dataPool = GetOrCreateDataPoolOfType<T>();
            dataPool.EnsureIndexIsReserved(index);
            dataPool.SetData(index, value);
        }
        public int InitData<T> (T value)
        {
            DataPool<T> dataPool = GetOrCreateDataPoolOfType<T>();
            int index = dataPool.ReserveNewIndex();
            dataPool.SetData(index, value);
            return index;
        }
        public void FreeData<T> (int index)
        {
            DataPool<T> dataPool = GetOrCreateDataPoolOfType<T>();
            dataPool.FreeIndex(index);
        }
        public void FreeData(Type type, int index)
        {
            IDataPool dataPool = GetDataPoolOfType(type);
            dataPool.FreeIndex(index);
        }
        public T ReadData<T>(int index)
        {
            DataPool<T> dataPool = GetOrCreateDataPoolOfType<T>();
            return dataPool.GetData(index);
        }
        public KeyValuePair<int, object>[] GetAllData ()
        {
            List<KeyValuePair<int, object>> dataList = new List<KeyValuePair<int, object>>();
            foreach(IDataPool dataPool in data.Values)
            {
                foreach(KeyValuePair<int, object> entry in dataPool.GetAllEntries())
                {
                    dataList.Add(entry);
                }
            }
            return dataList.ToArray();
        }

        public string DataToString()
        {
            string str = "";
            foreach(IDataPool dataPool in data.Values)
            {
                str += dataPool.DataToString();
            }
            return str;
        }
    }
}
