using System.Diagnostics.CodeAnalysis;
using BotInfrastructure.Abstract;
using BotInfrastructure.Interface;
using BotInfrastructure.Interface.Command;
using BotInfrastructure.Model;
using Microsoft.Extensions.Options;
using QuoteService.Interface;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BotInfrastructure.Impl.Command;

public class RateCommand(IOptions<CommandConfiguration> options,
	IParser parser,
	IFxRateService fxRateService) 
	: RegexCommand(@"((\d{4}-\d{2}-\d{2})|(\d{1,2}))"), IRateCommand
{
	protected override bool CanApply(Message message)
	{
		return base.CanApply(message) &&
		       TryParseMessage(message.Text!, out _);
	}

	protected override async Task ApplyInternalAsync(ITelegramBotClient botClient, Message message)
	{
		if (!TryParseMessage(message.Text!, out var date))
			return;

		string result;

        if (date <= DateTime.Now)
        {
            var rate = await fxRateService.GetFxRateAsync(date.Value);
            result = rate.ToString(options.Value.PriceFormat);
        }
        else
        {
            result = "I don't know future";
        }

        await botClient.SendMessage(message.Chat.Id, result).ConfigureAwait(false);
	}

    private bool TryParseMessage(string message, [NotNullWhen(true)] out DateTime? date)
    {
        if (Regex.Match(message) is not { Success: true } match)
        {
	        date = default;
			return false;
        }

        return parser.TryParseDateOrDay(match.Groups[2], match.Groups[3], out date);
    }
}