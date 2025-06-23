namespace BotInfrastructure.Interface;

public interface ICacheService
{
    Task<byte[]> GetOrAddAsync(string key, Func<Task<byte[]>> factory);
}
