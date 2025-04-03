using BotInfrastructure.Abstract;
using BotInfrastructure.Interface.Command;
using BotInfrastructure.Model;
using Microsoft.Extensions.Options;
using QuoteService.Interface;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotInfrastructure.Impl.Command;

public class HistoryCommand : RegexCommand, IRepliableCommand, IHistoryCommand
{
    private readonly (string FromDate, string Title)[] _hotIntervals;
    private readonly IOptions<HistoryCommandConfiguration> _commandOptions;
    private readonly IFxRateService _fxRateService;

    public HistoryCommand(IOptions<CommandConfiguration> options,
        IOptions<HistoryCommandConfiguration> commandOptions,
	    IFxRateService fxRateService)
	    : base(commandOptions.Value.Pattern)
    {
	    var today = DateTime.Today;
        var dateFormat = options.Value.DateFormat;

        _hotIntervals = commandOptions.Value.HotIntervals
	        .Select(i => (today.AddMonths(i.FromMonthsAgo).ToString(dateFormat), i.Title))
	        .ToArray();

        _commandOptions = commandOptions;
        _fxRateService = fxRateService;
    }
	
    protected override Task ApplyInternalAsync(ITelegramBotClient botClient, Message message)
    {
	    var keyboardMarkup = new InlineKeyboardMarkup(
	    [
		    _hotIntervals.Select(i => InlineKeyboardButton.WithCallbackData(i.Title, i.FromDate))
	    ]);

		return botClient.SendMessage(message.Chat.Id, _commandOptions.Value.Prompt, replyMarkup: keyboardMarkup);
	}

    public async Task ReplyAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
	    if (callbackQuery.Message is null ||
	        !DateTime.TryParse(callbackQuery.Data, out var date))
		    return;

		await using var fileStream = await _fxRateService.GetChartAsync(date).ConfigureAwait(false);

		await botClient.SendPhoto(callbackQuery.Message.Chat.Id, new InputFileStream(fileStream)).ConfigureAwait(false);
	}
}