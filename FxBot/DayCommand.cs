using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace FxBot
{
	public class DayCommand : ICommand
	{
		private readonly IFxRateService fxRateService_;

		private readonly Regex validCommandRegex_;
		private readonly Regex invalidCommandRegex_;

		public string Name { get; }

		public DayCommand(string name, IFxRateService fxRateService)
		{
			Name = name;
			fxRateService_ = fxRateService;

			validCommandRegex_ = new($@"^{name} (\d{{1,2}})$");
			invalidCommandRegex_ = new($@"^{name}.*$");
		}

		public async Task Run(ITelegramBotClient botClient, Message message)
		{
			if (await IsWarnedIfInvalid(botClient, message))
				return;

			if (message.Text is not null && validCommandRegex_.Match(message.Text) is var match && match.Success
				&& int.TryParse(match.Groups[1].Value, out var day) && day >= 1 
				&& DateTime.Now is var now && day <= DateTime.DaysInMonth(now.Year, now.Month))
			{
				var rate = await fxRateService_.GetFxRate(new DateTime(now.Year, now.Month, day));

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
				var usage = $"Правильно, например, так:{Environment.NewLine}{Name} 9";
				await botClient.SendTextMessageAsync(message.Chat.Id, usage, replyMarkup: new ReplyKeyboardRemove());

				return true;
			}

			return false;
		}
	}
}
