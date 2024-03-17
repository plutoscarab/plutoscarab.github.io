
namespace PlutoScarab;

public static class LaTeX 
{
    public static string Wrap(object n)
    {
        var s = n.ToString();

        if (s.Length == 1 && !char.IsLetter(s[0]))
            return s;

        return "{" + s + "}";
    }

    public static string Frac(string a, string b)
    {
        if (b == "1") return a;
        return "\\frac" + Wrap(a) + Wrap(b);
    }

    public static string Fracs(string a, string b)
    {
        if (b == "1") return a;
        return a + "/" + b;
    }

    public static string Frac(int a, int b)
    {
        var g = Functions.GCD(a, b);
        (a, b) = (a / g, b / g);
        return Frac(a.ToString(), b.ToString());
    }

    public static string Fracs(int a, int b)
    {
        var g = Functions.GCD(a, b);
        (a, b) = (a / g, b / g);
        return LaTeX.Fracs(a.ToString(), b.ToString());
    }

    public static string Pow(string expr, int a, int b)
    {
        var f = LaTeX.Fracs(a, b);

        if (f == "0")
            return "1";

        if (f == "1")
            return expr;

        if (f == "1/2")
            return "\\sqrt" + Wrap(expr);

        return expr + "^" + Wrap(f);
    }

    public static (int n, int f) SquareFree(int n)
    {
        var s = (int)(Math.Sqrt(n) + .5);

        if (s * s == n)
            return (1, s);

        var f = 1;

        for (var i = 2; i < s; i++)
        {
            var ii = i * i;

            if ((n % ii) == 0)
            {
                f *= i;
                n /= ii;
            }
        }

        return (n, f);
    }

    public static string Sqrt(string n)
    {
        if (n == "1")
            return "1";

        return "\\sqrt" + Wrap(n);
    }

    public static string Sqrt(int n)
    {
        (n, var f) = SquareFree(n);

        if (n == 1)
            return f.ToString();

        if (f > 1)
            return f + "\\sqrt" + Wrap(n);

        return "\\sqrt" + Wrap(n);
    }

    public static string Prod(string a, string b)
    {
        if (a == "1")
            return b;

        if (b == "1")
            return a;

        return a + " " + b;
    }

    public static string OverSqrt(int a, int b)
    {
        // a / Sqrt(b)
        (b, var c) = SquareFree(b); // -> a / (c * Sqrt(b))

        var d = Functions.GCD(a, b);  // (a/d)*Sqrt(d) / (c * Sqrt(b/d))
        (a, b) = (a / d, b / d); // a Sqrt(d) / (c Sqrt(b))

        var g = Functions.GCD(a, c);
        (a, c) = (a / g, c / g);

        if (d == 1)
            return Frac(a.ToString(), Prod(c.ToString(), Sqrt(b)));

        if (b == 1)
            return Frac(Prod(a.ToString(), Sqrt(d)), c.ToString());

        return Prod(Frac(a, c), Sqrt(Frac(d, b)));
    }
}