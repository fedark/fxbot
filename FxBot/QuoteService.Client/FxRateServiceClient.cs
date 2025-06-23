using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using Microsoft.Extensions.Options;
using QuoteService.Grpc.Client;

namespace QuoteService.Client;

public class FxRateServiceClient : FxRateService.FxRateServiceClient, IFxRateService
{
    private readonly FxRateService.FxRateServiceClient _client;

    public FxRateServiceClient(IOptions<QuoteServiceConnectionConfiguration> options)
    {
        var target = options.Value.GrpcTarget;
        var channel = GrpcChannel.ForAddress(target);
        _client = new(channel);
    }

    public async Task<double> GetFxRateAsync(DateTime date)
    {
        var request = new FxRateRequest
        {
            Date = Timestamp.FromDateTime(date)
        };

        var response = await _client.GetFxRateAsync(request).ConfigureAwait(false);

        return response.FxRate;
    }

    public async Task<byte[]> GetChartAsync(DateTime startDate)
    {
        var request = new ChartRequest
        {
            StartDate = Timestamp.FromDateTime(startDate)
        };

        var responseStream = _client.GetChart(request).ResponseStream;
        var cancellationTokenSource = new CancellationTokenSource();

        byte[] result = null!;
        int position = 0;

        while (await responseStream.MoveNext(cancellationTokenSource.Token).ConfigureAwait(false))
        {
            var chunk = responseStream.Current;

            result ??= new byte[chunk.FileSize];

            chunk.Chunk.CopyTo(result, position);
            position += chunk.Size;
        }

        return result;
    }
}
