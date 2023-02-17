using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using CliWrap;
using Newtonsoft.Json;

namespace QuoteService
{
	public class FxRateService : IFxRateService
	{
#if DEBUG
		private static readonly string ScriptLocation = @"..\..\..\..\QuoteService";
		private static readonly string ScriptProgram = "python";
#else
		private static readonly string ScriptLocation = @".";
		private static readonly string ScriptProgram = "python3";
#endif
		private static readonly string PointsFileName = "points.out";
		private static readonly string ChartFileName = "image.png";
		private static readonly string DateFormat = "yyyy-MM-dd";

		public async Task<double> GetFxRateAsync(DateTime date)
		{
			var rateResponce = await GetRateResponceAsync(date, date);
			var rates = ParseRates(rateResponce);
			return GetDenominatedValue(rates.Single());
		}

		public async Task<(Stream, string)> GetChartAsync(DateTime startDate)
		{
			var schedule = new List<(DateTime, DateTime)>();
			var currentStartDate = startDate;
			var currentEndDate = new DateTime(startDate.Year, 12, 31);
			var endDate = DateTime.Today;

			while (currentEndDate < endDate)
			{
				schedule.Add((currentStartDate, currentEndDate));

				currentStartDate = currentEndDate.AddDays(1);
				currentEndDate = minDate(endDate, new DateTime(currentStartDate.Year, 12, 31));
			}

			schedule.Add((currentStartDate, endDate));

			var rateResponce = new List<string>();
			foreach (var period in schedule)
			{
				rateResponce.AddRange(await GetRateResponceAsync(period.Item1, period.Item2));
			}

			var rates = ParseRates(rateResponce);
			ExportPoints(rates);
			await ExportChartAsync();

			var stream = new FileStream(ChartFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
			return (stream, ChartFileName);

			static DateTime minDate(DateTime date1, DateTime date2) => date1 < date2 ? date1 : date2;
		}

		private static async Task<IEnumerable<string>> GetRateResponceAsync(DateTime startDate, DateTime endDate)
		{
			var query = HttpUtility.ParseQueryString(string.Empty);
			query["startdate"] = startDate.ToString(DateFormat);
			query["enddate"] = endDate.ToString(DateFormat);

			var uriBuilder = new UriBuilder
			{
				Scheme = "https",
				Host = "www.nbrb.by",
				Query = query.ToString()
			};

			var client = new WebClient();
			var result = new List<string>();

			uriBuilder.Path = "api/exrates/rates/dynamics/145";
			result.Add(await client.DownloadStringTaskAsync(uriBuilder.Uri));

			uriBuilder.Path = "api/exrates/rates/dynamics/431";
			result.Add(await client.DownloadStringTaskAsync(uriBuilder.Uri));

			return result;
		}

		private static IEnumerable<FxRate> ParseRates(IEnumerable<string> responce)
		{
			return responce.SelectMany(r => JsonConvert.DeserializeObject<List<FxRate>>(r)!).OrderBy(r => r.Date);
		}

		private static double GetDenominatedValue(FxRate rate)
		{
			var denominationDate1 = new DateTime(2000, 1, 1);
			var denominationDate2 = new DateTime(2016, 1, 1);

			return (rate.Date, denominationDate1, denominationDate2) switch
			{
				(var d, var d1, _) when d < d1 => rate.Value / 1000 / 10000,
				(var d, var d1, var d2) when d1 <= d && d < d2 => rate.Value / 10000,
				_ => rate.Value
			};
		}

		private static void ExportPoints(IEnumerable<FxRate> rates)
		{
			using var file = new StreamWriter(PointsFileName);

			foreach (var rate in rates)
			{
				var denominatedValue = GetDenominatedValue(rate);
				file.WriteLine($"{rate.Date.ToString(DateFormat)},{denominatedValue}");
			}
		}

		private static async Task ExportChartAsync()
		{
			await Cli.Wrap(ScriptProgram)
				.WithArguments($"{Path.Combine(ScriptLocation, "export_chart.py")} {PointsFileName} {ChartFileName}")
				.ExecuteAsync();
		}
	}
}
