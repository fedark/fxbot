namespace QuoteService.Model;

public class FxRateApiConfiguration
{
    public required string BaseUrl { get; set; }
    public required string PathBeforeDenomination { get; set; }
    public required string PathAfterDenomination { get; set; }
    public required string RequestDateFormat { get; set; }

}