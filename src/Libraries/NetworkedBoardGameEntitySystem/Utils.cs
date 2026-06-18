using System;
using System.Text;

namespace TT2026.libraries.NetworkedBoardGameEntitySystem;

public static class Utils
{
    public static int Fnv1AHash(string input)
    {
        const int fnvPrime = 16777619;
        const int fnvOffsetBasis = unchecked((int)2166136261);

        int hash = fnvOffsetBasis;
        foreach (byte b in Encoding.UTF8.GetBytes(input))
        {
            hash ^= b;
            hash *= fnvPrime;
        }
        return hash;
    }
    
    public static int Mix32Hash(int x)
    {
        uint y = unchecked((uint)x);
        y ^= y >> 16;
        y *= 0x85ebca6b;
        y ^= y >> 13;
        y *= 0xc2b2ae35;
        y ^= y >> 16;
        return unchecked((int)y);
    }
    
    public static string[] ChunkString(string input, int maxSize)
    {
        if (string.IsNullOrEmpty(input))
            return Array.Empty<string>();

        if (maxSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxSize));

        int chunkCount = (input.Length + maxSize - 1) / maxSize;
        var result = new string[chunkCount];

        for (int i = 0; i < chunkCount; i++)
        {
            int start = i * maxSize;
            int length = Math.Min(maxSize, input.Length - start);
            result[i] = input.Substring(start, length);
        }

        return result;
    }
}