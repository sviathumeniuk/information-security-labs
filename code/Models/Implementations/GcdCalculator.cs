namespace code.Models;

public class GcdCalculator : IGcdCalculator
{
    public int Calculate(int a, int b)
    {
        while (b != 0)
        {
            int temp = b;
            b = a % b;
            a = temp;
        }
        return a;
    }
}