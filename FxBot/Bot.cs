using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
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

		private readonly DynamicCommand dynamicCommand_;
		private readonly DateCommand dateCommand_;
		private readonly DayCommand dayCommand_;
		private readonly List<ICommand> commands_;

		#endregion

		#region Public Methods

		public Bot(string token, IFxRateService fxRateService)
		{
			botClient_ = new TelegramBotClient(token);

			dynamicCommand_ = new(getCommandName("CommandDynamic"), fxRateService);
			dateCommand_ = new(getCommandName("CommandDate"), fxRateService);
			dayCommand_ = new(getCommandName("CommandDay"), fxRateService);
			commands_ = new() { dynamicCommand_, dateCommand_, dayCommand_ };

			static string getCommandName(string setting)
			{
				var commandName = ConfigurationManager.AppSettings[setting];
				if (commandName is null)
				{
					throw new Exception($"Command name under '{setting}' setting is not found");
				}

				return commandName;
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

		private async Task OnMessageReceivedAsync(ITelegramBotClient botClient, Message? message)
		{
			if (message is null || message.Text is null)
				return;

			foreach (var command in commands_)
			{
				if (command.IsMatch(message.Text))
				{
					await command.Run(botClient, message);
				}
			}
		}

		private async Task OnCallbackQueryReceivedAsync(ITelegramBotClient botClient, CallbackQuery? callbackQuery)
		{
			if (callbackQuery is null)
				return;

			await dynamicCommand_.ProcessReply(botClient, callbackQuery);
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
