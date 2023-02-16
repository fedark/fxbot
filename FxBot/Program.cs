using System;
using System.Threading;
using System.Threading.Tasks;
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
			services.AddSingleton<Bot>(provider => new(Settings.GetRequired(Settings.BotTokenKey), provider.GetRequiredService<IFxRateService>()));

			return services.BuildServiceProvider();
		}
	}
}
