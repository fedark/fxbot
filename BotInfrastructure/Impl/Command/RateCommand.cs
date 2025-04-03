using BotInfrastructure.Abstract;
using BotInfrastructure.Model;
using Microsoft.Extensions.Options;
using QuoteService.Interface;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BotInfrastructure.Impl.Command;

public class RateCommand(IOptions<CommandConfiguration> options, IFxRateService fxRateService)
    : MultipleRegexCommand(options.Value.Names[nameof(RateCommand)], [@"(\d{4}-\d{2}-\d{2})", @"(\d{1,2})"])
{
    protected override async Task RunAsync(ITelegramBotClient botClient, Message message)
    {
        if (message.Text is null ||
            !(TryParseDateCommand(message.Text, out var date) || TryParseDayCommand(message.Text, out date)))
        {
            await WarnInvalidAsync(botClient, message.Chat, "2022-06-01", "9");
            return;
        }

        var now = DateTime.Now;

        if (date <= now)
        {
            var rate = await fxRateService.GetFxRateAsync(date);
            await botClient.SendMessage(message.Chat.Id, rate.ToString());
        }
        else
        {
            await botClient.SendMessage(message.Chat.Id, "Не умею предсказывать будущее.");
        }
    }

    private bool TryParseDateCommand(string message, out DateTime date)
    {
        if (ValidRegexes[0].Match(message) is { Success: true } match
            && DateTime.TryParse(match.Groups[1].Value, out date))
        {
            return true;
        }

        date = default;
        return false;
    }

    private bool TryParseDayCommand(string message, out DateTime date)
    {
        if (ValidRegexes[1].Match(message) is { Success: true } match
            && int.TryParse(match.Groups[1].Value, out var day) && day >= 1
            && DateTime.Now is var now && day <= DateTime.DaysInMonth(now.Year, now.Month))
        {
            date = new DateTime(now.Year, now.Month, day);
            return true;
        }

        date = default;
        return false;
    }
}