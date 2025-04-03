using BotInfrastructure.Abstract;
using BotInfrastructure.Interface;
using BotInfrastructure.Model;
using Microsoft.Extensions.Options;
using QuoteService.Interface;
using System.Diagnostics.CodeAnalysis;
using BotInfrastructure.Interface.Command;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BotInfrastructure.Impl.Command;

public class ConvertCommand(IOptions<CommandConfiguration> options,
	IParser parser,
	IFxRateService fxRateService)
    : RegexCommand(@"(([+-]?\d*\.?\d+) ((\d{4}-\d{2}-\d{2})|(\d{1,2})))"), IConvertCommand
{
	protected override bool CanApply(Message message)
	{
		return base.CanApply(message) &&
		       TryParseMessage(message.Text!, out _, out _);
	}

	protected override async Task ApplyInternalAsync(ITelegramBotClient botClient, Message message)
	{
		if (!TryParseMessage(message.Text!, out var value, out var date))
			return;

		string result;

		if (date <= DateTime.Now)
		{
			var rate = await fxRateService.GetFxRateAsync(date.Value);
			var valueInForCcy = value.Value / rate;

			result = $"${valueInForCcy.ToString(options.Value.PriceFormat)}";
		}
		else
		{
			result = "I don't know future";
		}

		await botClient.SendMessage(message.Chat.Id, result).ConfigureAwait(false);
	}

	private bool TryParseMessage(string message, [NotNullWhen(true)] out double? value, [NotNullWhen(true)] out DateTime? date)
	{
		value = default;
		date = default;

		if (Regex.Match(message) is not { Success: true } match)
			return false;

        if (!double.TryParse(match.Groups[2].Value, out var parsedValue))
            return false;

        value = parsedValue;

		return parser.TryParseDateOrDay(message, match.Groups[4], match.Groups[5], out date);
	}
}