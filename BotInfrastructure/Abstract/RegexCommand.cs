using System.Text.RegularExpressions;
using Telegram.Bot.Types;

namespace BotInfrastructure.Abstract;

public abstract class RegexCommand(string pattern, RegexOptions? regexOptions = default) : CommandBase
{
	protected Regex Regex { get; } = regexOptions is not null 
		? new($"^{pattern}$", regexOptions.Value) 
		: new($"^{pattern}$");

	protected override bool CanApply(Message message)
	{
		return message.Text is not null && Regex.IsMatch(message.Text);
	}
}