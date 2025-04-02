using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CliWrap;
using Microsoft.Extensions.Options;
using QuoteService.Interface;
using QuoteService.Model;

namespace QuoteService.Impl;

public class FxRateService(IOptions<ScriptConfiguration> scriptOptions, IFxRateClient fxRateClient) : IFxRateService
{
	private static readonly DateTime DenominationDate1 = new(2000, 1, 1);
	private static readonly DateTime DenominationDate2 = new(2016, 1, 1);

	#region Public Methods

	public async Task<double> GetFxRateAsync(DateTime date)
	{
		var rateHistory = await fxRateClient.GetHistoryAsync(date, date).ConfigureAwait(false);

		return Denominate(rateHistory.Single());
	}

	public async Task<FileStream> GetChartAsync(DateTime startDate)
	{
		var schedule = GenerateRequestSchedule(startDate, DateTime.Today);

		var rateHistory = new List<FxRate>();

		foreach (var interval in schedule)
		{
			var intervalHistory = await fxRateClient.GetHistoryAsync(interval.Item1, interval.Item2).ConfigureAwait(false);
			rateHistory.AddRange(intervalHistory);
		}

		var orderedRates = rateHistory.OrderBy(r => r.Date);

		ExportPoints(orderedRates);
		await ExportChartAsync().ConfigureAwait(false);

		return new FileStream(scriptOptions.Value.ChartFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
	}
	
	#endregion

	#region Private Methods

	private static List<(DateTime, DateTime)> GenerateRequestSchedule(DateTime startDate, DateTime endDate)
	{
		var schedule = new List<(DateTime, DateTime)>();

		var currentStartDate = startDate;
		var currentEndDate = new DateTime(startDate.Year, 12, 31);

		while (currentEndDate < endDate)
		{
			schedule.Add((currentStartDate, currentEndDate));

			currentStartDate = currentEndDate.AddDays(1);
			currentEndDate = MinDate(endDate, new DateTime(currentStartDate.Year, 12, 31));
		}

		schedule.Add((currentStartDate, endDate));

		return schedule;

		static DateTime MinDate(DateTime date1, DateTime date2) => date1 < date2 ? date1 : date2;
	}

	private static double Denominate(FxRate rate)
	{
		return rate.Date switch
		{
			var d when d < DenominationDate1 => rate.Value / 1000 / 10000,
			var d when DenominationDate1 <= d && d < DenominationDate2 => rate.Value / 10000,
			_ => rate.Value
		};
	}

	private void ExportPoints(IEnumerable<FxRate> rates)
	{
		using var file = new StreamWriter(scriptOptions.Value.PointsFileName);

		foreach (var rate in rates)
		{
			var denominatedValue = Denominate(rate);
			file.WriteLine($"{rate.Date.ToString(scriptOptions.Value.ExportDateFormat)},{denominatedValue}");
		}
	}

	private Task ExportChartAsync()
	{
		return Cli.Wrap(scriptOptions.Value.CliProgram)
			.WithArguments($"{scriptOptions.Value.ScriptFullName} {scriptOptions.Value.PointsFileName} {scriptOptions.Value.ChartFileName}")
			.ExecuteAsync();
	}

	#endregion
}