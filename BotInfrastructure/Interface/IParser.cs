using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace BotInfrastructure.Interface;

public interface IParser
{
	bool TryParseDateOrDay(string dateOrDay, Group dateGroup, Group dayGroup, [NotNullWhen(true)] out DateTime? date);
}