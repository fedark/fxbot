using Telegram.Bot;
using Telegram.Bot.Types;

namespace BotInfrastructure.Interface.Command;

public interface IRepliableCommand : ICommand
{
    Task ReplyAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery);
}