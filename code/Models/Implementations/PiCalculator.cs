using static System.Math;

namespace code.Models;

public class PiCalculator : IPiCalculator
{
    private readonly IGcdCalculator _gcdCalculator;

    public PiCalculator(IGcdCalculator gcdCalculator)
    {
        _gcdCalculator = gcdCalculator;
    }

    public (double pi, double error) CalculatePi(int[] numbers, int a, int c, int m)
    {
        int coprimeCount = 0;

        for (int i = 0; i < numbers.Length; i++)
        {
            for (int j = i + 1; j < numbers.Length; j++)
            {
                if (_gcdCalculator.Calculate(numbers[i], numbers[j]) == 1)
                    coprimeCount++;
            }
        }

        double probability = (double)coprimeCount / (numbers.Length * (numbers.Length - 1) / 2);
        double piEstimation = Sqrt(6 / probability);
        double error = Abs(PI - piEstimation);

        return (piEstimation, error);
    }
}