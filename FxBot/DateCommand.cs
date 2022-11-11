using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace FxBot
{
	public class DateCommand : ICommand
	{
		private readonly IFxRateService fxRateService_;

		private readonly Regex validCommandRegex_;
		private readonly Regex invalidCommandRegex_;

		public string Name { get; }

		public DateCommand(string name, IFxRateService fxRateService)
		{
			Name = name;
			fxRateService_ = fxRateService;

			validCommandRegex_ = new($@"^{name} (\d{{4}}-\d{{2}}-\d{{2}})$");
			invalidCommandRegex_ = new($@"^{name}.*$");
		}

		public async Task Run(ITelegramBotClient botClient, Message message)
		{
			if (await IsWarnedIfInvalid(botClient, message))
				return;

			if (message.Text is not null && validCommandRegex_.Match(message.Text) is var match && match.Success
				&& DateTime.TryParse(match.Groups[1].Value, out var date))
			{
				var rate = await fxRateService_.GetFxRate(date);

				await botClient.SendTextMessageAsync(message.Chat.Id, rate.ToString(), replyMarkup: new ReplyKeyboardRemove());
			}
		}

		public bool IsMatch(string message)
		{
			return invalidCommandRegex_.IsMatch(message);
		}

		private async Task<bool> IsWarnedIfInvalid(ITelegramBotClient botClient, Message message)
		{
			if (message.Text is null || !validCommandRegex_.IsMatch(message.Text))
			{
				var usage = $"Правильно, например, так:{Environment.NewLine}{Name} 2022-06-01";
				await botClient.SendTextMessageAsync(message.Chat.Id, usage, replyMarkup: new ReplyKeyboardRemove());

				return true;
			}

			return false;
		}
	}
}
