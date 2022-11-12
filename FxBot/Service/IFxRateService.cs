using System;
using System.IO;
using System.Threading.Tasks;

namespace FxBot
{
	public interface IFxRateService
	{
		Task<(Stream, string)> GetChart(DateTime startDate);
		Task<double> GetFxRate(DateTime date);
	}
}