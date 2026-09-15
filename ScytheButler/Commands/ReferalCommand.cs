using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Configuration;
using ScytheButler.Services;

namespace ScytheButler.Commands
{
    public class ReferralCommand : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly ReferralService _referralService;
        private readonly IConfiguration _configuration;

        public ReferralCommand(
            ReferralService referralService,
            IConfiguration configuration)
        {
            _referralService = referralService;
            _configuration = configuration;
        }

        [SlashCommand("referral", "Add one or more referrals")]
        public async Task Referral(string names)
        {
            var referrals = names
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            if (referrals.Count == 0)
            {
                await RespondAsync(
                    "❌ Please provide at least one referral name.",
                    ephemeral: true);

                return;
            }

            // Get the clan/group ID from configuration
            var groupId = 13022;
            await DeferAsync();

            try
            {
                var results = await _referralService.FindReferralsAsync(Context.User.Id,groupId,referrals);

                if (results.Count == 0)
                {
                    await FollowupAsync(
                        "❌ No referral results could be processed.");

                    return;
                }

                var successful = results
                    .Where(r => r.Successful)
                    .ToList();

                var failed = results
                    .Where(r => !r.Successful)
                    .ToList();

                var embed = new EmbedBuilder()
                    .WithTitle("Referral Results")
                    .WithTimestamp(DateTimeOffset.UtcNow);

                // Successful referrals
                if (successful.Count > 0)
                {
                    var successfulText = string.Join(
                        "\n",
                        successful.Select(r =>
                            $"✅ **{r.InputName}** → **{r.MatchedUsername}** ({r.Similarity:P0})"));

                    embed.AddField(
                        $"Successful Referrals ({successful.Count})",
                        successfulText);
                }

                // Failed referrals
                if (failed.Count > 0)
                {
                    var failedText = string.Join(
                        "\n",
                        failed.Select(r =>
                        {
                            if (r.MatchedUsername != null)
                            {
                                return
                                    $"❌ **{r.InputName}** → " +
                                    $"closest: **{r.MatchedUsername}** ({r.Similarity:P0})";
                            }

                            return $"❌ **{r.InputName}** → No match found";
                        }));

                    embed.AddField(
                        $"Unsuccessful Referrals ({failed.Count})",
                        failedText);
                }

                embed.WithFooter(
                    $"{successful.Count} referral(s) added to your total.");

                await FollowupAsync(embed: embed.Build());
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Referral command error: {ex}");

                await FollowupAsync(
                    "❌ Something went wrong while processing the referrals.");
            }
        }
    }
}