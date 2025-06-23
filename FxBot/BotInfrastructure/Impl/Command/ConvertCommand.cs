using BotInfrastructure.Abstract;
using BotInfrastructure.Interface;
using BotInfrastructure.Model;
using Microsoft.Extensions.Options;
using QuoteService.Interface;
using System.Text;
using System.Text.RegularExpressions;
using BotInfrastructure.Interface.Command;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BotInfrastructure.Impl.Command;

public class ConvertCommand(IOptions<CommandConfiguration> options,
	IParser parser,
	IFxRateService fxRateService)
    : RegexCommand(@"(([+-]?\d*\.?\d+) ((\d{4}-\d{2}-\d{2})|(\d{1,2})))", RegexOptions.Multiline), IConvertCommand
{
	protected override bool CanApply(Message message)
	{
		return base.CanApply(message) &&
		       TryParseMessage(message.Text!, out _);
	}

	protected override async Task ApplyInternalAsync(ITelegramBotClient botClient, Message message)
	{
		if (!TryParseMessage(message.Text!, out var args))
			return;

		var result = new StringBuilder();

		foreach (var (value, date) in args)
		{
			if (date <= DateTime.Now)
			{
				var rate = await fxRateService.GetFxRateAsync(date);
				var valueInForCcy = value / rate;

				result.AppendLine($"${valueInForCcy.ToString(options.Value.PriceFormat)}");
			}
			else
			{
				result.AppendLine("I don't know future");
			}
		}

		await botClient.SendMessage(message.Chat.Id, result.ToString()).ConfigureAwait(false);
	}

	private bool TryParseMessage(string message, out (double, DateTime)[] args)
	{
		args = [];

		var lines = message.Split("\n");
		if (!lines.Any())
			return false;

		var result = new List<(double, DateTime)>();

		foreach (var line in lines)
		{
			if (Regex.Match(line) is not { Success: true } match)
				return false;

			if (!double.TryParse(match.Groups[2].Value, out var value) ||
			    !parser.TryParseDateOrDay(match.Groups[4], match.Groups[5], out var date))
				return false;

			result.Add((value, date.Value));
		}

		args = result.ToArray();
		return true;
	}
}