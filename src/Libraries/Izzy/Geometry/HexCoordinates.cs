using System;
using System.Collections.Generic;
using TT2026.libraries.Izzy.UnitTesting;

namespace TT2026.libraries.Izzy.Geometry;

// In the same order as the adjecency array. That way Directions.Left can
// be used like Adjecent[(int)Directions.Left]
public enum HexDirection
{
	Left,
	UpperLeft,
	UpperRight,
	Right,
	LowerRight,
	LowerLeft
}
[Serializable]
public struct CubicCoordinates : ICoordinate2d
{
	/// <summary>
	/// Is (x, y, z) a valid cubic coordinate set?
	/// </summary>
	public static bool Validate (int x, int y, int z)
	{
		return x + y + z == 0;
	}
	public int x { get; private set; }
	public int y { get; private set; }
	public int z { get; private set; }
	public CubicCoordinates(int x, int y, int z)
	{
#if DEBUG
		if (!Validate(x,y,z))
		{
			throw new System.ArgumentException($"Invalid coordinates ({x}, {y}, {z})");
		}
#endif
		this.x = x;
		this.y = y;
		this.z = z;
	}
	/// <summary>
	/// Z can be derived
	/// </summary>
	public CubicCoordinates(int x, int y)
	{
		this.x = x;
		this.y = y;
		this.z = -(x + y);
	}
	/* Order is explicit
			 0 -> Left
			 1 -> Upper Left
			 2 -> Upper Right
			 3 -> Right
			 4 -> Lower Right
			 5 -> Lower Left
		 */
	public IEnumerable<CubicCoordinates> Adjecent
	{
		get
		{
			yield return this + Left;
			yield return this + UpperLeft;
			yield return this + UpperRight;
			yield return this + Right;
			yield return this + LowerRight;
			yield return this + LowerLeft;
		}
	}
	public static CubicCoordinates Left => new(-1, 1, 0);
	public static CubicCoordinates UpperRight => new(0, -1, 1);
	public static CubicCoordinates UpperLeft => new(-1, 0, 1);
	public static CubicCoordinates Right => new(1, -1, 0);
	public static CubicCoordinates LowerLeft => new(0, 1, -1);
	public static CubicCoordinates LowerRight => new(1, 0, -1);

	/// <summary>
	/// [0] = distance 0, [1-6] = distance 1, [7-18] = distance 3, etc.
	/// </summary>
	public IEnumerable<CubicCoordinates> WithinDistance(int distance, int minDistance = 0)
	{
		if (minDistance == 0) yield return this;
		int n = 1;
		for (int iDistance = minDistance; iDistance <= distance; iDistance++)
		{
			if (iDistance == 0) continue;
			yield return new CubicCoordinates(this.x, this.y + iDistance, this.z + -iDistance);
			n++;
			yield return new CubicCoordinates(this.x, this.y + -iDistance, this.z + iDistance);
			n++;

			int x = -iDistance;
			for (int y = iDistance; y >= 0; y--)
			{
				yield return new CubicCoordinates(this.x + x, this.y + y);
				n++;
			}
			x = iDistance;
			for (int y = -iDistance; y <= 0; y++)
			{
				yield return new CubicCoordinates(this.x + x, this.y + y);
				n++;
			}
			x = iDistance - 1; // For some reason the following for loop doesn't initialize correctly unless we do it here..??? C# quirk I don't know about?
			for (x = iDistance - 1; x > 0; x--)
			{
				yield return new CubicCoordinates(this.x + x, this.y + -iDistance, this.z + iDistance - x);
				n++;
				yield return new CubicCoordinates(this.x + x, this.y + iDistance - x, this.z + -iDistance);
				n++;
			}
			for (x = -iDistance + 1; x < 0; x++)
			{
				yield return new CubicCoordinates(this.x + x, this.y + iDistance, this.z + (-iDistance - x));
				n++;
				yield return new CubicCoordinates(this.x + x, this.y + (-iDistance - x), this.z + iDistance);
				n++;
			}
		}
	}
	public static CubicCoordinates NumereticalPosition(int i)
	{
		int pos = Mathfi.Mod(i, 6);
		switch (pos)
		{
			case 0:
				return Left;
			case 1:
				return UpperLeft;
			case 2:
				return UpperRight;
			case 3:
				return Right;
			case 4:
				return LowerRight;
			case 5:
				return LowerLeft;
			default:
				throw new System.InvalidOperationException();
		}
	}
	public static CubicCoordinates operator +(CubicCoordinates a, CubicCoordinates b)
	{
		int x = a.x + b.x;
		int y = a.y + b.y;
		int z = a.z + b.z;
		return new CubicCoordinates(x, y, z);
	}
	public static CubicCoordinates operator -(CubicCoordinates a, CubicCoordinates b)
	{
		int x = a.x - b.x;
		int y = a.y - b.y;
		int z = a.z - b.z;
		return new CubicCoordinates(x, y, z);
	}
	public static CubicCoordinates operator *(CubicCoordinates a, int b)
	{
		int x = a.x * b;
		int y = a.y * b;
		int z = a.z * b;
		return new CubicCoordinates(x, y, z);
	}

