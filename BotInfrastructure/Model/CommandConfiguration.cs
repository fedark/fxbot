namespace BotInfrastructure.Model;

public class CommandConfiguration
{
	public required Dictionary<string, string> Names { get; set; } = new();
	public required string DateFormat { get; set; }
}