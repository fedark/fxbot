using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuoteService.Impl;
using QuoteService.Interface;
using QuoteService.Model.Configuration;


var builder = WebApplication.CreateSlimBuilder(args);

MapConfiguration(builder.Configuration, builder.Services);
ConfigureServices(builder.Services);

builder.Services.AddGrpc();

var app = builder.Build();

app.MapGrpcService<FxRateServiceImpl>();

app.Run();


static void MapConfiguration(IConfiguration configuration, IServiceCollection services)
{
    services.AddOptions();

    services.Configure<FxRateApiConfiguration>(configuration.GetRequiredSection("FxRateApi"));
    services.Configure<ScriptConfiguration>(configuration.GetRequiredSection("Script"));
    services.Configure<ResponseConfiguration>(configuration.GetRequiredSection("Response"));
}

static void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<IFxRateApiClient, FxRateApiClient>();
}