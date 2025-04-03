namespace BotInfrastructure.Abstract;

public abstract class RegexCommand(string name, string validRegexArg) : MultipleRegexCommand(name, [validRegexArg])
{
}