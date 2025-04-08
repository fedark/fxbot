namespace BotInfrastructure.Model;

public class HistoryCommandConfiguration
{
    public required string Pattern { get; set; }
    public required string Prompt { get; set; }
    public required HistoryCommandHotIntervalConfiguration[] HotIntervals { get; set; }
}

public class HistoryCommandHotIntervalConfiguration
{
	public required int FromMonthsAgo { get; set; }
	public required string Title { get; set; }
}