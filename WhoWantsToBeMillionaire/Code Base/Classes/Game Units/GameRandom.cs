using System;
using System.Collections.Generic;
using System.Linq;

namespace WhoWantsToBeMillionaire
{
    static class GameRandom
    {
        private static readonly Random s_random = new Random();

        public static int Next(int maxValue = int.MaxValue)
        {
            return s_random.Next(maxValue);
        }

        public static float NextFloat()
        {
            return Convert.ToSingle(s_random.NextDouble());
        }

        public static T GetRandomElement<T>(IEnumerable<T> values)
        {
            return values.ElementAt(s_random.Next(values.Count()));
        }

        public static bool CheckChance(double probability)
        {
            return s_random.NextDouble() < probability;
        }

        public static void Shuffle<T>(ref T[] array)
        {
            array = array.OrderBy(_ => s_random.Next()).ToArray();
        }
    }
}
