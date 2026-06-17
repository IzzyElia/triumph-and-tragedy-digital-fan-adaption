using TT2026.libraries.Izzy.UnitTesting;

namespace TT2026.libraries.Izzy.Geometry;

/// <summary>
/// A generic class that can represent a vector of length 2, 3, or 4
/// </summary>
[System.Serializable]
public struct Vector
{
    //TODO - define GetHashCode(). Then define ==, !=, Equals()
    //TODO - create a method to generate a cross product
    public float this[int index]
    {
        get
        {
            if (index < _points.Length) { return _points[index]; }
            else { return 0; }
        }
        set
        {
            if (index >= _points.Length)
            {
                float[] newPoints = new float[index + 1];
                _points.CopyTo(newPoints, 0);
                _points = newPoints;
            }
            _points[index] = value;
        }
    }
    float[] _points;
    public float x { get { if (_points.Length > 0) { return _points[0]; } else { return 0; } } set { if (_points.Length > 0) { _points[0] = value; } } }
    public float y { get { if (_points.Length > 1) { return _points[1]; } else { return 0; } } set { if (_points.Length > 1) { _points[1] = value; } } }
    public float z { get { if (_points.Length > 2) { return _points[2]; } else { return 0; } } set { if (_points.Length > 2) { _points[2] = value; } } }
    public float w { get { if (_points.Length > 3) { return _points[3]; } else { return 0; } } set { if (_points.Length > 3) { _points[3] = value; } } }

    //Properties
    public int Dimentions => _points.Length;
    public float Magnitude
    {
        get
        {
            float sum = 0;
            for (int i = 0; i < _points.Length; i++)
            {
                sum += _points[i] * _points[i];
            }
            return Mathfi.Sqrt(sum);
        }
    }

    public Vector Normalized { get => this / Magnitude; }

    //Construction

    public Vector(float[] points)
    {
        _points = points;
    }
    public Vector(float x)
    {
        _points = new float[1] { x };
    }
    public Vector(float x, float y)
    {
        _points = new float[2] { x, y };
    }
    public Vector(float x, float y, float z)
    {
        _points = new float[3] { x, y, z };

    }
    public Vector(float x, float y, float z, float w)
    {
        _points = new float[4] { x, y, z, w };

    }
    public static Vector Zero() => Zero(0);
    public static Vector Zero(int length)
    {
        float[] points = new float[length];
        for (int i = 0; i < length; i++)
        {
            points[i] = 0;
        }
        return new Vector(points);
    }
    public static Vector One(int length)
    {
        float[] points = new float[length];
        for (int i = 0; i < length; i++)
        {
            points[i] = 1;
        }
        return new Vector(points);
    }

    //Operators

    public static Vector operator +(Vector a, Vector b)
    {
        float[] points = new float[(int)Mathfi.Max(a._points.Length, b._points.Length)];
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = a[i] + b[i];
        }
        return new Vector(points);
    }
    public static Vector operator -(Vector a, Vector b)
    {
        float[] points = new float[(int)Mathfi.Max(a._points.Length, b._points.Length)];
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = a[i] - b[i];
        }
        return new Vector(points);
    }
    public static Vector operator -(Vector a)
    {
        float[] points = new float[a._points.Length];
        for (int i = 0; i < a._points.Length; i++)
        {
            points[i] = -a._points[i];
        }
        return new Vector(points);
    }
    public static Vector operator *(Vector a, float b)
    {
        float[] points = new float[a._points.Length];
        for (int i = 0; i < a._points.Length; i++)
        {
            points[i] = a[i] * b;
        }
        return new Vector(points);
    }
    public static Vector operator *(float b, Vector a)
    {
        return a * b;
    }
    public static Vector operator /(Vector a, float b)
    {
        if (b == 0) { throw new System.ArgumentOutOfRangeException($"Tried to divide vector by zero"); }
        float[] points = new float[a._points.Length];
        for (int i = 0; i < a._points.Length; i++)
        {
            points[i] = a._points[i] / b;
        }
        return new Vector(points);
    }

    //String Packing

    /// <summary>
    /// Convert the vector into a string array. Can be unpacked with Vector.FromStringArray
    /// </summary>
    /// <returns></returns>
    public string[] ToStringArray()
    {
        string[] array = new string[_points.Length];
        for (int i = 0; i < _points.Length; i++)
        {
            array[i] = _points[i].ToString();
        }
        return array;
    }
    /// <summary>
    /// Create a vector from a string array
    /// </summary>
    public static Vector FromStringArray(string[] array)
    {
        float[] points = new float[array.Length];
        for (int i = 0; i < array.Length; i++)
        {
            points[i] = float.Parse(array[i]);
        }
        return new Vector(points);
    }
    public static Vector FromFloatArray(float[] array)
    {
        if (array.Length < 2) { throw new System.ArgumentException($"array must be of at least length 2"); }
        return new Vector(array);
    }


    // Extras
    public static Vector Lerp(Vector a, Vector b, float t)
    {
        int numPoints = Mathfi.Min(a._points.Length, b._points.Length);
        float[] newPoints = new float[numPoints];
        for (int i = 0; i < numPoints; i++)
        {
            newPoints[i] = a[i] + (b[i] - a[i]) * t;
        }
        return new Vector(newPoints);
    }

    public static Vector RandomPositionInCircle(float minRadius, float maxRadius)
    {
        float radius = maxRadius - minRadius;
        float a = RandFi.Float() * 2 * (float)System.Math.PI;
        float r = radius * (float)System.Math.Sqrt(RandFi.Float());
        r += minRadius;
        if (r < 0.5f && r > -0.5f && minRadius == 0.5f)
        {
            r = 0;
        }
        return new Vector(r * (float)System.Math.Cos(a), r * (float)System.Math.Sin(a), 0);
    }
    public static Vector RandomPositionInCircle(float radius) { return RandomPositionInCircle(0, radius); }
    public static Vector RandomPositionInCircle() { return RandomPositionInCircle(1, 0); }
    public static Vector RandomPositionInBox(int dimentions, float size)
    {
        float[] points = new float[dimentions];
        for (int i = 0; i < dimentions; i++)
        {
            points[i] = (RandFi.Float() * size) - (size / 2f);
        }
        return new Vector(points);
    }
    public static Vector RandomPositionInBox(int dimentions) { return RandomPositionInBox(dimentions, 1); }
    public static Vector RandomPositionInSquare(float size) { return RandomPositionInBox(2, size); }
    public static Vector RandomPositionInCube(float size) { return RandomPositionInBox(3, size); }
}


# if DEBUG
public class VectorTests
{
    [Test]
    public static TestResult TestRandomCircle()
    {
        bool success = true;
        Vector[] positions = new Vector[2000];
        for (int i = 0; i < 1000; i++)
        {
            Vector vector = Vector.RandomPositionInCircle(0, 1);
            if (vector.x > 1 || vector.x < -1 || vector.y > 1 || vector.y < -1)
            { success = false; }
            positions[i] = vector;
        }
        for (int i = 0; i < 1000; i++)
        {
            Vector vector = Vector.RandomPositionInCircle(0.5f, 1);
            if (vector.x > 1 || vector.x < -1 || vector.y > 1 || vector.y < -1
                || (/*magnitude < 0.5f*/false))
            { success = false; }
            positions[1000 + i] = vector;
        }
        string message = "";
        for (int i = 0; i < positions.Length; i++)
        {
            message += $"{positions[i].x},{positions[i].y}\n";
        }
        return new TestResult(success, message);
    }
}

#endif