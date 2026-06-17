using System.Diagnostics.CodeAnalysis;

namespace TT2026.libraries.Izzy
{
    public struct Pair<T>
    {
        T _a;
        T _b;
        public Pair(T a, T b)
        {
            _a = a;
            _b = b;
        }
        public static bool operator ==(Pair<T> a, Pair<T> b) => a.GetHashCode() == b.GetHashCode();
        public static bool operator !=(Pair<T> a, Pair<T> b) => a.GetHashCode() == b.GetHashCode();
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return obj is Pair<T> && obj.GetHashCode() == this.GetHashCode();
        }
        public override int GetHashCode()
        {
            int hash;
            unchecked
            {
                hash = _a.GetHashCode() + _b.GetHashCode();
            }
            return hash;
        }
    }
}
