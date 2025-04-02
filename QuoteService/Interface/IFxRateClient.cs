using QuoteService.Model;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace QuoteService.Interface;

public interface IFxRateClient
{
	Task<IEnumerable<FxRate>> GetHistoryAsync(DateTime startDate, DateTime endDate);
}