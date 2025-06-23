namespace QuoteService.Model.Configuration;

public class ResponseConfiguration
{
    public required double ResponseChunkSizeRatio { get; set; }

    public double GetValidChunkRatio()
    {
        if (ResponseChunkSizeRatio is > 0 and <= 1)
            return ResponseChunkSizeRatio;

        return 0.1;
    }
}
