namespace PortfolioStrat.Models;

public class InvestmentStrategy
{
	public double Principal;
	public double Principal2;
	public int Interval;
	public bool IsOrdinaryAnnuity;
	public double Contributions;
	public double Contributions2;
	public double Contributions3;
	public double Rate;
	public double Years;
	public bool IncludeRecessionStrategy;
	public double rate_rec_drop;
	public double timespan_recovery;
	public double max_balance;
	public double min_balance;
	public int recession_period;
}

