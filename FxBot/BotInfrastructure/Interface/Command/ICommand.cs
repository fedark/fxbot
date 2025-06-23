using Telegram.Bot;
using Telegram.Bot.Types;

namespace BotInfrastructure.Interface.Command;

public interface ICommand
{
    ICommand? Next { get; set; }
    Task ApplyAsync(ITelegramBotClient botClient, Message message);
}