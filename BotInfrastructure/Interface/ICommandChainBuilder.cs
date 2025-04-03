using BotInfrastructure.Interface.Command;

namespace BotInfrastructure.Interface;

public interface ICommandChainBuilder
{
	ICommandChainBuilder FollowBy(ICommand command);
	ICommand? Build();
}