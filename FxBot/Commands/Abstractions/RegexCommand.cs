using System.Text.RegularExpressions;

namespace FxBot.Commands.Abstractions
{
	public abstract class RegexCommand : MultipleRegexCommand
	{
		public Regex ValidRegex => ValidRegexes[0];

		protected RegexCommand(string name, string validRegexArg) : base(name, new[] { validRegexArg })
		{
		}
	}
}
