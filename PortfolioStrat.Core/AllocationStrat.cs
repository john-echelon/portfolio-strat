namespace PortfolioStrat.Core;

public class AllocationStrat
{
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
	public static List<Tuple<double, double, double>> CalculateFV(InvestmentStrategy strat, bool verbose, double start_principal, double contribution, double rate, double t_remaining)
	{
		var result = new List<Tuple<double, double, double>>();
		var t_elapsed = 0.0;
		while (t_remaining > 0)
		{
			var t_interval = Math.Min(1, t_remaining);
			t_elapsed += t_interval;
			var fv_principal = Utils.CompoundInterestCalculator(start_principal, rate, t_elapsed, strat.Interval);
			var fv_contributions = Utils.AnnuityCalculator(contribution, rate, t_elapsed, strat.Interval2, strat.IsOrdinaryAnnuity);
			result.Add(new Tuple<double, double, double>(t_elapsed, fv_principal, fv_contributions));
			if (verbose)
				Console.WriteLine("{0:00.00}  | {1,15:0,0.00}  | {2,15:0,0.00}  | {3,15:0,0.00}  | {4,10:0.00000}", t_elapsed, fv_principal, fv_contributions, fv_principal + fv_contributions, rate);
			t_remaining -= t_interval;
		}
		return result;
	}
}
