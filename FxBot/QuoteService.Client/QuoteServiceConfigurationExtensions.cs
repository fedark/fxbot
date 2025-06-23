using Microsoft.Extensions.DependencyInjection;

namespace QuoteService.Client;

public static class QuoteServiceConfigurationExtensions
{
    public static IServiceCollection AddQuoteService(this IServiceCollection services, Action<QuoteServiceConnectionConfiguration> configureOptions)
    {
        services.AddOptions();
        services.Configure(configureOptions);
        services.AddSingleton<IFxRateService, FxRateServiceClient>();

        return services;
    }
}
