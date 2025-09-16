using System;
using System.Collections.Generic;

namespace code.Models;

public class RandomGenerator : IRandomGenerator
{
    public IEnumerable<int> Generate(int count, int seed, int a, int c, int m)
    {
        int x = seed;
        for (int i = 0; i < count; ++i)
        {
            x = (a * x + c) % m;
            yield return x;
        }
    }

    public IEnumerable<int> GenerateSystemRandom(int count, int seed, int maxValue)
    {
        var random = new Random(seed);
        for (int i = 0; i < count; ++i)
            yield return random.Next(0, maxValue);
    }

    public int CalculatePeriod(int seed, int a, int c, int m)
    {
        var seen = new HashSet<int>();
        int x = seed;
        int period = 0;

        while (true)
        {
            x = (a * x + c) % m;
            period++;

            if (seen.Contains(x))
                return period - 1;

            if (period > m)
                return -1;

            seen.Add(x);
        }
    }
}
