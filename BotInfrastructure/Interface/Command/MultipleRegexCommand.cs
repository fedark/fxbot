using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BotInfrastructure.Interface.Command;

public abstract class MultipleRegexCommand : MessageCommand
{
	public Regex[] ValidRegexes { get; }
	public Regex InvalidRegex { get; }

	protected MultipleRegexCommand(string name, string[] validRegexArgs) : base(name)
	{
		ValidRegexes = validRegexArgs.Select(s => new Regex($"^{name} {s}$")).ToArray();
		InvalidRegex = new($"^{name}.*$");
	}

	public override Task RunIfMatchAsync(ITelegramBotClient botClient, object message)
	{
		if (message is Message { Text: not null } convertedMessage &&
		    InvalidRegex.IsMatch(convertedMessage.Text))
		{
			return RunAsync(botClient, convertedMessage);
		}

		return Task.CompletedTask;
	}
}