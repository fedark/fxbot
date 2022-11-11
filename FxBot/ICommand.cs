using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FxBot
{
	public interface ICommand
	{
		string Name { get; }

		bool IsMatch(string message);
		Task Run(ITelegramBotClient botClient, Message message);
	}
}