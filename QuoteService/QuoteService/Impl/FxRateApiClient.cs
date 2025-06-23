using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using System;
using System.Net.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using QuoteService.Interface;
using QuoteService.Model;
using Mapster;
using QuoteService.Model.Configuration;

namespace QuoteService.Impl;

public class FxRateApiClient : HttpClient, IFxRateApiClient
{
	private readonly IOptions<FxRateApiConfiguration> _clientOptions;

    public FxRateApiClient(IOptions<FxRateApiConfiguration> clientOptions)
	{
		_clientOptions = clientOptions;
        BaseAddress = new Uri(clientOptions.Value.BaseUrl, UriKind.Absolute);
	}

	public async Task<IEnumerable<FxRate>> GetHistoryAsync(DateTime startDate, DateTime endDate)
	{
		var query = BuildQuery(startDate, endDate);

		var beforeDenominationResult = await ProcessRequestAsync($"{_clientOptions.Value.PathBeforeDenomination}{query}").ConfigureAwait(false);
		var afterDenominationResult = await ProcessRequestAsync($"{_clientOptions.Value.PathAfterDenomination}{query}").ConfigureAwait(false);

		var result = new List<FxRateApiModel>();

		result.AddRange(beforeDenominationResult);
		result.AddRange(afterDenominationResult);

		return result.Adapt<IEnumerable<FxRate>>();
	}

	private string BuildQuery(DateTime startDate, DateTime endDate)
	{
		var query = HttpUtility.ParseQueryString(string.Empty);

		query["startdate"] = startDate.ToString(_clientOptions.Value.RequestDateFormat);
		query["enddate"] = endDate.ToString(_clientOptions.Value.RequestDateFormat);

		return $"?{query}";
	}

	private async Task<IEnumerable<FxRateApiModel>> ProcessRequestAsync(string uri)
	{
		var response = await GetAsync(uri).ConfigureAwait(false);
		var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
		var result = JsonConvert.DeserializeObject<IEnumerable<FxRateApiModel>>(content);

		return result ?? [];
	}
}