using BotInfrastructure.Interface;
using Microsoft.Extensions.Caching.Distributed;

namespace BotInfrastructure.Impl;

public class CacheService(IDistributedCache cache) : ICacheService
{
    public async Task<byte[]> GetOrAddAsync(string key, Func<Task<byte[]>> factory)
    {
        var file = await cache.GetAsync(key).ConfigureAwait(false);

        if (file is null)
        {
            file = await factory().ConfigureAwait(false);

            cache.Set(key, file);
        }

        return file;
    }
}
