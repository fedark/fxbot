namespace QuoteService.Client;

public interface IFxRateService
{
    Task<double> GetFxRateAsync(DateTime date);
    Task<byte[]> GetChartAsync(DateTime startDate);
}
