namespace PortfolioStrat.Core;
public static class Utils {
	/*
		Compounding interest formula:
		A = P(1+r/n)^tn
  */
	public static double CompoundInterestCalculator(double principal, double rate, double year, int interval = 12)
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
	public static double AnnuityCalculator(double contribution, double rate, double year, int interval = 12, bool isOrdinaryAnnuity = true)
	{
		var r = rate / interval;
		var result = contribution * ((Math.Pow(1 + r, year * interval) - 1) / r);
		return isOrdinaryAnnuity ? result : result * (1 + r);
	}
}