namespace PortfolioStrat.Console;

public static class HelpText
{
  public static string Usage = @"Usage Examples:

An investment fund allocation report, 9% annual rate of return, 10k total fund allocation over the span of 10 years (default):

$ foliostrat -a 10000 -t 10 -r 0.09 --drop-pct 0.47 --time-to-recover 0.50

An investment fund allocation report with 15% annual rate of return with no recessionary event:

$ foliostrat -r 0.15 --include-recession=false

An investment fund allocation report with 15% annual rate of return with a yearly breakdown for allocation strategy #0:

$ foliostrat -r 0.15 -b 0

An investment fund with a starting total allocation of 100k that spans 33 years with a max time to recover of 13.5 years after a market recessionary event:

$ foliostrat -a 100000 -t 33 --time-to-recover 13.5
";

}
