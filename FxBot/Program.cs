using System;
using System.Threading;
using System.Threading.Tasks;
using FxBot.Commands.Abstractions;
using FxBot.Commands.Implementation;
using Microsoft.Extensions.DependencyInjection;
using QuoteService;

namespace FxBot
{
	public class Program
    {
        public static async Task Main(string[] args)
        {
			var serviceProvider = Configure();

			try
			{
				var bot = serviceProvider.GetRequiredService<Bot>();

				using var cancelSource = new CancellationTokenSource();

				await bot.Start(cancelSource.Token);

				Console.ReadLine();
				cancelSource.Cancel();
			}
			catch (Exception e)
			{
				Console.WriteLine($"Well, shit: {e.Message}.");
			}
        }

		public static IServiceProvider Configure()
		{
			var services = new ServiceCollection();
			services.AddSingleton<IFxRateService, FxRateService>();
			services.AddSingleton<ICommand, DynamicCommand>(provider => new(Settings.GetRequired(Settings.CommandDynamicKey), provider.GetRequiredService<IFxRateService>()));
			services.AddSingleton<ICommand, RateCommand>(provider => new(Settings.GetRequired(Settings.CommandRateKey), provider.GetRequiredService<IFxRateService>()));
			services.AddSingleton<ICommand, ConvertCommand>(provider => new(Settings.GetRequired(Settings.CommandConvertKey), provider.GetRequiredService<IFxRateService>()));
			services.AddSingleton<Bot>(provider => new(Settings.GetRequired(Settings.BotTokenKey), provider.GetServices<ICommand>()));

			return services.BuildServiceProvider();
		}
	}
}
