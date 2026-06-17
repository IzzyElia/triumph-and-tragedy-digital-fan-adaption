namespace TT2026.libraries.Izzy.Utils;

public static class ArrayUtils
{
    public static float SumArray(float[] array)
    {
        float sum = 0;

        foreach (float number in array)
        {
            sum += number;
        }

        return sum;
    }
    public static double SumArray(double[] array)
    {
        double sum = 0;

        foreach (double number in array)
        {
            sum += number;
        }

        return sum;
    }
    public static int SumArray(int[] array)
    {
        int sum = 0;

        foreach (int number in array)
        {
            sum += number;
        }

        return sum;
    }
    public static decimal SumArray(decimal[] array)
    {
        decimal sum = 0;

        foreach (decimal number in array)
        {
            sum += number;
        }

        return sum;
    }
    public static long SumArray(long[] array)
    {
        long sum = 0;

        foreach (long number in array)
        {
            sum += number;
        }

        return sum;
    }
    public static short SumArray(short[] array)
    {
        short sum = 0;

        foreach (short number in array)
        {
            sum += number;
        }

        return sum;
    }

}