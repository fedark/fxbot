using System.Collections.Generic;

namespace FxBot.Commands
{
	public class CommandConfiguration
	{
		public Dictionary<string, string> Names { get; set; } = new();
		public string DateFormat { get; set; } = default!;
	}
}
