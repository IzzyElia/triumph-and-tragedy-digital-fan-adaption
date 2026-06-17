

namespace TT2026.libraries.Izzy.Geometry 
{
	/// <summary>
	/// 
	/// </summary>
	/// <remarks>
	///	In all classes implementing ICoordinate2d, Equals should always return true if the coordinate positions match another ICoordinate2d
	/// GetHashCode should be identical in all classes implementing ICoordinate2d (see GenericCoordinate2d for expected implementation)
	/// </remarks>
	public interface ICoordinate2d
	{

		public int x { get; }
		public int y { get; }

		public string BracketsString => "(" + x + ", " + y + ")";
	}
}