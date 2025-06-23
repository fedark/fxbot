using System;
using BotInfrastructure.Impl;
using BotInfrastructure.Impl.Command;
using BotInfrastructure.Interface;
using BotInfrastructure.Interface.Command;
using BotInfrastructure.Model;
using FxBot;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using QuoteService.Client;

var appBuilder = Host.CreateDefaultBuilder(args);

appBuilder.ConfigureServices((context, services) =>
{
	MapConfiguration(context.Configuration, services);
	ConfigureCaching(context.Configuration, services);
	ConfigureServices(context.Configuration, services);

	services.AddHostedService<BotWorker>();
});

await appBuilder.Build().RunAsync();

return;

static void MapConfiguration(IConfiguration configuration, IServiceCollection services)
{
	services.AddOptions();

	services.Configure<BotConfiguration>(c =>
	{
		c.Token = configuration.GetRequired("Bot:Token");
	});

	services.Configure<CommandConfiguration>(c =>
	{
		c.PriceFormat = configuration.GetRequired("Bot:PriceFormat");
		c.DateFormat = configuration.GetRequired("Bot:DateFormat");
	});

	services.Configure<HistoryCommandConfiguration>(configuration.GetRequiredSection("Bot:Commands:History"));
}

static void ConfigureCaching(IConfiguration configuration, IServiceCollection services)
{
    services.AddStackExchangeRedisCache(options =>
    {
		options.Configuration = configuration.GetRequired("Redis:Configuration");
		options.InstanceName = configuration.GetRequired("Redis:InstanceName");
    });

	services.AddSingleton<ICacheService, CacheService>();
}

static void ConfigureServices(IConfiguration configuration, IServiceCollection services)
{
	services.AddQuoteService(options =>
	{
		var target = configuration.GetRequired("QuoteService:Target");
		options.GrpcTarget = target;
	});

	services.AddSingleton<IRateCommand, RateCommand>();
	services.AddSingleton<IConvertCommand, ConvertCommand>();
	services.AddSingleton<IHistoryCommand, HistoryCommand>();
	services.AddSingleton<IWrongCommand, WrongCommand>();

	services.AddTransient<ICommandChainBuilder, CommandChainBuilder>();
	services.AddSingleton<ICommandChainFactory, CommandChainFactory>();

	services.AddSingleton<IParser, Parser>();
	services.AddSingleton<IBot, Bot>(provider =>
	{
		var options = provider.GetRequiredService<IOptions<BotConfiguration>>();
		var factory = provider.GetRequiredService<ICommandChainFactory>();

		var chain = factory.GetDefaultChain() ?? throw new Exception("No commands are specified");
		var replyChain = factory.GetReplyChain() ?? throw new Exception("No commands are specified");

		return new Bot(options, chain, replyChain);
	});
}
