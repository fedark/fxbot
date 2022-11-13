using System;
using System.Configuration;

namespace FxBot
{
	public class Settings
	{
		public static readonly string BotTokenKey = "BotToken";
		public static readonly string CommandDynamicKey = "CommandDynamic";
		public static readonly string CommandRateKey = "CommandRate";
		public static readonly string CommandConvertKey = "CommandConvert";
		public static readonly string DateFormatKey = "DateFormat";

		public static string GetRequired(string key)
		{
			var setting = ConfigurationManager.AppSettings[key];
			if (setting is not null)
			{
				return setting;
			}

			throw new Exception($"Well, shit: Setting '{key}' is not found.");
		}
	}
}
