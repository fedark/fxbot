using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace FxBot
{
	public class Program
    {
        public static async Task Main(string[] args)
        {
            var botToken = ConfigurationManager.AppSettings["BotToken"];
            if (botToken is null)
			{
				Console.WriteLine($"Well, shit: Bot token is not found.");
                return;
			}

			try
			{
				var bot = new Bot(botToken, new FxRateService());

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
    }
}
