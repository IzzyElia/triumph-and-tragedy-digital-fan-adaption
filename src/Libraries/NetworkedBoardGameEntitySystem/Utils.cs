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
}