
namespace TT2026.libraries.Izzy.Geometry
{
	public static class MathG
	{
		public static Vector RadiansToVector (float radians)
		{
			return new Vector((float)System.Math.Cos(radians), (float)System.Math.Sin(radians));
		}
		/// <summary>
		/// Returns a random point in a circle with a diameter of 1
		/// </summary>
		public static Vector RandomPointInACircle (int seed)
		{
			float angle = RandFi.Float(seed) * 2 * Mathfi.PI;
			float distance = 0.5f * Mathfi.Sqrt(RandFi.Float(seed << 16));
			return new Vector
				(
				distance * Mathfi.Cos(angle),
				distance * Mathfi.Sin(angle)
				);
		}
	}
}