	/// <summary> Rounds from a 3-point-vector to the nearest integer cubic coordinate </summary>
	public static CubicCoordinates RoundFromCubicVector (Vector vector)
	{
		int x = Mathfi.RoundToInt(vector.x);
		int y = Mathfi.RoundToInt(vector.y);
		int z = Mathfi.RoundToInt(vector.z);

		float x_diff = Mathfi.Abs(x - vector.x);
		float y_diff = Mathfi.Abs(y - vector.y);
		float z_diff = Mathfi.Abs(z - vector.z);

		if (x_diff > y_diff && x_diff > z_diff)
			x = -y - z;
		else if (y_diff > z_diff)
			y = -x - z;
		else
			z = -x - y;


		return new CubicCoordinates(x, y, z);
	}
	public int DistanceTo (CubicCoordinates destination)
	{
		CubicCoordinates vector = this - destination;
		return (Mathfi.Abs(vector.x) + Mathfi.Abs(vector.y) + Mathfi.Abs(vector.z)) / 2;
	}

	public override string ToString()
	{
		return $"{x},{y},{z}";
	}
	public static CubicCoordinates FromString(string str)
	{
		int x, y, z;
		try
		{
			string[] coordinates = str.Split(',');
			x = int.Parse(coordinates[0]);
			y = int.Parse(coordinates[1]);
			z = int.Parse(coordinates[2]);
		}
		catch (System.Exception)
		{
			throw new System.ArgumentException($"Failed to parse cube coordinates from string: {str})");
		}
		CubicCoordinates cube = new CubicCoordinates(x, y, z);
		return cube;
	}

	public override int GetHashCode()
	{
		unchecked
		{
			return ToOffset.GetHashCode();
		}
	}

	public OffsetErCoordinates ToOffset
	{
		get
		{
			int ox = x + (z + (z & 1)) / 2;
			int oy = z;
			return new OffsetErCoordinates(ox, oy);
		}
	}
	public OffsetDoubleCoordinates ToDouble
	{
		get
		{
			int dx = 2 * x + z;
			int dy = z;
			return new OffsetDoubleCoordinates(dx, dy);
		}
	}



}

//compatible with even-r coordinates only
[Serializable]
public struct OffsetErCoordinates : ICoordinate2d
{
	public int x { get; private set; }
	public int y { get; private set; }
	public OffsetErCoordinates (int x, int y)
	{
		this.x = x;
		this.y = y;
	}
	public OffsetErCoordinates (ICoordinate2d coordinate)
	{
		this.x = coordinate.x;
		this.y = coordinate.y;
	}
	// A Y version of this method doesn't really make sense under this coordinate system. Diagonal versions maybe?
	public OffsetErCoordinates OffsetX(int x)
	{
		return new OffsetErCoordinates(this.x + x, this.y);
	}
	public OffsetErCoordinates WrapX(int maxX)
	{
		return new OffsetErCoordinates(Mathfi.Mod(x, maxX), y);
	}
	public IEnumerable<OffsetErCoordinates> Adjecent
	{
		get
		{
			foreach (var adjecent in ToCube.Adjecent)
			{
				yield return adjecent.ToOffset;
			}
		}
	}
	public IEnumerable<OffsetErCoordinates> AdjecentWrappedX (int maxX)
	{
		foreach (var adjecent in ToCube.Adjecent)
		{
			yield return adjecent.ToOffset.WrapX(maxX);
		}
	}
	public int DistanceTo (OffsetErCoordinates destination)
	{
		return ToCube.DistanceTo(destination.ToCube);
	}
	public int DistanceTo_WrappingX (OffsetErCoordinates b, int wrapWidth)
	{
		return Mathfi.Min
		(
			DistanceTo(b),
			DistanceTo(b.OffsetX(-wrapWidth))
		);
	}

	public static bool operator ==(OffsetErCoordinates a, OffsetErCoordinates b) => a.GetHashCode() == b.GetHashCode();
	public static bool operator !=(OffsetErCoordinates a, OffsetErCoordinates b) => a.GetHashCode() != b.GetHashCode();
	// In all classes implementing ICoordinate2d, Equals should always return true if the coordinate positions match another ICoordinate2d
	public override bool Equals(object obj)
	{
		if (obj is OffsetErCoordinates) return this == (OffsetErCoordinates)obj;
		if (obj is OffsetDoubleCoordinates) { OffsetDoubleCoordinates cast = (OffsetDoubleCoordinates)obj; return cast.ToOffset == this; }
		if (obj is CubicCoordinates) { CubicCoordinates cast = (CubicCoordinates)obj; return cast.ToOffset == this; }
		if (obj is ICoordinate2d coordinate)
			return x == coordinate.x && y == coordinate.y;
		return false;
	}
	// GetHashCode should be identical in all classes implementing ICoordinate2d
	public override int GetHashCode()
	{
		return (x << 16) | y;
	}
	public override string ToString()
	{
		return $"{x},{y}";
	}
	public static OffsetErCoordinates FromString(string str)
	{
		int x, y;
		try
		{
			string[] coordinates = str.Split(',');
			x = int.Parse(coordinates[0]);
			y = int.Parse(coordinates[1]);
		}
		catch (System.Exception)
		{
			throw new System.ArgumentException($"Failed to parse cube coordinates from string: {str})");
		}
		OffsetErCoordinates offset = new OffsetErCoordinates(x, y);
		return offset;
	}
	
