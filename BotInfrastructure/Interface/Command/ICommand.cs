using Telegram.Bot;
using Telegram.Bot.Types;

namespace BotInfrastructure.Interface.Command;

public interface ICommand
{
	Task RunIfMatchAsync(ITelegramBotClient botClient, object message);
	Task ProcessReplyAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery);
}