namespace BotInfrastructure.Interface;

public interface IBot
{
	Task StartAsync(CancellationToken cancelToken);
}