using BotInfrastructure.Interface.Command;
using BotInfrastructure.Model;
using Microsoft.Extensions.Options;
using QuoteService.Interface;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotInfrastructure.Impl.Command;

public class DynamicCommand : MessageCommand
{
	private static readonly string DynamicOptionsMessage = "Выберите период.";

	private readonly List<(string, string)> _suggestedPeriods;
	private readonly IFxRateService _fxRateService;


	public DynamicCommand(IOptions<CommandConfiguration> options, IFxRateService fxRateService) : base(options.Value.Names[nameof(DynamicCommand)])
	{
		var dateFormat = options.Value.DateFormat;
		_suggestedPeriods =
		[
			(DateTime.Today.AddYears(-1).ToString(dateFormat), "Год"),
			(DateTime.Today.AddMonths(-6).ToString(dateFormat), "Полгода"),
			(DateTime.Today.AddMonths(-1).ToString(dateFormat), "Месяц")
		];

		_fxRateService = fxRateService;
	}

	protected override async Task RunAsync(ITelegramBotClient botClient, Message message)
	{
		var keyboardMarkup = new InlineKeyboardMarkup(
		[
			_suggestedPeriods.Select(d => InlineKeyboardButton.WithCallbackData(d.Item2, d.Item1))
		]);

		await botClient.SendMessage(message.Chat.Id, DynamicOptionsMessage, replyMarkup: keyboardMarkup);
	}

	public override async Task ProcessReplyAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
	{
		if (callbackQuery.Message is null || !DateTime.TryParse(callbackQuery.Data, out var date))
		{
			return;
		}

		await using var fileStream = await _fxRateService.GetChartAsync(date).ConfigureAwait(false);

		await botClient.SendPhoto(callbackQuery.Message.Chat.Id, new InputFileStream(fileStream));
	}
}