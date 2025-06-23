using BotInfrastructure.Abstract;
using BotInfrastructure.Interface;
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
	private readonly ICacheService _cache;

    public HistoryCommand(IOptions<CommandConfiguration> options,
        IOptions<HistoryCommandConfiguration> commandOptions,
	    IFxRateService fxRateService,
		ICacheService cache)
	    : base(commandOptions.Value.Pattern)
    {
	    var today = DateTime.Today;
        var dateFormat = options.Value.DateFormat;

        _hotIntervals = commandOptions.Value.HotIntervals
	        .Select(i => (today.AddMonths(-i.FromMonthsAgo).ToString(dateFormat), i.Title))
	        .ToArray();

        _commandOptions = commandOptions;
        _fxRateService = fxRateService;
        _cache = cache;
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

		var cacheKey = date.ToString();

		var file = await _cache.GetOrAddAsync(cacheKey,
			async () => await _fxRateService.GetChartAsync(date).ConfigureAwait(false))
			.ConfigureAwait(false);

		using var memoryStream = new MemoryStream(file);

		await botClient.SendPhoto(callbackQuery.Message.Chat.Id, new InputFileStream(memoryStream)).ConfigureAwait(false);
	}
}