using BotInfrastructure.Interface;
using BotInfrastructure.Interface.Command;
using BotInfrastructure.Model;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotInfrastructure.Impl
{
    public class Bot(IOptions<BotConfiguration> options, ICommand chain, IRepliableCommand replyChain) : IBot
    {
		#region Private Fields

		private readonly ITelegramBotClient _botClient = new TelegramBotClient(options.Value.Token);

		#endregion

		#region Public Methods

		public async Task StartAsync(CancellationToken cancelToken)
		{
			// allow all updates
			var receiverOptions = new ReceiverOptions { AllowedUpdates = { } };
			
			_botClient.StartReceiving(HandleUpdateAsync, HandleError, receiverOptions, cancelToken);
			var bot = await _botClient.GetMe(cancelToken).ConfigureAwait(false);

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
				await handler.ConfigureAwait(false);
			}
			catch (Exception e)
			{
				await HandleError(botClient, e, cancelToken).ConfigureAwait(false);
			}
		}

		private Task OnMessageReceivedAsync(ITelegramBotClient botClient, Message? message)
		{
			if (message is not null)
			{
				return chain.ApplyAsync(botClient, message);
			}

			return Task.CompletedTask;
		}

		private Task OnCallbackQueryReceivedAsync(ITelegramBotClient botClient, CallbackQuery? callbackQuery)
		{
			if (callbackQuery is not null)
			{
				return replyChain.ReplyAsync(botClient, callbackQuery);
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
