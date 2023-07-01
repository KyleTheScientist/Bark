using System.Collections.Generic;

namespace Bark.Extensions
{
    public static class MathExtensions
    {
        public static int Wrap(int x, int min, int max)
        {
            if (x < min) x = max - (min - x) + 1;
            if (x > max) x = min + (max - x) + 1;
            return x;
        }

        public static float Map(float x, float a1, float a2, float b1, float b2)
        {
            // Calculate the range differences
            float inputRange = a2 - a1;
            float outputRange = b2 - b1;

            // Calculate the normalized value of x within the input range
            float normalizedValue = (x - a1) / inputRange;

            // Map the normalized value to the output range
            float mappedValue = b1 + (normalizedValue * outputRange);

            return mappedValue;
        }


        private static System.Random rng = new System.Random();
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
