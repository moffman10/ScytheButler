using Discord.Interactions;
using System.Threading.Tasks;

public class PingCommand : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("ping", "Check if the bot is alive")]
    public async Task Ping()
    {
        await RespondAsync("Pong!");
    }
}
