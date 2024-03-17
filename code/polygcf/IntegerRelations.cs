
namespace PlutoScarab;

public static class IntegerRelations
{
    public delegate bool Relation(MpfrFloat x, string xs, double y, out MpfrFloat z, out string scf);

    public static bool Quadratic(MpfrFloat x, string xs, double y, out MpfrFloat z, out string scf)
    {
        z = default;
        scf = default;

        // (a + bx + cx^2) / d = y
        // a + bx + cx^2 - dy = 0
        var pslq = PSLQ(new[] { 1.0, (double)x, (double)(x * x), -y });

        if (pslq[3] == 0)
            return false;

        z = (pslq[0] + x * (pslq[1] + x * pslq[2])) / pslq[3];

        if (z == 0 || MpfrFloat.Abs(x / z - 1, null) > 1e-6)
            return false;

        scf = TeX.Frac(Poly.ToFactoredString(pslq[0..3], xs), pslq[3].ToString());
        return true;
    }

    public static bool RationalPower(MpfrFloat x, string xs, double y, out MpfrFloat z, out string scf)
    {
        z = default;
        scf = default;
        if (x <= 0 || y <= 0) return false;

        // x^(a/b) = y
        // a log x - b log y = 0
        var pslq = PSLQ(new[] { Math.Log((double)x), -Math.Log(y) });
        z = MpfrFloat.Power(x, pslq[0] / (MpfrFloat)pslq[1]);

        if (z == 0 || MpfrFloat.Abs(x / z - 1, null) > 1e-6)
            return false;

        scf = TeX.Pow(xs, pslq[0], pslq[1]);
        return true;
    }

    public static bool MobiusTransform(MpfrFloat x, string xs, double y, out MpfrFloat z, out string scf)
    {
        // (a + bx) / (c + dx) = y
        // a + bx - cy - dxy = 0
        var pslq = PSLQ(new[] { 1.0, (double)x, -y, -y * (double)x });

        if (pslq[0] * pslq[3] == pslq[1] * pslq[2])
        {
            // result is rational number not dependent on x
            z = default;
            scf = default;
            return false;
        }

        if (pslq[0] + x * pslq[1] < 0)
        {
            // Negate all coefficients
            for (var i = 0; i < pslq.Length; i++) pslq[i] *= -1;
        }

        z = (pslq[0] + x * pslq[1]) / (pslq[2] + x * pslq[3]);

        if (pslq[2] == 0)
        {
            var num = Poly.ToFactoredString(new[] { pslq[0] }, xs);
            var den = Poly.ToFactoredString(new[] { 0, pslq[3] }, xs);

            if (pslq[1] == 0)
                scf = TeX.Frac(num, den);
            else if (pslq[1] * pslq[3] < 0)
                scf = TeX.Frac(num, den) + "-" + TeX.Frac(-pslq[1], pslq[3]);
            else
                scf = TeX.Frac(num, den) + "+" + TeX.Frac(pslq[1], pslq[3]);
        }
        else if (pslq[3] == 0)
        {
            var num = Poly.ToFactoredString(new[] { 0, pslq[1] }, xs);
            var den = Poly.ToFactoredString(new[] { pslq[2] }, xs);

            if (pslq[0] == 0)
                scf = TeX.Frac(num, den);
            else if (pslq[0] * pslq[2] < 0)
                scf = TeX.Frac(num, den) + "-" + TeX.Frac(-pslq[0], pslq[2]);
            else
                scf = TeX.Frac(num, den) + "+" + TeX.Frac(pslq[0], pslq[2]);
        }
        else
        {
            var num = Poly.ToFactoredString(new[] { pslq[0], pslq[1] }, xs);
            var den = Poly.ToFactoredString(new[] { pslq[2], pslq[3] }, xs);
            scf = TeX.Frac(num, den);
        }

        return true;
    }

    static int[] PSLQ(double[] x)
    {
        var γ = 2 / Math.Sqrt(3);
        var n = x.Length;
        var A = new int[n + 1, n + 1];
        var B = new int[n + 1, n + 1];

        for (var i = 1; i <= n; i++)
            B[i, i] = A[i, i] = 1;

        var s = new double[n + 1];

        for (var k = 1; k <= n; k++)
            s[k] = Math.Sqrt(Enumerable.Range(k, n - k + 1).Select(j => x[j - 1] * x[j - 1]).Sum());

        var y = new double[n + 1];
        var t = s[1];

        for (var k = 1; k <= n; k++)
        {
            y[k] = x[k - 1] / t;
            s[k] /= t;
        }

        var H = new double[n + 1, n];

        for (var i = 1; i <= n; i++)
        {
            if (i < n) H[i, i] = s[i + 1] / s[i];

            for (var j = 1; j < i; j++)
                H[i, j] = -y[i] * y[j] / (s[j] * s[j + 1]);
        }

        for (var i = 2; i <= n; i++)
        {
            for (var j = i - 1; j >= 1; j--)
            {
                var u = (int)Math.Round(H[i, j] / H[j, j]);
                y[j] += u * y[i];

                for (var k = 1; k <= j; k++)
                    H[i, k] -= u * H[j, k];

                for (var k = 1; k <= n; k++)
                {
                    A[i, k] -= u * A[j, k];
                    B[k, j] += u * B[k, i];
                }
            }
        }

        while (true)
        {
            var max = double.MinValue;
            var m = -1;

            for (var i = 1; i < n; i++)
            {
                var q = Math.Pow(γ, i) * Math.Abs(H[i, i]);
                if (q > max) { max = q; m = i; }
            }

            (y[m], y[m + 1]) = (y[m + 1], y[m]);

            for (var i = 1; i <= n; i++)
                (A[m, i], A[m + 1, i]) = (A[m + 1, i], A[m, i]);

            for (var i = 1; i < n; i++)
                (H[m, i], H[m + 1, i]) = (H[m + 1, i], H[m, i]);

            for (var i = 1; i <= n; i++)
                (B[i, m], B[i, m + 1]) = (B[i, m + 1], B[i, m]);

            if (m <= n - 2)
            {
                var t0 = Math.Sqrt(Math.Pow(H[m, m], 2) + Math.Pow(H[m, m + 1], 2));
                var t1 = H[m, m] / t0;
                var t2 = H[m, m + 1] / t0;

                for (var i = m; i <= n; i++)
                {
                    var t3 = H[i, m];
                    var t4 = H[i, m + 1];
                    H[i, m] = t1 * t3 + t2 * t4;
                    H[i, m + 1] = -t2 * t3 + t1 * t4;
                }
            }

            for (var i = m + 1; i <= n; i++)
            {
                for (var j = Math.Min(i - 1, m + 1); j >= 1; j--)
                {
                    var u = (int)Math.Round(H[i, j] / H[j, j]);
                    y[j] += u * y[i];

                    for (var k = 1; k <= j; k++)
                        H[i, k] -= u * H[j, k];

                    for (var k = 1; k <= n; k++)
                    {
                        A[i, k] -= u * A[j, k];
                        B[k, j] += u * B[k, i];
                    }
                }
            }

            var min = double.MaxValue;
            var c = -1;

            for (var i = 1; i <= n; i++)
            {
                if (Math.Abs(y[i]) < min) { min = Math.Abs(y[i]); c = i; }
            }

            if (min < 1e-8)
            {
                var result = new int[n];

                for (var i = 1; i <= n; i++)
                    result[i - 1] = B[i, c];

                return result;
            }
        }
    }
}