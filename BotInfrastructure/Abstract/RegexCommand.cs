using System.Text.RegularExpressions;
using Telegram.Bot.Types;

namespace BotInfrastructure.Abstract;

public abstract class RegexCommand(string pattern) : CommandBase
{
	protected Regex Regex { get; } = new($"^{pattern}$");

	protected override bool CanApply(Message message)
	{
		return message.Text is not null && Regex.IsMatch(message.Text);
	}
}