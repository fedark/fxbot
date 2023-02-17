using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace FxBot.Commands.Abstractions
{
	public abstract class MultipleRegexCommand : MessageCommand
	{
		public Regex[] ValidRegexes { get; }
		public Regex InvalidRegex { get; }

		protected MultipleRegexCommand(string name, string[] validRegexArgs) : base(name)
		{
			ValidRegexes = validRegexArgs.Select(s => new Regex($@"^{name} {s}$")).ToArray();
			InvalidRegex = new($@"^{name}.*$");
		}

		public override Task RunIfMatchAsync(ITelegramBotClient botClient, object message)
		{
			if (message is Message convertedMessage && convertedMessage.Text is not null
				&& InvalidRegex.IsMatch(convertedMessage.Text))
			{
				return RunAsync(botClient, convertedMessage);
			}

			return Task.CompletedTask;
		}
	}
}
