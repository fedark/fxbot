using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FxBot.Commands.Abstractions
{
	public abstract class NamedCommand : ICommand
	{
		protected string Name { get; }

		protected NamedCommand(string name)
		{
			Name = name;
		}

		public abstract Task RunIfMatchAsync(ITelegramBotClient botClient, object message);

		public virtual Task ProcessReplyAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
		{
			return Task.CompletedTask;
		}
	}
}
