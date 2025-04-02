using System;
using System.Threading.Tasks;
using FxBot.Commands.Abstractions;
using Microsoft.Extensions.Options;
using QuoteService;
using QuoteService.Interface;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FxBot.Commands.Implementation
{
	public class ConvertCommand : MultipleRegexCommand
	{
		private readonly IFxRateService fxRateService_;

		public ConvertCommand(IOptions<CommandConfiguration> options, IFxRateService fxRateService) : base(options.Value.Names[nameof(ConvertCommand)],
			new[] { @"([+-]?\d*\.?\d+) (\d{4}-\d{2}-\d{2})", @"([+-]?\d*\.?\d+) (\d{1,2})$" })
		{
			fxRateService_ = fxRateService;
		}

		protected override async Task RunAsync(ITelegramBotClient botClient, Message message)
		{
			if (message.Text is null ||
				!(TryParseDateCommand(message.Text, out var value, out var date) || TryParseDayCommand(message.Text, out value, out date)))
			{
				await WarnInvalidAsync(botClient, message.Chat, "17.45 2022-06-01", "17.45 9");
				return;
			}

			var now = DateTime.Now;

			if (date <= now)
			{
				var rate = await fxRateService_.GetFxRateAsync(date);
				var valueInForCcy = Math.Round(value / rate, 4);

				await botClient.SendTextMessageAsync(message.Chat.Id, $"${valueInForCcy}");
			}
			else
			{
				await botClient.SendTextMessageAsync(message.Chat.Id, "Не умею предсказывать будущее.");
			}
		}

		private bool TryParseDateCommand(string message, out double value, out DateTime date)
		{
			if (ValidRegexes[0].Match(message) is var match && match.Success
				&& double.TryParse(match.Groups[1].Value, out value)
				&& DateTime.TryParse(match.Groups[2].Value, out date))
			{
				return true;
			}

			value = default;
			date = default;
			return false;
		}

		private bool TryParseDayCommand(string message, out double value, out DateTime date)
		{
			if (ValidRegexes[1].Match(message) is var match && match.Success
				&& double.TryParse(match.Groups[1].Value, out value)
				&& int.TryParse(match.Groups[2].Value, out var day) && day >= 1
				&& DateTime.Now is var now && day <= DateTime.DaysInMonth(now.Year, now.Month))
			{
				date = new DateTime(now.Year, now.Month, day);
				return true;
			}

			value = default;
			date = default;
			return false;
		}
	}
}
