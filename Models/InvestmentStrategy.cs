namespace PortfolioStrat.Models;

public class InvestmentStrategy
{
	public double Principal;
	public double Principal2;
	public int Interval;
	public int Interval2;
	public bool IsOrdinaryAnnuity;
	public double Contribution;
	public double Contribution2;
	public double Contribution3;
	public double Rate;
	public double Period;
	public bool IncludeRecessionStrategy;
	public double rec_drop_pct;
	public double timespan_recovery;
	public double max_balance;
	public double min_balance;
	public int recession_period;
}

