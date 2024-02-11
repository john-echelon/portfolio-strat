using System.Collections.ObjectModel;

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
    C[((1+r/n)^nt) - 1/r]
    Future Value of an Annuity Due formula:
    C[((1+r/n)^nt) - 1/r] * (1+r/n)

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
		/*
		// TODO: Reuse for unit tests
		CompoundingStrategy strat1 = new CompoundingStrategy
		{
			Principal = 10000,
			Interval = 12,
			Contributions = 10000 / (12 * 10),
			Principal2 = 10000,
			Rate = 0.15,
			Years = 10,
			rate_rec_gain = 3.00,
			rate_rec_drop = .50,
			years_rec_recover = 0.5,
			IsOrdinaryAnnuity = true,
			IncludeRecessionStrategy = true,
		};
		CalculateStrategy(strat1);
		*/
		var strategies = GenerateStrategies();
		foreach (var strat in strategies) {
			// Console.WriteLine("Strategy Allocations: {0:##,#.00} {1:#,##.00} {2:#,##.00}", strat.Principal, strat.Principal2, strat.Contributions);
			CalculateStrategy(strat, false);
		}
		var result = strategies.OrderByDescending(s => s.max_balance).ToList();
		Console.WriteLine("Strategy Allocations:");
		Console.WriteLine("Principal | Lump Sum | Contribution | Rate | Years | Worst | Best\n");
		foreach (var strat in result) {
			Console.WriteLine("{0,10:,0.00} {1,15:,0.00} {2,15:0,0.00} {3,15:,0.00} {4,10:0} {5,15:,0.00} {6,15:,0.00}",
				strat.Principal, strat.Principal2, strat.Contributions, strat.Rate, strat.Years, strat.min_balance, strat.max_balance);
		}
		var optimal = result[0];
		Console.WriteLine("\nShowing yearly breakdown for Optimal Allocation: {0:,0.00} {1:,0#.00} {2:,0.00}",
			optimal.Principal, optimal.Principal2, optimal.Contributions);
		CalculateStrategy(optimal, true);
	}
	private static List<Models.InvestmentStrategy> GenerateStrategies(double amount = 10000)
	{
		var allocationPcts = new double[] { 1.00, .75, .50, .33, .25, 0.0 };
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
				var allocs = new Tuple<double, double, double> (alloc1, alloc2, alloc3);
				allocations.Add(allocs);
			}
		}
		foreach(var s in allocations) {
			// Console.WriteLine("{0:##,#.00} {1:#,##.00} {2:#,##.00} {3:#,##.00}", s.Item1, s.Item2, s.Item3, s.Item1 + s.Item2 + s.Item3);
			var strat = new InvestmentStrategy
			{
				Principal = s.Item1,
				Interval = 12,
				Contributions = s.Item2 / (12 * 10),
				Principal2 = s.Item3,
				Rate = 0.15,
				Years = 10,
				rate_rec_drop = 0.5,
				timespan_recovery = 1,
				IsOrdinaryAnnuity = true,
				IncludeRecessionStrategy = true,
				min_balance = double.MaxValue
			};
			strats.Add(strat);
		}
		return strats;
	}

	private static void CalculateStrategy(InvestmentStrategy strat1, bool verbose = false)
	{
		if (!strat1.IncludeRecessionStrategy)
		{
			if (verbose)
				Console.WriteLine("Period | Accrued Principal | Accrued Contributions | Balance\n");
			for (var i = 1; i <= strat1.Years; i++)
			{
				var a = CompoundInterestCalculator(strat1.Principal, strat1.Rate, i);
				var contribs = AnnuityCalculator(strat1.Contributions, strat1.Rate, i, strat1.Interval, strat1.IsOrdinaryAnnuity);
				if (verbose)
					Console.WriteLine("{0:00} {1:##,#.00} {2:#,##.00} {3:#,##.00}", i, a, contribs, a + contribs);
				if (i == strat1.Years)
					strat1.max_balance = Math.Max(strat1.max_balance, a + contribs);
					strat1.min_balance = Math.Min(strat1.min_balance, a + contribs);
			}
			return;
		}
		// i: recessionary period
		for (var i = 1; i <= strat1.Years; i++)
		{
			double a = strat1.Principal;
			double contribs = 0;

			// Time: Pre Recession
			if (verbose)
				Console.WriteLine("Period | Accrued Principal | Accrued Contributions | Balance | Rate | Time Elapsed\n");
			for (var j = 1; j < i; j++)
			{
				a = CompoundInterestCalculator(strat1.Principal, strat1.Rate, j);
				contribs = AnnuityCalculator(strat1.Contributions, strat1.Rate, j, strat1.Interval, strat1.IsOrdinaryAnnuity);
				if (verbose)
					Console.WriteLine("{0:00} {1,15:0,0.00} {2,15:0,0.00} {3,15:0,0.00}", j, a, contribs, a + contribs);
			}

			// Intermittent period for Recession
			var balance_before_rec = a + contribs;
			// Account for recessionary period when it exceeds strategy timespan; allow calculators to stop at end of strategy timespan
			var timespan_rec = Math.Min(strat1.timespan_recovery, strat1.Years - i + 1);
			var rec_rate = strat1.Interval*(Math.Pow(1.0/(1.0 * strat1.rate_rec_drop), 1 / (strat1.Interval * strat1.timespan_recovery))-1);
			a = balance_before_rec * strat1.rate_rec_drop + strat1.Principal2;
			a = CompoundInterestCalculator(a, rec_rate, timespan_rec);
			// Note new starting principal includes pre-recession balance from pre-recession contributions; accrued contributions reset to 0
			contribs = AnnuityCalculator(strat1.Contributions, rec_rate, timespan_rec, strat1.Interval, strat1.IsOrdinaryAnnuity);

			if (verbose)
				Console.WriteLine("{0:00} {1,15:0,0.00} {2,15:0,0.00} {3,15:0,0.00} {4,10:0.00} {5,10:0.00} <- Recessionary period", i, a, contribs,a + contribs, rec_rate, timespan_rec);
			// get final balance if recession occurs in the last period
			if (strat1.Years - i + 1 - strat1.timespan_recovery <= 0) {
				strat1.max_balance = Math.Max(strat1.max_balance, a + contribs);
				strat1.min_balance = Math.Min(strat1.min_balance, a + contribs);
			}

			// Time: Post Recession
			// Note new starting principal includes contributions during intermittent period; accrued contributions reset to 0 
			var min_balance = Double.MaxValue;
			for (var j = i + 1; j <= 1 + strat1.Years - strat1.timespan_recovery; j++)
			{
				var a2 = CompoundInterestCalculator(a + contribs, strat1.Rate, j - i);
				var contribs2 = AnnuityCalculator(strat1.Contributions, strat1.Rate, j - i, strat1.Interval, strat1.IsOrdinaryAnnuity);
				if (verbose)
					Console.WriteLine("{0:00} {1,15:0,0.00} {2,15:0,0.00} {3,15:0,0.00}", j, a2, contribs2, a2 + contribs2);
				// get final balance
				strat1.max_balance = Math.Max(strat1.max_balance, a2 + contribs2);
				// if (j == strat1.Years)
				min_balance = a2 + contribs2;
			}
			strat1.min_balance = Math.Min(strat1.min_balance, min_balance);
		}
	}
}
