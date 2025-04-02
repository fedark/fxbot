using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FxBot.Commands.Abstractions;
using Microsoft.Extensions.Options;
using QuoteService.Interface;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace FxBot.Commands.Implementation
{
	public class DynamicCommand : MessageCommand
	{
		private static readonly string DynamicOptionsMessage = "Выберите период.";

		private readonly List<(string, string)> suggestedPeriods_;
		private readonly IFxRateService fxRateService_;


		public DynamicCommand(IOptions<CommandConfiguration> options, IFxRateService fxRateService) : base(options.Value.Names[nameof(DynamicCommand)])
		{
			var dateFormat = options.Value.DateFormat;
			suggestedPeriods_ = new()
			{
				(DateTime.Today.AddYears(-1).ToString(dateFormat), "Год"),
				(DateTime.Today.AddMonths(-6).ToString(dateFormat), "Полгода"),
				(DateTime.Today.AddMonths(-1).ToString(dateFormat), "Месяц")
			};

			fxRateService_ = fxRateService;
		}

		protected override async Task RunAsync(ITelegramBotClient botClient, Message message)
		{
			var keyboardMarkup = new InlineKeyboardMarkup(
				new[]
				{
					suggestedPeriods_.Select(d => InlineKeyboardButton.WithCallbackData(d.Item2, d.Item1)),
				});

			await botClient.SendTextMessageAsync(message.Chat.Id, DynamicOptionsMessage, replyMarkup: keyboardMarkup);
		}

		public override async Task ProcessReplyAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
		{
			if (callbackQuery.Message is null || !DateTime.TryParse(callbackQuery.Data, out var date))
			{
				return;
			}

			var fileStream = await fxRateService_.GetChartAsync(date);

			try
			{
				await botClient.SendPhotoAsync(callbackQuery.Message.Chat.Id, new InputFileStream(fileStream));
			}
			catch (Exception)
			{
				throw;
			}
			finally
			{
				await fileStream.DisposeAsync();
			}
		}
	}
}
