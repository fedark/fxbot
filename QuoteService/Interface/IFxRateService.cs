using System;
using System.IO;
using System.Threading.Tasks;

namespace QuoteService.Interface;

public interface IFxRateService
{
	Task<double> GetFxRateAsync(DateTime date);
	Task<FileStream> GetChartAsync(DateTime startDate);
}