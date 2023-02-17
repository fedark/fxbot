using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace FxBot
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			try
			{
				var serviceProvider = Configure();
				var bot = serviceProvider.GetRequiredService<Bot>();

				using var cancelSource = new CancellationTokenSource();

				await bot.StartAsync(cancelSource.Token);

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
			var setup = new Setup();
			var services = new ServiceCollection();
			setup.ConfigureServices(services);
			return services.BuildServiceProvider();
		}
	}
}
