using BotInfrastructure.Abstract;
using BotInfrastructure.Interface.Command;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BotInfrastructure.Impl.Command;

public class WrongCommand : CommandBase, IWrongCommand
{
	protected override bool CanApply(Message message)
	{
		return true;
	}

	protected override async Task ApplyInternalAsync(ITelegramBotClient botClient, Message message)
	{
		var result = "Wrong command format";
		await botClient.SendMessage(message.Chat.Id, result).ConfigureAwait(false);
	}
}