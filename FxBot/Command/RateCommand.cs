using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using QuoteService;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace FxBot
{
	public class RateCommand : ICommand
	{
		private readonly IFxRateService fxRateService_;

		private readonly Regex validCommandDateRegex_;
		private readonly Regex validCommandDayRegex_;
		private readonly Regex invalidCommandRegex_;

		public string Name { get; }

		public RateCommand(string name, IFxRateService fxRateService)
		{
			Name = name;
			fxRateService_ = fxRateService;

			validCommandDateRegex_ = new($@"^{name} (\d{{4}}-\d{{2}}-\d{{2}})$");
			validCommandDayRegex_ = new($@"^{name} (\d{{1,2}})$");
			invalidCommandRegex_ = new($@"^{name}.*$");
		}

		public bool IsMatch(string message)
		{
			return invalidCommandRegex_.IsMatch(message);
		}

		public async Task RunAsync(ITelegramBotClient botClient, Message message)
		{
			if (await TryWarnIfInvalidAsync(botClient, message) || message.Text is null)
				return;

			var date = TryParseDateCommand(message.Text) ?? TryParseDayCommand(message.Text);
			if (date is not null)
			{
				var now = DateTime.Now;

				if (date.Value <= now)
				{
					var rate = await fxRateService_.GetFxRateAsync(date.Value);
					await botClient.SendTextMessageAsync(message.Chat.Id, rate.ToString(), replyMarkup: new ReplyKeyboardRemove());
				}
				else
				{
					var futureDateReply = (date.Value - now).TotalDays <= 30 ? "Губа не дура." : "А ты не охуел с такими запросами?";
					await botClient.SendTextMessageAsync(message.Chat.Id, futureDateReply, replyMarkup: new ReplyKeyboardRemove());
				}
			}
		}

		private DateTime? TryParseDateCommand(string message)
		{
			if (validCommandDateRegex_.Match(message) is var match && match.Success
				&& DateTime.TryParse(match.Groups[1].Value, out var date))
			{
				return date;
			}

			return null;
		}

		private DateTime? TryParseDayCommand(string message)
		{
			if (validCommandDayRegex_.Match(message) is var match && match.Success
				&& int.TryParse(match.Groups[1].Value, out var day) && day >= 1
				&& DateTime.Now is var now && day <= DateTime.DaysInMonth(now.Year, now.Month))
			{
				return new DateTime(now.Year, now.Month, day);
			}

			return null;
		}

		private async Task<bool> TryWarnIfInvalidAsync(ITelegramBotClient botClient, Message message)
		{
			if (message.Text is null || !IsMatchValidRegex(message.Text))
			{
				var usage = new StringBuilder();
				usage.AppendLine("Правильно, например, так:");
				usage.AppendLine($"{Name} 2022-06-01");
				usage.AppendLine();
				usage.AppendLine("или так:");
				usage.Append($"{Name} 9");

				await botClient.SendTextMessageAsync(message.Chat.Id, usage.ToString(), replyMarkup: new ReplyKeyboardRemove());

				return true;
			}

			return false;
		}

		private bool IsMatchValidRegex(string message)
		{
			return validCommandDateRegex_.IsMatch(message) || validCommandDayRegex_.IsMatch(message);
		}
	}
}
