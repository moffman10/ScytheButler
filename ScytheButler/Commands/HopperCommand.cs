using Discord;
using Discord.Interactions;
using System.Linq;
using System.Threading.Tasks;

namespace ScytheButler.Commands
{
    public class HopperCommand : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("hopper", "Submit users and weights for a weighted random pick.")]
        public async Task Hopper([Summary("entries", "Comma-separated list of users and weights, e.g. @Moffy 2, @Skippy 4")] string entriesInput)
        {
            var parsedEntries = HopperHelper.ParseEntries(entriesInput);

            if (!parsedEntries.Any())
            {
                await RespondAsync("No valid user mentions found. Make sure to ping users like @Moffy 2, @Skippy 4");
                return;
            }

            var result = HopperService.PickWeighted(parsedEntries);

            var embed = new EmbedBuilder()
                .WithTitle("🎲 HOPPER - Weighted Random Pick")
                .WithDescription($"**Picked:** <@{result.PickedUserId}>")
                .AddField("Entries", string.Join("\n", parsedEntries.Select(e => $"<@{e.UserId}>: {e.Weight} vote(s)")))
                .WithColor(Color.Green);

            await RespondAsync(embed: embed.Build());
        }
    }
}