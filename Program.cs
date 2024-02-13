using System.Collections.ObjectModel;
using System.Runtime;

namespace PortfolioStrat.Models;

class Program
{
	/*
		Compounding interest formula:
		A = P(1+r/n)^tn
  */
	static double CompoundInterestCalculator(double principal, double rate, double year, int interval = 12)
	{
		var r = rate / interval;
		return principal * Math.Pow(1 + r, year * interval);
	}
	/*
    Future Value of an Annuity formula (ordinary annuity):
    C[((1+r/n)^nt) - 1/(r/n)]
    Future Value of an Annuity Due formula:
    C[((1+r/n)^nt) - 1/(r/n)] * (1+r/n)

		isOrdinaryAnnuity: pays interests at the end of a particular period.
  */
	static double AnnuityCalculator(double contribution, double rate, double year, int interval = 12, bool isOrdinaryAnnuity = true)
	{
		var r = rate / interval;
		var result = contribution * ((Math.Pow(1 + r, year * interval) - 1) / r);
		return isOrdinaryAnnuity ? result : result * (1 + r);
	}
	static void Main(string[] args)
	{
		var amount = 100000.0;
		var strategies = GenerateStrategies(amount);
		foreach (var strat in strategies)
		{
			// Console.WriteLine("Strategy Allocations: {0:##,#.00} {1:#,##.00} {2:#,##.00}", strat.Principal, strat.Principal2, strat\.Contribution);
			CalculateStrategy(strat, false);
		}
		var result = strategies.OrderByDescending(s => s.max_balance).ToList();
		var single = result[0];

		Console.WriteLine("Portfolio setup:");
		Console.WriteLine("Total amount allocated: {0:0,0.00}", amount);
		Console.WriteLine("Timespan (y): {0:,0.00}", single.Period);
		Console.WriteLine("Rate: {0:,0.0000}", single.Rate);
		Console.WriteLine("Contribution Interval: {0:,0.00}", single.Interval2);
		Console.WriteLine("Drop % during recession: {0:,0.00}", single.rec_drop_pct * 100);
		Console.WriteLine("Time to recover (y): {0:0.00}", single.timespan_recovery);

		Console.WriteLine("\nStrategy Allocations:");
		var captionHeader = "#    |  Principal               |    Contribution               |        Lump Sum               |           Worst  |          Best";
		Console.WriteLine(captionHeader);
		Console.WriteLine(new String('-', captionHeader.Length));
		var i = 0;
		foreach (var strat in result)
		{
			var contribution = strat.Contribution * strat.Interval2 * strat.Period;
			Console.WriteLine("{0:000}  | {1,10:0,0.00}  {2,10:0.00}%  | {3,15:0,0.00}  {4,10:0.00}%  | {5,15:0,0.00}  {6,10:0.00}%  | {7,15:0,0.00}  | {8,15:0,0.00}",
				i++, strat.Principal, strat.Principal / amount * 100, contribution, contribution / amount * 100, strat.Principal2, strat.Principal2 / amount * 100, strat.min_balance, strat.max_balance);
		}
		var index = result.Count - 1;
		var selection = result[index];
		selection.IncludeRecessionStrategy = false;
		Console.WriteLine("\nShowing yearly breakdown for Allocation: {0:,0}", index);
		CalculateStrategy(selection, true);
	}
	private static List<Models.InvestmentStrategy> GenerateStrategies(double amount = 10000)
	{
		var allocationPcts = new double[] { 1.00, .80, .75, .666666, .60, .50, .40, .333333, .30, .25, .20, .10, 0.0 };
		// var allocationPcts = new double[] { 1.00, .75, .666666, .50, .333333, .25, 0.0 };
		var strats = new List<InvestmentStrategy>();
		var allocations = new HashSet<Tuple<double, double, double>>();
		// Generate set allocations of principal, contributions, lumpsum:
		for (var i = 0; i < allocationPcts.Length; i++)
		{
			for (var j = 0; j < allocationPcts.Length; j++)
			{
				var alloc1 = amount * allocationPcts[i];
				var alloc2 = (amount - alloc1) * allocationPcts[j];
				var alloc3 = amount - alloc1 - alloc2;
				var allocs = new Tuple<double, double, double>(alloc1, alloc2, alloc3);
				allocations.Add(allocs);
			}
		}
		var duration = 10;
		var interval2 = 12;
		foreach (var s in allocations)
		{
			// Console.WriteLine("{0:##,#.00} {1:#,##.00} {2:#,##.00} {3:#,##.00}", s.Item1, s.Item2, s.Item3, s.Item1 + s.Item2 + s.Item3);
			var strat = new InvestmentStrategy
			{
				Principal = s.Item1,
				Interval = 1,
				Interval2 = interval2,
				Contribution = s.Item2 / (interval2 * duration),
				Principal2 = s.Item3,
				Rate = 0.15,
				Period = duration,
				rec_drop_pct = 0.5,
				timespan_recovery = 0.5,
				IsOrdinaryAnnuity = true,
				IncludeRecessionStrategy = true,
				min_balance = double.MaxValue
			};
			strats.Add(strat);
		}
		return strats;
	}

