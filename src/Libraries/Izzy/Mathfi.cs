using System.Collections.Generic;
using Godot;

namespace TT2026.libraries.Izzy
{
    //Floating point math
    public static class Mathfi
    {
        //math
        public static float PI => (float)System.Math.PI;
        public static string ConvertToPercentageString(float value)
        {
            int percentageInt = (int)System.Math.Round((double)(value * 100));
            return percentageInt.ToString() + "%";
        }
        public static string GetTimestamp(System.DateTime value)
        {
            return value.ToString("yyyyMMddHHmmssffff");
        }

        /// <summary>
        /// Returns the modulo of a number.
        /// </summary>
        /// <param name="x">The number to be divided.</param>
        /// <param name="m">The divisor.</param>
        /// <returns>The modulo of <paramref name="x"/> and <paramref name="m"/>.</returns>
        public static int Mod(int x, int m)
        {
            int r = x % m;
            return r < 0 ? r + m : r;
        }
        public static float Mod(float x, float m)
		{
            float r = x % m;
            return r < 0 ? r + m : r;
        }
        public static float Sin(float x) => (float)System.Math.Sin(x);
        public static float Cos(float x) => (float)System.Math.Cos(x);
        public static float Sigmoid(float x)
        {
            return 1 / (1 + Exp(-x));
        }
        ///<summary> Returns /f/ raised to power /p/. </summary>
        public static float Pow(float x, float p) { return (float)System.Math.Pow(x, p); }

