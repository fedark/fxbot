using System;
using System.IO;
using System.Threading.Tasks;

namespace QuoteService
{
	public interface IFxRateService
	{
		Task<(Stream, string)> GetChartAsync(DateTime startDate);
		Task<double> GetFxRateAsync(DateTime date);
	}
}