	public CubicCoordinates ToCube
	{
		get
		{
			int cx = x - (y + (y & 1)) / 2;
			int cz = y;
			int cy = -cx - cz;
			CubicCoordinates cube = new CubicCoordinates(cx, cy, cz);
			return cube;
		}
	}
	public OffsetDoubleCoordinates ToDouble
	{
		get
		{
			int dx = (x * 2) - (y & 1);
			int dy = y;
			return new OffsetDoubleCoordinates(dx, dy);
		}
	}
	public Vector ToVector => ToDouble.ToVector;

}

public struct OffsetDoubleCoordinates //Doubled width coordinates
{
	public int x { get; private set; }
	public int y { get; private set; }
	public OffsetDoubleCoordinates(int x, int y)
	{
		//TODO - validate coordinate validity
		this.x = x;
		this.y = y;
	}

	public Vector ToVector => new Vector
	(
		Mathfi.Sqrt(3f) / 2f * x,
		3f / 2f * y
	);

	public override string ToString()
	{
		return $"x:{x}, y:{y}";
	}
	public static OffsetDoubleCoordinates FromString(string str)
	{
		int x, y;
		try
		{
			string[] coordinates = str.Split(',');
			x = int.Parse(coordinates[0]);
			y = int.Parse(coordinates[1]);
		}
		catch (System.Exception)
		{
			throw new System.ArgumentException($"Failed to parse cube coordinates from string: {str})");
		}
		OffsetDoubleCoordinates offsetDouble = new OffsetDoubleCoordinates(x, y);
		return offsetDouble;
	}
	public override int GetHashCode()
	{
		unchecked
		{
			return ToOffset.GetHashCode();
		}
	}
	public CubicCoordinates ToCube
	{
		get
		{
			int cx = (x - y) / 2;
			int cz = y;
			int cy = -cx - cz;
			return new CubicCoordinates(cx, cy, cz);
		}
	}
	public OffsetErCoordinates ToOffset
	{
		get
		{
			int ox = (x + (y & 1)) / 2;
			int oy = y;
			return new OffsetErCoordinates(ox, oy);
		}
	}
}


# if DEBUG
internal class HexCoordinateUnitTests
{
	[Test]
	static TestResult TestNumereticalPosition()
	{
		bool ok =
		(
			CubicCoordinates.NumereticalPosition(-1).Equals(CubicCoordinates.LowerLeft) &&
			CubicCoordinates.NumereticalPosition(0).Equals(CubicCoordinates.Left) &&
			CubicCoordinates.NumereticalPosition(5).Equals(CubicCoordinates.LowerLeft) &&
			CubicCoordinates.NumereticalPosition(6).Equals(CubicCoordinates.Left)
		);
		string message = "[";
		for (int i = -12; i < 12; i++)
		{
			message += $"\n:{i.ToString()} => {CubicCoordinates.NumereticalPosition(i).ToString()}:";
		}
		message += "]";
		return new TestResult(ok, message);
	}
	[Test]
	static TestResult TestWithinDistance()
	{
		const int distance = 4;
		CubicCoordinates start = new CubicCoordinates(0, 0, 0);
		string o = $"Results within distance {distance}";
		foreach (CubicCoordinates position in start.WithinDistance(distance))
		{
			o += $"\n{position.ToString()}";
		}
		return new TestResult(true, o);
	}
	[Test]
	static TestResult TestWrap()
	{
		bool ok =
		(
			new OffsetErCoordinates(-5, 2).WrapX(10).x == 5 &&
			new OffsetErCoordinates(2, 0).WrapX(2).x == 0 &&
			new OffsetErCoordinates(2, 0).WrapX(3).x == 2

		);
		return new TestResult(ok);
	}
	[Test]
	static TestResult TestHashing()
	{
		int duplicatesCount = 0;
		System.Collections.Generic.List<int> duplicates = new System.Collections.Generic.List<int>();
		const int dimentions = 128; //x by x sized grid
		int size = dimentions * dimentions;
		System.Collections.Generic.HashSet<int> hashes = new System.Collections.Generic.HashSet<int>();
		int[] hashesRaw = new int[size];
		for (int x = 0; x < dimentions; x++)
		{
			for (int y = 0; y < dimentions; y++)
			{
				int i = (x * dimentions) + y;
				int hash = new OffsetErCoordinates(x, y).GetHashCode();
				if (hashes.Contains(hash)) { duplicatesCount++; duplicates.Add(hash); }
				hashes.Add(hash);
				hashesRaw[i] = hash;
			}
		}

		switch (duplicatesCount == 0)
		{
			case true:
				return new TestResult(true, $"No hash collisions found in a {dimentions} by {dimentions} space");
			case false:
				return new TestResult(false, $"{duplicatesCount} duplicate hash codes found in a {dimentions} by {dimentions} space");
		}

	}
}

#endif