using BotInfrastructure.Interface.Command;
using BotInfrastructure.Model;
using Microsoft.Extensions.Options;
using QuoteService.Interface;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BotInfrastructure.Impl.Command;

public class ConvertCommand(IOptions<CommandConfiguration> options, IFxRateService fxRateService)
	: MultipleRegexCommand(options.Value.Names[nameof(ConvertCommand)], [@"([+-]?\d*\.?\d+) (\d{4}-\d{2}-\d{2})", @"([+-]?\d*\.?\d+) (\d{1,2})$"])
{
	protected override async Task RunAsync(ITelegramBotClient botClient, Message message)
	{
		if (message.Text is null ||
		    !(TryParseDateCommand(message.Text, out var value, out var date) || TryParseDayCommand(message.Text, out value, out date)))
		{
			await WarnInvalidAsync(botClient, message.Chat, "17.45 2022-06-01", "17.45 9").ConfigureAwait(false);
			return;
		}

		var now = DateTime.Now;

		if (date <= now)
		{
			var rate = await fxRateService.GetFxRateAsync(date).ConfigureAwait(false);
			var valueInForCcy = Math.Round(value / rate, 4);

			await botClient.SendMessage(message.Chat.Id, $"${valueInForCcy}").ConfigureAwait(false);
		}
		else
		{
			await botClient.SendMessage(message.Chat.Id, "Не умею предсказывать будущее.").ConfigureAwait(false);
		}
	}

	private bool TryParseDateCommand(string message, out double value, out DateTime date)
	{
		if (ValidRegexes[0].Match(message) is { Success: true } match
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
		if (ValidRegexes[1].Match(message) is { Success: true } match
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