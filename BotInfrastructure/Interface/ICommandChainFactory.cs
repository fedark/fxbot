using BotInfrastructure.Interface.Command;

namespace BotInfrastructure.Interface;

public interface ICommandChainFactory
{
	ICommand? GetDefaultChain();
	IRepliableCommand? GetReplyChain();
}