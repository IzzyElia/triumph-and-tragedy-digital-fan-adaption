using System;

namespace TT2026.libraries.Izzy.DataPooling
{
    public struct DataPointer<T>
    {
        private int _index;
        public int Index { get { return _index; } }

        public DataPointer (DataPool<T> dataPool)
        {
            _index = dataPool.ReserveNewIndex();
        }

        public DataPointer(DataPool<T> dataPool, int forceIndex)
        {
            _index = dataPool.EnsureIndexIsReserved(forceIndex);
        }

        /// <summary>
        /// Frees the index reserved by this DataPointer so it can be reused
        /// </summary>
        /// <param name="dataPool"></param>
        public void Release (IDataPool dataPool)
        {
            dataPool.FreeIndex(_index);
        }

        public void Set (IDataPool dataPool, T value)
        {
            try
            {
                Set((DataPool<T>)dataPool, value);
            }
            catch (InvalidCastException)
            {
                throw new InvalidOperationException("Data type mismatch");
            }
        }
        public void Set(DataPool<T> dataPool, T value)
        {
            dataPool.SetData(_index, value);
        }

        public T Get(IDataPool dataPool)
        {
            try
            {
                return Get((DataPool<T>)dataPool);
            }
            catch (InvalidCastException)
            {
                throw new InvalidOperationException("Data type mismatch");
            }
        }
        public T Get(DataPool<T> dataPool, T value)
        {
            return dataPool.GetData(_index);
        }
    }
}