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
using QuoteService.Impl;
using QuoteService.Interface;
using QuoteService.Model;

var appBuilder = Host.CreateDefaultBuilder(args);

appBuilder.ConfigureServices((context, services) =>
{
	MapConfiguration(context.Configuration, services);
	ConfigureServices(services);

	services.AddHostedService<BotWorker>();
});

await appBuilder.Build().RunAsync();

return;

static void MapConfiguration(IConfiguration configuration, IServiceCollection services)
{
	services.AddOptions();

	services.Configure<BotConfiguration>(c =>
	{
		var botSection = configuration.GetRequiredSection("Bot");
		c.Token = botSection.GetRequired("Token");
	});

	services.Configure<CommandConfiguration>(c =>
	{
		var botSection = configuration.GetRequiredSection("Bot");

		c.PriceFormat = botSection.GetRequired("PriceFormat");
		c.DateFormat = botSection.GetRequired("DateFormat");
	});

	services.Configure<HistoryCommandConfiguration>(configuration.GetRequiredSection("Bot:Commands:History"));

	services.Configure<FxRateApiConfiguration>(configuration.GetRequiredSection("QuoteService:FxRateApi"));
	services.Configure<ScriptConfiguration>(configuration.GetRequiredSection("QuoteService:Script"));
}

static void ConfigureServices(IServiceCollection services)
{
	services.AddSingleton<IFxRateClient, FxRateClient>();
	services.AddSingleton<IFxRateService, FxRateService>();

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
