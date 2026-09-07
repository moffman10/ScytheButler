using Discord.Interactions;
using ScytheButler.Services;
using System.Threading.Tasks;

namespace ScytheButler.Commands
{
    public class AchievementAdminModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly AchievementService _achievementService;

        public AchievementAdminModule(AchievementService achievementService)
        {
            _achievementService = achievementService;
        }

        [SlashCommand("deploy-achievements", "Deploys the full ScytheBound Legends achievement panels to the specified channel.")]
        public async Task DeployAchievementsAsync(Discord.WebSocket.SocketTextChannel channel)
        {
            await DeferAsync(ephemeral: true);

            var result = await _achievementService.DeployPanelsToChannelAsync(channel.Id.ToString());

            if (result.Count > 0)
            {
                await FollowupAsync($"Successfully deployed panels! Tracked {result.Count} message IDs for future updates.", ephemeral: true);
            }
            else
            {
                await FollowupAsync("Deployment failed or encountered an error. Check console logs.", ephemeral: true);
            }
        }
    }
}