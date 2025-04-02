using System.Threading;
using System.Threading.Tasks;
using BotInfrastructure.Interface;
using Microsoft.Extensions.Hosting;

namespace FxBot;

public class BotWorker(IBot bot) : IHostedService
{
	public Task StartAsync(CancellationToken cancellationToken)
	{
		return bot.StartAsync(cancellationToken);
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		return Task.CompletedTask;
	}
}