using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FxBot.Commands.Abstractions;
using QuoteService;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace FxBot
{
	public class Bot
	{
		#region Private Fields

		private readonly ITelegramBotClient botClient_;

		private delegate Task MessageEventHandler(ITelegramBotClient botClient, Message message);
		private delegate Task CallbackQueryEventHandler(ITelegramBotClient botClient, CallbackQuery callbackQuery);

		private event MessageEventHandler? MessageEvent;
		private event CallbackQueryEventHandler? CallbackQueryEvent;

		#endregion

		#region Public Methods

		public Bot(string token, IEnumerable<ICommand> commands)
		{
			botClient_ = new TelegramBotClient(token);

			foreach (var command in commands)
			{
				MessageEvent += command.RunIfMatchAsync;
				CallbackQueryEvent += command.ProcessReplyAsync;
			}
		}

		public async Task Start(CancellationToken cancelToken)
		{
			// allow all updates
			var receiverOptions = new ReceiverOptions { AllowedUpdates = { } };

			botClient_.StartReceiving(HandleUpdateAsync, HandleError, receiverOptions, cancelToken);
			var bot = await botClient_.GetMeAsync(cancelToken);

			Console.WriteLine($"Start listening for @{bot.Username}");
		}

		#endregion

		#region Handlers

		private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancelToken)
		{
			var handler = update.Type switch
			{
				UpdateType.Message => OnMessageReceivedAsync(botClient, update.Message),
				UpdateType.CallbackQuery => OnCallbackQueryReceivedAsync(botClient, update.CallbackQuery),
				_ => UnknownUpdateAsync()
			};

			try
			{
				await handler;
			}
			catch (Exception e)
			{
				await HandleError(botClient, e, cancelToken);
			}
		}

		private Task OnMessageReceivedAsync(ITelegramBotClient botClient, Message? message)
		{
			if (message is not null && MessageEvent is not null)
			{
				return MessageEvent(botClient, message);
			}

			return Task.CompletedTask;
		}

		private Task OnCallbackQueryReceivedAsync(ITelegramBotClient botClient, CallbackQuery? callbackQuery)
		{
			if (callbackQuery is not null && CallbackQueryEvent is not null)
			{
				CallbackQueryEvent(botClient, callbackQuery);
			}

			return Task.CompletedTask;
		}

		private static Task UnknownUpdateAsync()
		{
			return Task.CompletedTask;
		}

		private static Task HandleError(ITelegramBotClient botClient, Exception exception, CancellationToken cancelToken)
		{
			Console.WriteLine(exception switch
			{
				ApiRequestException apiRequestException => $"Telegram API Error: [{apiRequestException.ErrorCode}]:{apiRequestException.Message}",
				_ => exception.ToString()
			});

			return Task.CompletedTask;
		}

		#endregion
	}
}
