using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CliWrap;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Options;
using QuoteService.Grpc;
using QuoteService.Interface;
using QuoteService.Model;
using QuoteService.Model.Configuration;

namespace QuoteService.Impl;

public class FxRateServiceImpl(IOptions<ScriptConfiguration> scriptOptions,
	IOptions<ResponseConfiguration> responseOptions,
	IFxRateApiClient fxRateApiClient) 
	: FxRateService.FxRateServiceBase
{
	private static readonly DateTime DenominationDate1 = new(2000, 1, 1);
	private static readonly DateTime DenominationDate2 = new(2016, 1, 1);

	#region Public Methods

	public override async Task<FxRateResponse> GetFxRate(FxRateRequest request, ServerCallContext context)
	{
		var date = request.Date.ToDateTime();
		var rateHistory = await fxRateApiClient.GetHistoryAsync(date, date).ConfigureAwait(false);
		var rate = rateHistory.Single();
		var denominatedRate = Denominate(rate);

		return new FxRateResponse { FxRate = denominatedRate };
	}

	public override async Task GetChart(ChartRequest request, IServerStreamWriter<ChartChunk> responseStream, ServerCallContext context)
	{
		var startDate = request.StartDate.ToDateTime();
		var schedule = GenerateRequestSchedule(startDate, DateTime.Today);

		var rateHistory = new List<FxRate>();

		foreach (var interval in schedule)
		{
			var intervalHistory = await fxRateApiClient.GetHistoryAsync(interval.Item1, interval.Item2).ConfigureAwait(false);
			rateHistory.AddRange(intervalHistory);
		}

		var orderedRates = rateHistory.OrderBy(r => r.Date);

		await ExportPointsAsync(orderedRates).ConfigureAwait(false);
		await ExportChartAsync().ConfigureAwait(false);

		using var fileStream = new FileStream(scriptOptions.Value.ChartFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
		
		var fileSize = fileStream.Length;
		var chunkRatio = responseOptions.Value.GetValidChunkRatio();
        var chunkSize = (int)Math.Floor(fileSize * chunkRatio);

		var buffer = new byte[chunkSize];

        var readSize = await fileStream.ReadAsync(buffer).ConfigureAwait(false);

        while (readSize > 0 && !context.CancellationToken.IsCancellationRequested)
		{
			var chunk = new ChartChunk
			{
				Chunk = ByteString.CopyFrom(buffer, 0, readSize),
				Size = readSize,
			};
			await responseStream.WriteAsync(chunk).ConfigureAwait(false);
		}
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

	private async Task ExportPointsAsync(IEnumerable<FxRate> rates)
	{
		using var file = new StreamWriter(scriptOptions.Value.PointsFileName);

		foreach (var rate in rates)
		{
			var denominatedValue = Denominate(rate);
			await file.WriteLineAsync($"{rate.Date.ToString(scriptOptions.Value.ExportDateFormat)},{denominatedValue}").ConfigureAwait(false);
		}
	}

	private Task ExportChartAsync()
	{
		var target = scriptOptions.Value.CliProgram;
		var script = scriptOptions.Value.ScriptFullName;
		var points = scriptOptions.Value.PointsFileName;
		var chart = scriptOptions.Value.ChartFileName;

		return Cli.Wrap(target)
			.WithArguments($"{script} {points} {chart}")
			.ExecuteAsync();
	}

	#endregion
}