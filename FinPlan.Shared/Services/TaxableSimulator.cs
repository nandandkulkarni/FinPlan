using System;

namespace FinPlan.Shared.Services
{
    public static class TaxableSimulator
    {
        // Simulate post-tax final balance from initial monthly payment pmt0 (growing annuity, payments at start)
        public static decimal SimulatePostTaxFinalFromPmt(decimal pmt0, decimal monthlyRate, decimal gm, int periods, decimal ordinaryRate, decimal ltgRate, (decimal q, decimal nonq, decimal lt, decimal st) dist, bool roundIntermediate = false)
        {
            if (periods <= 0) return 0m;
            decimal bal = 0m;
            int monthIndex = 0;

            while (monthIndex < periods)
            {
                decimal yearGrowth = 0m;
                for (int m = 0; m < 12 && monthIndex < periods; m++, monthIndex++)
                {
                    var payment = pmt0 * DecimalPow(1m + gm, monthIndex);
                    bal += payment;
                    if (roundIntermediate) bal = Math.Round(bal, 8);
                    var interest = bal * monthlyRate;
                    yearGrowth += interest;
                    bal += interest;
                    if (roundIntermediate) bal = Math.Round(bal, 8);
                }

                var qual = yearGrowth * dist.q;
                var nonq = yearGrowth * dist.nonq;
                var lt = yearGrowth * dist.lt;
                var st = yearGrowth * dist.st;
                var taxes = qual * ltgRate + (nonq + st) * ordinaryRate + lt * ltgRate;
                if (roundIntermediate) taxes = Math.Round(taxes, 8);
                bal -= taxes;
                if (roundIntermediate) bal = Math.Round(bal, 8);
            }

            return bal; // return full-precision final balance; caller can round for display
        }

        public static decimal SolveInitialMonthlyForPostTax(decimal targetNet, decimal monthlyRate, decimal gm, int periods, decimal ordinaryRate, decimal ltgRate, (decimal q, decimal nonq, decimal lt, decimal st) dist, int maxIterations, decimal tolerance, bool roundIntermediate = false)
        {
            if (targetNet <= 0m || periods <= 0) return 0m;

            decimal low = 0m;
            decimal high = Math.Max(100m, targetNet / periods); // heuristic

            for (int expand = 0; expand < 200; expand++)
            {
                var testNet = SimulatePostTaxFinalFromPmt(high, monthlyRate, gm, periods, ordinaryRate, ltgRate, dist, roundIntermediate);
                if (testNet >= targetNet) break;
                high *= 2m;
            }

            decimal result = 0m;
            for (int iter = 0; iter < maxIterations; iter++)
            {
                var mid = (low + high) / 2m;
                var net = SimulatePostTaxFinalFromPmt(mid, monthlyRate, gm, periods, ordinaryRate, ltgRate, dist, roundIntermediate);
                var diff = net - targetNet;
                if (Math.Abs((double)diff) <= (double)tolerance)
                {
                    result = mid;
                    break;
                }
                if (net < targetNet) low = mid; else high = mid;
                result = mid;
            }

            return result;
        }

        // Decimal power helper (integer exponent >= 0)
        private static decimal DecimalPow(decimal x, int pow)
        {
            if (pow == 0) return 1m;
            double dx = (double)x;
            double r = Math.Pow(dx, pow);
            return (decimal)r;
        }
    }
}
