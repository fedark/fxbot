using BotInfrastructure.Interface;
using BotInfrastructure.Interface.Command;

namespace BotInfrastructure.Impl;

public class CommandChainBuilder : ICommandChainBuilder
{
	private ICommand? _chainStart;
	private ICommand? _chainEnd;

	public ICommandChainBuilder FollowBy(ICommand command)
	{
		if (_chainStart is null)
		{
			_chainStart = command;
			_chainEnd = _chainStart;
		}
		else
		{
			_chainEnd!.Next = command;
			_chainEnd = _chainEnd.Next;
		}

		return this;
	}

	public ICommand? Build()
	{
		return _chainStart;
	}
}