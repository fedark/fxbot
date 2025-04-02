namespace BotInfrastructure.Interface.Command;

public abstract class RegexCommand(string name, string validRegexArg) : MultipleRegexCommand(name, [validRegexArg])
{
}