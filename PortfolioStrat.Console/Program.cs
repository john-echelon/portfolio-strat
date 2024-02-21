using System.Collections.ObjectModel;
using System.Runtime;
using PortfolioStrat.Core;
using CommandLine;
using CommandLine.Text;
using Microsoft.VisualBasic;

class Program
{
	class AllocBase
	{
		[Option(
			'a', "amount",
			Default = 10000,
			HelpText = "Total [a]mount to allocate.")]
		public double Amount { get; set; }

		[Option('t', "timespan", Default = 10.0, HelpText = "The [t]imespan of the investment given in years.")]
		public double Timespan { get; set; }
		[Option(
			'r', "rate",
			Default = 0.09,
			HelpText = "The interest [r]ate of the investment.")]
		public double Rate { get; set; }
		[Option(
			'c', "contrib-interval",
			Default = 12,
			HelpText = "The number of contributions in a given year.")]
		public int Interval2 { get; set; }
		[Option(
			"include-recession",
			Default = true,
			HelpText = "Account for a recessionary event or market trough.")]
		public bool IncludeRecessionStrategy { get; set; }
		[Option(
			"drop-pct",
			Default = 0.47,
			HelpText = "The drop % of the investment in the event of a recession or market trough.")]
		public double DropPct { get; set; }
		[Option(
			"time-to-recover",
			Default = 0.50,
			HelpText = "The time to recover(y) in the event of a recession or market trough.")]
		public double TimeToRecover { get; set; }
	}

	[Verb("alloc", HelpText = "Run allocation test cases.")]
	class AllocOptions: AllocBase
	{
		[Option(
			'b', "show-yearly-breakdown",
			Default = -1,
			HelpText = "Show yearly [b]reakdown for a given strategy.")]
		public int ShowYearlyBreakdown { get; set; }
	}

	static void Main(string[] args)
	{ 
		var parser = new Parser(with => with.HelpWriter = null);
		var parserResult = parser.ParseArguments<AllocOptions>(args);
		parserResult.WithParsed((AllocOptions opts) => RunAllocations(opts))
		.WithNotParsed(errs => DisplayHelp(parserResult, errs));
	}
	static void DisplayHelp<T>(ParserResult<T> result, IEnumerable<Error> errs)
	{
		HelpText? helpText = null;
		if (errs.IsVersion())  //check if error is version request
			helpText = HelpText.AutoBuild(result);
		else
		{
			helpText = HelpText.AutoBuild(result, h =>
			{
				//configure help
				h.AdditionalNewLineAfterOption = false;
				h.AddPostOptionsText(PortfolioStrat.Console.HelpText.Usage);
				return HelpText.DefaultParsingErrorsHandler(result, h);
			}, e => e);
		}
		Console.WriteLine(helpText);
	}

	static int RunAllocations(AllocOptions opts)
	{
		var amount = opts.Amount;
		var strategies = GenerateStrategies(opts);
		foreach (var strat in strategies)
		{
			// Console.WriteLine("Strategy Allocations: {0:##,#.00} {1:#,##.00} {2:#,##.00}", strat.Principal, strat.Principal2, strat\.Contribution);
			AllocationStrat.CalculateStrategy(strat, false);
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
		if (opts.ShowYearlyBreakdown >= 0) {
			var index = opts.ShowYearlyBreakdown;
			var selection = result[index];
			selection.IncludeRecessionStrategy = opts.IncludeRecessionStrategy;
			Console.WriteLine("\nShowing yearly breakdown for Allocation: {0:,0}", index);
			AllocationStrat.CalculateStrategy(selection, true);
		}
		return 0;
	}

	private static List<InvestmentStrategy> GenerateStrategies(AllocOptions opts)
	{
		var amount = opts.Amount;
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
		var duration = opts.Timespan;
		var interval2 = opts.Interval2;
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
				Rate = opts.Rate,
				Period = duration,
				rec_drop_pct = opts.DropPct,
				timespan_recovery = opts.TimeToRecover,
				IsOrdinaryAnnuity = true,
				IncludeRecessionStrategy = opts.IncludeRecessionStrategy,
				min_balance = double.MaxValue
			};
			strats.Add(strat);
		}
		return strats;
	}

}
