using Microsoft.Extensions.Configuration;
using System;

namespace FxBot;

public static class ConfigurationExtensions
{
	public static string GetRequired(this IConfiguration configuration, string key)
	{
		return configuration[key] ?? throw new ArgumentNullException(nameof(key), "Failed to load configuration key");
	}
}