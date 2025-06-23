using BotInfrastructure.Interface;
using Microsoft.Extensions.Caching.Distributed;

namespace BotInfrastructure.Impl;

public class CacheService(IDistributedCache cache) : ICacheService
{
    public async Task<byte[]> GetOrAddAsync(string key, Func<Task<Stream>> factory)
    {
        var file = await cache.GetAsync(key).ConfigureAwait(false);

        if (file is null)
        {
            await using var stream = await factory().ConfigureAwait(false);

            file = new byte[stream.Length];
            await stream.ReadAsync(file).ConfigureAwait(false);

            cache.Set(key, file);
        }

        return file;
    }
}
