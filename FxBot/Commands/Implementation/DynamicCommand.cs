﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FxBot.Commands.Abstractions;
using QuoteService;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace FxBot.Commands.Implementation
{
	public class DynamicCommand : MessageCommand
	{
		private static readonly string DynamicOptionsMessage = "Выберите период.";
		private static readonly string DateFormat = Settings.GetRequired(Settings.DateFormatKey);

		private static readonly List<(string, string)> SuggestedPeriods = new()
		{
			(DateTime.Today.AddYears(-1).ToString(DateFormat), "Год"),
			(DateTime.Today.AddMonths(-6).ToString(DateFormat), "Полгода"),
			(DateTime.Today.AddMonths(-1).ToString(DateFormat), "Месяц")
		};

		private readonly IFxRateService fxRateService_;


		public DynamicCommand(string name, IFxRateService fxRateService) : base(name)
		{
			fxRateService_ = fxRateService;
		}

		protected override async Task RunAsync(ITelegramBotClient botClient, Message message)
		{
			var keyboardMarkup = new InlineKeyboardMarkup(
				new[]
				{
					SuggestedPeriods.Select(d => InlineKeyboardButton.WithCallbackData(d.Item2, d.Item1)),
				});

			await botClient.SendTextMessageAsync(message.Chat.Id, DynamicOptionsMessage, replyMarkup: keyboardMarkup);
		}

		public override async Task ProcessReplyAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
		{
			if (callbackQuery.Message is null || !DateTime.TryParse(callbackQuery.Data, out DateTime date))
				return;

			var (stream, file) = await fxRateService_.GetChartAsync(date);

			try
			{
				await botClient.SendPhotoAsync(callbackQuery.Message.Chat.Id, new InputOnlineFile(stream, file));
			}
			catch (Exception)
			{
				throw;
			}
			finally
			{
				stream.Dispose();
			}
		}
	}
}