namespace code.Models;

public interface IPiCalculator
{
    (double pi, double error) CalculatePi(int[] numbers, int a, int c, int m);
}