using System;
using System.IO;
using FxBot.Commands;
using FxBot.Commands.Abstractions;
using FxBot.Commands.Implementation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuoteService;

namespace FxBot
{
	public class Setup
	{
		private IConfigurationRoot Configuration { get; }

		public Setup()
		{
			Configuration = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", false, true)
				.Build();
		}

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddLogging(builder =>
			{
				builder.AddConfiguration(Configuration.GetRequiredSection("Logging"));
				builder.AddConsole();
				builder.AddDebug();
			});
			services.AddOptions();

			services.Configure<BotSettings>(opt =>
			{
				var botSection = Configuration.GetRequiredSection("Bot");
				opt.Token = botSection.GetRequired("Token");
			});

			services.Configure<CommandSettings>(opt =>
			{
				var botSection = Configuration.GetRequiredSection("Bot");
				var commandsSection = botSection.GetRequiredSection("Commands");

				opt.Names[nameof(DynamicCommand)] = commandsSection.GetRequired("Dynamic");
				opt.Names[nameof(RateCommand)] = commandsSection.GetRequired("Rate");
				opt.Names[nameof(ConvertCommand)] = commandsSection.GetRequired("Convert");

				opt.DateFormat = botSection.GetRequired("DateFormat");
			});

			services.AddSingleton<IFxRateService, FxRateService>();

			services.AddSingleton<ICommand, DynamicCommand>();
			services.AddSingleton<ICommand, RateCommand>();
			services.AddSingleton<ICommand, ConvertCommand>();

			services.AddSingleton<Bot>();
		}
	}

	public static class ConfigurationExtensions
	{
		public static string GetRequired(this IConfiguration configuration, string key)
		{
			return configuration[key] ?? throw new ArgumentNullException(nameof(key), "Failed to load configuration key");
		}
	}
}
