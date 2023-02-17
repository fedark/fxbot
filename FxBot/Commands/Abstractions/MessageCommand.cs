using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace FxBot.Commands.Abstractions
{
	public abstract class MessageCommand : NamedCommand
	{
		protected MessageCommand(string name) : base(name)
		{
		}

		public override Task RunIfMatchAsync(ITelegramBotClient botClient, object message)
		{
			if (message is Message convertedMessage && convertedMessage.Text is not null
				&& convertedMessage.Text == Name)
			{
				return RunAsync(botClient, convertedMessage);
			}

			return Task.CompletedTask;
		}

		protected abstract Task RunAsync(ITelegramBotClient botClient, Message message);

		protected async Task WarnInvalidAsync(ITelegramBotClient botClient, Chat chat, params string[] exampleArguments)
		{
			if (exampleArguments.Length < 1)
			{
				return;
			}

			var usage = new StringBuilder();
			usage.AppendLine("Правильно, например, так:");
			usage.AppendLine($"{Name} {exampleArguments[0]}");

			for (int i = 1; i < exampleArguments.Length; i++)
			{
				usage.AppendLine();
				usage.AppendLine("или так:");
				usage.AppendLine($"{Name} {exampleArguments[i]}");
			}

			await botClient.SendTextMessageAsync(chat.Id, usage.ToString().Trim());
		}
	}
}
