using System.Collections.Generic;

namespace code.Models;

public interface IRandomGenerator
{
    IEnumerable<int> Generate(int count, int seed, int a, int c, int m);
    IEnumerable<int> GenerateSystemRandom(int count, int seed, int maxValue);
    int CalculatePeriod(int seed, int a, int c, int m);
}