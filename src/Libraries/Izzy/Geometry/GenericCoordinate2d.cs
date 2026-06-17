using System;

namespace TT2026.libraries.Izzy.Geometry
{
    [Serializable]
    public struct GenericCoordinate2d : ICoordinate2d
    {
        public int x { get; private set; }
        public int y { get; private set; }

        public GenericCoordinate2d(int x, int y)
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
        
        public static implicit operator SquareCoordinate(GenericCoordinate2d coordinate)
        {
            return new SquareCoordinate(coordinate.x, coordinate.y);
        }

        public static implicit operator OffsetErCoordinates(GenericCoordinate2d coordinate)
        {
            return new OffsetErCoordinates(coordinate.x, coordinate.y);
        }
        
        public int DistanceTo_WrappingX(ICoordinate2d iCoordinate2d, int width)
        {
            throw new NotImplementedException();
        }

        public int DistanceTo(ICoordinate2d iCoordinate2d)
        {
            throw new NotImplementedException();
        }
    }
}