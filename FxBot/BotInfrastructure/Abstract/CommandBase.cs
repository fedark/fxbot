using BotInfrastructure.Interface.Command;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BotInfrastructure.Abstract;

public abstract class CommandBase : ICommand
{
	#region Public Methods

	public ICommand? Next { get; set; }

	public virtual async Task ApplyAsync(ITelegramBotClient botClient, Message message)
	{
		if (CanApply(message))
		{
			await ApplyInternalAsync(botClient, message).ConfigureAwait(false);
		}
		else if (Next is not null)
		{
			await Next.ApplyAsync(botClient, message).ConfigureAwait(false);
		}
	}

	#endregion

	#region Non-Public Methods

	protected abstract bool CanApply(Message message);
	protected abstract Task ApplyInternalAsync(ITelegramBotClient botClient, Message message);

	#endregion
}