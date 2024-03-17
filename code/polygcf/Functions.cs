
namespace PlutoScarab;

public static class Functions
{
    public static int GCD(int m, int n)
    {
        var sign = m < 0 && n < 0 ? -1 : 1;

        while (n != 0)
        {
            (m, n) = (n, m % n);
        }

        return Math.Abs(m) * sign;
    }

}