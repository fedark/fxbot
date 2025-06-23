using System.Threading;
using System.Threading.Tasks;
using BotInfrastructure.Interface;
using Microsoft.Extensions.Hosting;

namespace FxBot;

public class BotWorker(IBot bot) : BackgroundService
{
	protected override Task ExecuteAsync(CancellationToken stoppingToken)
	{
		return bot.StartAsync(stoppingToken);
	}
}