using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FxBot.Commands.Abstractions
{
	public interface ICommand
	{
		Task RunIfMatchAsync(ITelegramBotClient botClient, object message);
		Task ProcessReplyAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery);
	}
}