	public static void CalculateStrategy(InvestmentStrategy strat1, bool verbose = false)
	{
		var captionHeader2 = "Period |    FV Principal  | FV Contributions |         Balance  |       Rate";
		if (!strat1.IncludeRecessionStrategy)
		{
			if (verbose)
				Console.WriteLine($"\n{ captionHeader2}");
			var start_principal = strat1.Principal;
			var contribution = strat1.Contribution;
			var rate = strat1.Rate;
			var t_remaining = strat1.Period;

			var results = CalculateFV(strat1, verbose, start_principal, contribution, rate, t_remaining);
			var lastResult = results[results.Count - 1];
			strat1.min_balance = strat1.max_balance = lastResult.Item2 + lastResult.Item3;
			return;
		}
		// i: designates the recessionary period; precision rounded by year
		for (var i = 0; i < strat1.Period; i++)
		{
			if (verbose)
			{
				Console.WriteLine($"\n{captionHeader2}\n{new String('-', captionHeader2.Length)}");
			}
			// Period: Pre Recession
			var start_principal = strat1.Principal;
			double contribution = strat1.Contribution;
			var rate = strat1.Rate;
			double t_period = Math.Min(strat1.Period, i);
			double t_elapsed = t_period;

			if (verbose)
			{
				Console.WriteLine("Period: Pre Recession. Timespan {0:0.00}", t_period);
				// Console.WriteLine(new String('-', captionHeader2.Length));
			}

			var results = new List<Tuple<double, double, double>>();
			Tuple<double, double, double> lastResult = new Tuple<double, double, double>(0.0, 0.0, 0.0);
			results = CalculateFV(strat1, verbose, start_principal, contribution, rate, t_period);

			if (verbose)
			{
				Console.WriteLine(new String('-', captionHeader2.Length));
			}

			// Intermittent period of Recession
			// New starting principal: pre-recession fv_principal fv_contributions; accrued contributions reset to 0
			// Introduce lump sum contribution to principal
			if (results.Count == 0)
				start_principal = strat1.Principal * strat1.rec_drop_pct + strat1.Principal2;
			else
			{
				lastResult = results[results.Count - 1];
				start_principal = (lastResult.Item2 + lastResult.Item3) * strat1.rec_drop_pct + strat1.Principal2;
			}
			contribution = strat1.Contribution;
			rate = strat1.Interval * (Math.Pow(1.0 / (1.0 * strat1.rec_drop_pct), 1 / (strat1.Interval * strat1.timespan_recovery)) - 1);
			t_period = Math.Min(strat1.timespan_recovery, strat1.Period - t_elapsed);
			t_elapsed += t_period;

			if (verbose)
			{
				Console.WriteLine("Intermittent period of recession. Timespan {0:0.00}", t_period);
				// Console.WriteLine(new String('-', captionHeader2.Length));
			}

			results.AddRange(CalculateFV(strat1, verbose, start_principal, contribution, rate, t_period));

			if (verbose)
			{
				Console.WriteLine(new String('-', captionHeader2.Length));
			}
			// Period: Post Recession
			// Note new starting principal includes contributions during intermittent period; accrued contributions reset to 0 
			lastResult = results[results.Count - 1];
			start_principal = lastResult.Item2 + lastResult.Item3;
			contribution = strat1.Contribution;
			rate = strat1.Rate;
			t_period = strat1.Period - t_elapsed;

			if (verbose)
			{
				Console.WriteLine("Period: Post Recession. Timespan {0:0.00}", t_period);
				// Console.WriteLine(new String('-', captionHeader2.Length));
			}

			results.AddRange(CalculateFV(strat1, verbose, start_principal, contribution, rate, t_period));
			lastResult = results[results.Count - 1];
			strat1.min_balance = Math.Min(strat1.min_balance, lastResult.Item2 + lastResult.Item3);
			strat1.max_balance = Math.Max(strat1.max_balance, lastResult.Item2 + lastResult.Item3);

			if (verbose)
			{
				Console.WriteLine(new String('-', captionHeader2.Length));
			}
		}
	}

	private static List<Tuple<double, double, double>> CalculateFV(InvestmentStrategy strat, bool verbose, double start_principal, double contribution, double rate, double t_remaining)
	{
		var result = new List<Tuple<double, double, double>>();
		var t_elapsed = 0.0;
		while (t_remaining > 0)
		{
			var t_interval = Math.Min(1, t_remaining);
			t_elapsed += t_interval;
			var fv_principal = CompoundInterestCalculator(start_principal, rate, t_elapsed, strat.Interval);
			var fv_contributions = AnnuityCalculator(contribution, rate, t_elapsed, strat.Interval2, strat.IsOrdinaryAnnuity);
			result.Add(new Tuple<double, double, double>(t_elapsed, fv_principal, fv_contributions));
			if (verbose)
				Console.WriteLine("{0:00.00}  | {1,15:0,0.00}  | {2,15:0,0.00}  | {3,15:0,0.00}  | {4,10:0.00000}", t_elapsed, fv_principal, fv_contributions, fv_principal + fv_contributions, rate);
			t_remaining -= t_interval;
		}
		return result;
	}
}
