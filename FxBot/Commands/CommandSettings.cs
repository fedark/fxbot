using System.Collections.Generic;

namespace FxBot.Commands
{
	public class CommandSettings
	{
		public Dictionary<string, string> Names { get; set; } = new();
		public string DateFormat { get; set; } = default!;
	}
}
