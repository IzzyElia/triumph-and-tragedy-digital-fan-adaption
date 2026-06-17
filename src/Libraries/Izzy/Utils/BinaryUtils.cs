using System;
using System.Runtime.InteropServices;
using System.Text;
using TT2026.libraries.Izzy.UnitTesting;

namespace TT2026.libraries.Izzy.Utils;

public static class BinaryUtils
{
    public static byte[] PackString(string str)
    {
        byte[] stringAsByteArray = Encoding.UTF8.GetBytes(str);
        byte[] data = new byte[stringAsByteArray.Length + 1];
        for (int i = 1; i < data.Length; i++)
        {
            data[i] = stringAsByteArray[i - 1];
        }
        return data;
    }
    public static string ByteArrayToString(byte[] bytes)
    {
        string str = "[";
        if (bytes.Length > 0)
        {
            foreach (byte b in bytes)
            {
                str += b.ToString() + ", ";
            }
            str = str.Substring(0, str.Length - 2);
        }
        str += "]";
        return str;
    }
    public static byte[] SerializeUnmanaged<T>(T @struct) where T : struct
    {
        var size = Marshal.SizeOf(typeof(T));
        var array = new byte[size];
        var memoryPointer = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.StructureToPtr(@struct, memoryPointer, true);
            Marshal.Copy(memoryPointer, array, 0, size);
        }
        finally
        {
            Marshal.FreeHGlobal(memoryPointer);
        }
        return array;
    }
    // A version of the above that does not check whether the type is a struct
    // The method will fail if a non-value type is passed
    public static byte[] SerializeUnmanaged_NoCheck(object @struct)
    {
        var size = Marshal.SizeOf(@struct.GetType());
        var array = new byte[size];
        var memoryPointer = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.StructureToPtr(@struct, memoryPointer, true);
            Marshal.Copy(memoryPointer, array, 0, size);
        }
        finally
        {
            Marshal.FreeHGlobal(memoryPointer);
        }
        return array;
    }

    public static T DeserializeUnmanaged<T>(byte[] array) where T : struct
    {
        object? boxed;
        var size = Marshal.SizeOf(typeof(T));
        var memoryPointer = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.Copy(array, 0, memoryPointer, size);
            boxed = Marshal.PtrToStructure(memoryPointer, typeof(T));
        }
        finally
        {
            Marshal.FreeHGlobal(memoryPointer);
        }
        if (boxed == null) throw new InvalidOperationException($"Byte array could not be converted into a {typeof(T).Name}");
        return (T)boxed;
    }
    public static object DeserializeUnmanaged_NoCheck(byte[] array, Type type)
    {
        object? boxed;
        var size = Marshal.SizeOf(type);
        var memoryPointer = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.Copy(array, 0, memoryPointer, size);
            boxed = Marshal.PtrToStructure(memoryPointer, type);
        }
        finally
        {
            Marshal.FreeHGlobal(memoryPointer);
        }
        if (boxed == null) throw new InvalidOperationException($"Byte array could not be converted into a {type.Name}");
        return boxed;
    }

    /// <summary>
    /// Serialize an array of primitives to a byte array
    /// </summary>
    public static unsafe byte[] SerializeArray<T>(T[] array) where T : unmanaged
    {
        if (array == null)
            throw new ArgumentException("Array cannot be null");

        byte[] serializedAray = new byte[array.Length * sizeof(T)];
        Buffer.BlockCopy(array, 0, serializedAray, 0, serializedAray.Length);
        return serializedAray;
    }
    public static unsafe byte[] SerializeArray_Anonymous(Array array)
    {
        if (array == null)
            throw new ArgumentException("Array cannot be null");

        Type type = array.GetType();
        Type baseType = type.GetElementType();

        if (!type.IsArray)
            throw new ArgumentException("Type is not an array");
        if (!baseType.IsPrimitive)
            throw new ArgumentException("Array must be of a primitive type");

        byte[] serializedAray = new byte[array.Length * Marshal.SizeOf(baseType)];
        Buffer.BlockCopy(array, 0, serializedAray, 0, serializedAray.Length);
        return serializedAray;
    }
    /// <summary>
    /// Deserialize a byte array created with <see cref="SerializeArray{T}(T[])"/>
    /// </summary>
    public static unsafe T[] DeserializeArray<T>(byte[] bytes) where T : unmanaged
    {
        T[] array = new T[bytes.Length / sizeof(T)];
        Buffer.BlockCopy(bytes, 0, array, 0, bytes.Length);
        return array;
    }
}


class BinaryUtilsTests : ITestable
{
    public TestResult[] RunTests()
    {
        return new TestResult[]
        {
            TestPrimitiveArraySerialization()
        };
    }
    TestResult TestPrimitiveArraySerialization()
    {
        try
        {
            int[] array = new int[] { 1, 2, 3 };
            byte[] serialized = BinaryUtils.SerializeArray<int>(array);
            int[] deserialized = BinaryUtils.DeserializeArray<int>(serialized);
            for (int i = 0; i < array.Length; i++)
            {
                if (deserialized.Length != array.Length)
                {
                    return new TestResult(false, "Output array is of the wrong length");
                }
                if (deserialized[i] != array[i])
                    return new TestResult(false, "Deserialized array does not match input array");
            }

            return new TestResult(true);
        }
        catch (Exception e)
        {
            return new TestResult(false, e.Message);
        }
    }
}
public struct TestStruct
{
    float _float;
    int _int;

    public TestStruct(float f = 0.5f)
    {
        _float = f;
        _int = 2;
    }

    public static bool operator ==(TestStruct a, TestStruct b)
    {
        return a._float == b._float && a._int == b._int;
    }
    public static bool operator !=(TestStruct a, TestStruct b)
    {
        return !(a == b);
    }

    public override bool Equals(object obj)
    {
        return obj is TestStruct && (TestStruct)obj == this;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return _float.GetHashCode() + (17 * _int.GetHashCode());

        }
    }
}