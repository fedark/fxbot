using Telegram.Bot;
using Telegram.Bot.Types;

namespace BotInfrastructure.Interface.Command;

public abstract class NamedCommand(string name) : ICommand
{
	protected string Name { get; } = name;

	public abstract Task RunIfMatchAsync(ITelegramBotClient botClient, object message);

	public virtual Task ProcessReplyAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
	{
		return Task.CompletedTask;
	}
}