using BotInfrastructure.Interface;
using BotInfrastructure.Interface.Command;

namespace BotInfrastructure.Impl;

public class CommandChainFactory(
    IRateCommand rateCommand,
    IConvertCommand convertCommand,
    IHistoryCommand historyCommand,
    IWrongCommand wrongCommand)
    : ICommandChainFactory
{
    public ICommand? GetDefaultChain()
    {
        return new CommandChainBuilder()
            .FollowBy(rateCommand)
            .FollowBy(convertCommand)
            .FollowBy(historyCommand)
            .FollowBy(wrongCommand)
            .Build();
    }

    public IRepliableCommand? GetReplyChain()
    {
        return (IRepliableCommand?)new CommandChainBuilder()
			.FollowBy(historyCommand)
            .Build();
    }
}