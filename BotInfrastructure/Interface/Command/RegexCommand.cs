using System.Text.RegularExpressions;

namespace BotInfrastructure.Interface.Command;

public abstract class RegexCommand(string name, string validRegexArg) : MultipleRegexCommand(name, [validRegexArg])
{
	public Regex ValidRegex => ValidRegexes[0];
}