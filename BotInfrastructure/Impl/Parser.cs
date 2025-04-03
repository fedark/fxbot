using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using BotInfrastructure.Interface;

namespace BotInfrastructure.Impl;

public class Parser : IParser
{
	public bool TryParseDateOrDay(string dateOrDay, Group dateGroup, Group dayGroup, [NotNullWhen(true)] out DateTime? date)
	{
		if (dateGroup.Success &&
		    DateTime.TryParse(dateGroup.Value, out var parsedDate))
		{
			date = parsedDate;
			return true;
		}

		if (dayGroup.Success &&
		    int.TryParse(dayGroup.Value, out var day) &&
		    day >= 1 &&
		    DateTime.Now is var now &&
		    day <= DateTime.DaysInMonth(now.Year, now.Month))
		{
			date = new DateTime(now.Year, now.Month, day);
			return true;
		}

		date = default;
		return false;
	}
}