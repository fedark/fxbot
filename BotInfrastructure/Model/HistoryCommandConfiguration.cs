namespace BotInfrastructure.Model;

public class HistoryCommandConfiguration
{
    public required string Pattern { get; set; }
    public required string Prompt { get; set; }
    public required (int FromMonthsAgo, string Title)[] HotIntervals { get; set; }
}