        ///<summary> Returns e raised to the specified power. </summary>
        public static float Exp(float power) { return (float)System.Math.Exp(power); }
        public static float Sq(float x) => x * x;
        public static float Sqrt (float x) { return (float)System.Math.Sqrt(x); }
        public static float Abs (float x) { if (x < 0) return x * -1; else return x; }
        public static int Abs (int x) { if (x < 0) return -x; else return x; }
        public static float Round (float f)
		{
            return (float)System.Math.Round(f);
		}
        public static float Round(float value, int decimalPlaces)
        {
            float scale = Mathf.Pow(10, decimalPlaces);
            return Round(value * scale) / scale;
        }
        public static int RoundToInt(float f)
		{
            return (int)System.Math.Round(f);
		}
        public static float CircularMean(List<float> points, float period) // credit to stackoverflow user relatively_random
        {
            float scalingFactor = 2 * (float)System.Math.PI / period;

            float sinesTotal = 0f;
            float cosinesTotal = 0f;
            foreach (float value in points)
            {
                float radians = value * scalingFactor;
                sinesTotal += (float)System.Math.Sin(radians);
                cosinesTotal += (float)System.Math.Cos(radians);
            }

            float circularMean = Atan2(sinesTotal, cosinesTotal) / scalingFactor;

            if (circularMean >= 0)
                return circularMean;
            else
                return circularMean + period;
        }
        public static float DegreesToRadians (float degrees) => degrees * ((float)System.Math.PI * 2F / 360F);
        public static float RadiansToDegrees (float radians) => radians * (1F / ((float)System.Math.PI * 2F / 360F));
        public static float Atan(float x) { return (float)System.Math.Atan(x); }
        public static float Atan2(float y, float x) { return (float)System.Math.Atan2(y, x); }
        public static float Min(float a, float b) { return a < b ? a : b; }
        public static float Min(params float[] values)
        {
            float smallest = values[0];
            for (int i = 1; i < values.Length; i++)
            {
                if (values[i] < smallest) { smallest = values[i]; }
            }
            return smallest;
        }
        public static int Min(int a, int b) { return a < b ? a : b; }
        public static int Min(params int[] values)
        {
            int smallest = values[0];
            for (int i = 1; i < values.Length; i++)
            {
                if (values[i] < smallest) { smallest = values[i]; }
            }
            return smallest;
        }
        public static float Max(float a, float b) { return a > b ? a : b; }
        public static float Max(params float[] values)
		{
            float largest = values[0];
			for (int i = 1; i < values.Length; i++)
			{
                if (values[i] > largest) { largest = values[i]; }
			}
            return largest;
		}
        public static int Max(int a, int b) { return a > b ? a : b; }
        public static int Max(params int[] values)
        {
            int largest = values[0];
            for (int i = 1; i < values.Length; i++)
            {
                if (values[i] > largest) { largest = values[i]; }
            }
            return largest;
        }
        public static float Average (params float[] values)
        {
            float sum = 0;
            for (int i = 0; i < values.Length; i++)
            {
                sum += values[i];
            }
            return sum / values.Length;
        }
        public static float Average(params int[] values)
        {
            float sum = 0;
            for (int i = 0; i < values.Length; i++)
            {
                sum += values[i];
            }
            return sum / values.Length;
        }
        public static float NearestZero (float a, float b)
		{
            return Abs(a) < Abs(b) ? a : b;
		}
        public static float NearestZero(params float[] values)
		{
            float nearest = float.PositiveInfinity;
            int nearestIndex = 0;
			for (int i = 0; i < values.Length; i++)
			{
                float absoluteValue = Abs(values[i]);
                if (absoluteValue < nearest)
				{
                    nearest = absoluteValue;
                    nearestIndex = i;
				}
			}
            return values[nearestIndex];
		}
        public static int NearestZero(int a, int b)
        {
            return Abs(a) < Abs(b) ? a : b;
        }
        public static int NearestZero(params int[] values)
        {
            float nearest = int.MaxValue;
            int nearestIndex = 0;
            for (int i = 0; i < values.Length; i++)
            {
                int absoluteValue = Abs(values[i]);
                if (absoluteValue < nearest)
                {
                    nearest = absoluteValue;
                    nearestIndex = i;
                }
            }
            return values[nearestIndex];
        }
        public static float Truncate (float x) { return (float)System.Math.Truncate((float)x); }
        public static int Floor (float x) { return (int)System.Math.Floor(x); }
        public static int Ceiling (float x) { return (int)System.Math.Ceiling(x); }
        public static float Clamp(float value, float min = 0, float max = 1)
		{
            return System.Math.Clamp(value, min, max);
            /*
			if (value < min) { return min; }
            else if (value > max) { return max; }
            else { return value; }
            */
        }
        public static int Clamp(int value, int min, int max)
        {
            return System.Math.Clamp(value, min, max);
            /*
            if (value < min) { return min; }
            else if (value > max) { return max; }
            else { return value; }
            */
        }
        public static float Lerp (float a, float b, float t)
		{
            return a + (b - a) * Clamp(t);
		}
        public static float Logistic (float x, float max = 1f, float offset = 0.5f, float steepness = 13f)
        {
            return max / (1 + Exp(-steepness * (x - offset)));
        }
        public static Geometry.Vector Circumcenter(Geometry.Vector A, Geometry.Vector B, Geometry.Vector C)
        {
            float d = (A.x * (B.y - C.y) + B.x * (C.y - A.y) + C.x * (A.y - B.y)) * 2;

            Izzy.Geometry.Vector circumcenter = new Izzy.Geometry.Vector
                (
                x: (1 / d) * ((Sq(A.x) + Sq(A.y)) * (B.y - C.y) + (Sq(B.x) + Sq(B.y)) * (C.y - A.y) + (Sq(C.x) + Sq(C.y)) * (A.y - B.y)),
                y: (1 / d) * ((Sq(A.x) + Sq(A.y)) * (C.x - B.x) + (Sq(B.x) + Sq(B.y)) * (A.x - C.x) + (Sq(C.x) + Sq(C.y)) * (B.x - A.x))
                );
            return circumcenter;
        }
        /*
        public static float Gaussian(float x, float peak, float distribution)
        {
            const float inv_sqrt_2pi = 0.3989422804014327f;
            float a = (x - peak) / distribution;
            return inv_sqrt_2pi / distribution * Exp(-0.5f * a * a);
        }
        */
    }
}
