using FinPlan.Shared.Services;
using Xunit;

namespace FinPlan.Shared.Tests
{
    public class TaxableSimulatorTests
    {
        [Fact]
        public void SimulatePostTaxFinalFromPmt_BasicCheck()
        {
            // monthly rate 6% annual => 0.5% per month
            decimal monthlyRate = 0.06m / 12m;
            decimal gm = 0m; // no growth in payments
            int periods = 12 * 32; // 32 years
            decimal ordinaryRate = 0.24m;
            decimal ltgRate = 0.15m;
            var dist = (q: 0.25m, nonq: 0.25m, lt: 0.40m, st: 0.10m);

            decimal pmt0 = 1000m;
            var final = TaxableSimulator.SimulatePostTaxFinalFromPmt(pmt0, monthlyRate, gm, periods, ordinaryRate, ltgRate, dist, false);

            Assert.True(final > 0);
        }

        [Fact]
        public void SolverProducesCloserToTarget_WithMoreIterations()
        {
            decimal monthlyRate = 0.06m / 12m;
            decimal gm = 0m;
            int periods = 12 * 32;
            decimal ordinaryRate = 0.24m;
            decimal ltgRate = 0.15m;
            var dist = (q: 0.25m, nonq: 0.25m, lt: 0.40m, st: 0.10m);

            decimal target = 1000000m / 3m; // one bucket's target

            var p1 = TaxableSimulator.SolveInitialMonthlyForPostTax(target, monthlyRate, gm, periods, ordinaryRate, ltgRate, dist, 50, 0.5m, false);
            var f1 = TaxableSimulator.SimulatePostTaxFinalFromPmt(p1, monthlyRate, gm, periods, ordinaryRate, ltgRate, dist, false);
            var err1 = System.Math.Abs((double)(f1 - target));

            var p2 = TaxableSimulator.SolveInitialMonthlyForPostTax(target, monthlyRate, gm, periods, ordinaryRate, ltgRate, dist, 1000, 0.01m, false);
            var f2 = TaxableSimulator.SimulatePostTaxFinalFromPmt(p2, monthlyRate, gm, periods, ordinaryRate, ltgRate, dist, false);
            var err2 = System.Math.Abs((double)(f2 - target));

            Assert.True(err2 <= err1);
        }

        [Fact]
        public void SolverAndComponentGridSimulation_AgreeForPostTax()
        {
            // Reproduce the component's yearly grid accumulation for a single post-tax bucket
            decimal annualReturn = 0.06m;
            decimal monthlyRate = annualReturn / 12m; // matches component MonthlyRate = AnnualReturnPercent/100/12 when percentages are used as decimals
            decimal gm = 0m;
            int years = 32;
            int periods = years * 12;
            decimal ordinaryRate = 0.24m;
            decimal ltgRate = 0.15m;
            var dist = (q: 0.25m, nonq: 0.25m, lt: 0.40m, st: 0.10m);

            decimal target = 1000000m / 3m;

            // Solve using the shared solver
            var pmt0 = TaxableSimulator.SolveInitialMonthlyForPostTax(target, monthlyRate, gm, periods, ordinaryRate, ltgRate, dist, 1000, 0.01m, false);
            var finalFromSimulator = TaxableSimulator.SimulatePostTaxFinalFromPmt(pmt0, monthlyRate, gm, periods, ordinaryRate, ltgRate, dist, false);

            // Now replicate the component's BuildYearlyGrid accumulation for a single post-tax bucket
            decimal postBal = 0m;
            decimal postNoTaxBal = 0m;
            int monthIndex = 0;
            for (int year = 1; year <= years; year++)
            {
                decimal postYearGrowth = 0m;
                for (int m = 1; m <= 12 && monthIndex < periods; m++, monthIndex++)
                {
                    var payment = pmt0 * DecimalPow(1m + gm, monthIndex);
                    postNoTaxBal += payment;
                    postBal += payment;

                    var interest = postBal * monthlyRate;
                    postYearGrowth += interest;
                    postBal += interest;

                    var interestNoTax = postNoTaxBal * monthlyRate;
                    postNoTaxBal += interestNoTax;
                }

                var qual = postYearGrowth * dist.q;
                var nonq = postYearGrowth * dist.nonq;
                var lt = postYearGrowth * dist.lt;
                var st = postYearGrowth * dist.st;
                var taxes = qual * ltgRate + (nonq + st) * ordinaryRate + lt * ltgRate;
                postBal -= taxes;
            }

            var finalFromGrid = postBal;

            // Allow a small tolerance in comparing the two simulations
            var diff = System.Math.Abs((double)(finalFromSimulator - finalFromGrid));
            Assert.True(diff < 0.0001, $"Simulator and grid disagree: sim={finalFromSimulator} grid={finalFromGrid} diff={diff}");

            // Also ensure simulator produced close to target (sanity)
            var err = System.Math.Abs((double)(finalFromSimulator - target));
            Assert.True(err <= 1.0, $"Final from simulator not within $1 of target: final={finalFromSimulator} target={target} err={err}");
        }

        // Helper decimal pow
        private static decimal DecimalPow(decimal x, int pow)
        {
            if (pow == 0) return 1m;
            double dx = (double)x;
            double r = System.Math.Pow(dx, pow);
            return (decimal)r;
        }
    }
}
