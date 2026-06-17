using System.Collections.Generic;

namespace TT2026.libraries.Izzy
{
    public static class RandFi
    {
		//Access
		public static int Range (int min, int max)
		{
            return generator.Next(min, max);
		}
        public static int Range(int min, int max, int seed)
        {
            return new System.Random(seed).Next(min, max);
        }
		/// <returns>A value between 0 and <paramref name="weights"/>, weighted by the values of <paramref name="weights"/></returns>
		public static int WeightedRange(int[] weights)
		{
			int numPosibilites = weights.Length;
			int weightsSum = 0;
			int[] maxRollForResult = new int[numPosibilites];
			for (int i = 0; i < numPosibilites; i++)
			{
				weightsSum += weights[i];
				maxRollForResult[i] = weightsSum;
			}
			int random = Range(0, weightsSum);
			for (int i = 0; i < numPosibilites; i++)
			{
				if (random < maxRollForResult[i]) return i;
			}
			throw new System.InvalidOperationException($"Unhandled issue with Random.WeightedRange()");
		}
		// Identical to above, but takes an IList
		public static int WeightedRange(IList<int> weights)
		{
			int numPosibilites = weights.Count;
			int weightsSum = 0;
			int[] maxRollForResult = new int[numPosibilites];
			for (int i = 0; i < numPosibilites; i++)
			{
				weightsSum += weights[i];
				maxRollForResult[i] = weightsSum;
			}
			int random = Range(0, weightsSum);
			for (int i = 0; i < numPosibilites; i++)
			{
				if (random < maxRollForResult[i]) return i;
			}
			throw new System.InvalidOperationException($"Unhandled issue with Random.WeightedRange()");
		}
		// Identical to above, but takes a byte[] array
		/// <returns>A value between 0 and <paramref name="weights"/>.Length, weighted by the values of <paramref name="weights"/></returns>
		public static int WeightedRange(byte[] weights)
		{
			int numPosibilites = weights.Length;
			int weightsSum = 0;
			int[] maxRollForResult = new int[numPosibilites];
			for (int i = 0; i < numPosibilites; i++)
			{
				weightsSum += weights[i];
				maxRollForResult[i] = weightsSum;
			}
			int random = Range(0, weightsSum);
			for (int i = 0; i < numPosibilites; i++)
			{
				if (random < maxRollForResult[i]) return i;
			}
			throw new System.InvalidOperationException($"Unhandled issue with Random.WeightedRange()");
		}
		public static int WeightedRange(IList<byte> weights)
		{
			int numPosibilites = weights.Count;
			int weightsSum = 0;
			int[] maxRollForResult = new int[numPosibilites];
			for (int i = 0; i < numPosibilites; i++)
			{
				weightsSum += weights[i];
				maxRollForResult[i] = weightsSum;
			}
			int random = Range(0, weightsSum);
			for (int i = 0; i < numPosibilites; i++)
			{
				if (random < maxRollForResult[i]) return i;
			}
			throw new System.InvalidOperationException($"Unhandled issue with Random.WeightedRange()");
		}
		public static float Float ()
		{
            return (float)generator.NextDouble();
		}
		public static float Float (float max) => Float() * max;
		public static float Float(float min, float max) => (Float() * (max - min)) + min;
        public static float Float_Seeded(int seed)
		{
            return (float)new System.Random(seed).NextDouble();
		}
		public static float Float_Seeded(float max, int seed) => Float(seed) * max;
		public static float Float_Seeded(float min, float max, int seed) => (Float(seed) * (max - min)) + min;
        public static int Int()
        {
            return generator.Next();
        }
        public static int Int(int max) => generator.Next(max);
        public static int Int(int min, int max) => generator.Next(min, max);
        public static int Int_Seeded(int seed)
        {
            return new System.Random(seed).Next();
        }
        public static int Int_Seeded(int max, int seed) => new System.Random(seed).Next(max);
        public static int Int_Seeded(int min, int max, int seed) => new System.Random(seed).Next(min, max);
        //Core
        static System.Random generator;
        static RandFi ()
		{
            generator = new System.Random();
		}
    }
}
