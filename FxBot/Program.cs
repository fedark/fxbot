using BotInfrastructure.Impl;
using BotInfrastructure.Impl.Command;
using BotInfrastructure.Interface;
using BotInfrastructure.Model;
using FxBot;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
		var commandsSection = botSection.GetRequiredSection("Commands");

		c.Names[nameof(DynamicCommand)] = commandsSection.GetRequired("Dynamic");
		c.Names[nameof(RateCommand)] = commandsSection.GetRequired("Rate");
		c.Names[nameof(ConvertCommand)] = commandsSection.GetRequired("Convert");

		c.DateFormat = botSection.GetRequired("DateFormat");
	});

	services.Configure<FxRateApiConfiguration>(configuration.GetRequiredSection("QuoteService:FxRateApi"));
	services.Configure<ScriptConfiguration>(configuration.GetRequiredSection("QuoteService:Script"));
}

static void ConfigureServices(IServiceCollection services)
{
	services.AddSingleton<IFxRateClient, FxRateClient>();
	services.AddSingleton<IFxRateService, FxRateService>();

	services.AddSingleton<ICommand, DynamicCommand>();
	services.AddSingleton<ICommand, RateCommand>();
	services.AddSingleton<ICommand, ConvertCommand>();

	services.AddSingleton<IBot, Bot>();
}
