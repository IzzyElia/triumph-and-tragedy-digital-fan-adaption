
namespace TT2026.libraries.Izzy.Geometry
{
    public struct SquareCoordinate : ICoordinate2d
    {
        public int x { get; private set; }
        public int y { get; private set; }

        public SquareCoordinate(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        // In all classes implementing ICoordinate2d, Equals should always return true if the coordinate positions match another ICoordinate2d
        public override bool Equals(object obj)
        {
            if (obj is ICoordinate2d coordinate)
                return x == coordinate.x && y == coordinate.y;
      
            return false;
        }
        // Equals and GetHashCode should be identical in all classes implementing ICoordinate2d
        public override int GetHashCode()
        {
            return (x << 16) | y;
        }

        public SquareCoordinate WrappedX(int maxX)
        {
            return new SquareCoordinate(Mathfi.Mod(x, maxX), y);
        }
        public SquareCoordinate WrappedY(int maxY)
        {
            return new SquareCoordinate(x, Mathfi.Mod(y, maxY));
        }
        public SquareCoordinate WrappedBoth(int maxX, int maxY)
        {
            return new SquareCoordinate(Mathfi.Mod(x, maxX), Mathfi.Mod(y, maxY));
        }
    }
}