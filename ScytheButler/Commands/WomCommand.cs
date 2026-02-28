using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using ScytheButler.AutoCompleteHandlers;
using ScytheButler.Models;
using ScytheButler.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace ScytheButler.Commands
{
    public class WomCommand : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly WiseOldManService _wiseOldManService;
        private readonly ulong _allowedRoleId;

        public WomCommand(WiseOldManService wiseOldManService, IConfiguration config)
        {
            _wiseOldManService = wiseOldManService;

            if (!ulong.TryParse(config["AllowedRoleId"], out _allowedRoleId))
                throw new Exception("AllowedRoleId missing or invalid in user secrets!");
        }

        [SlashCommand("createcompetition", "Create a Wise Old Man Competition")]
        public async Task CreateCompetition(
    string Title,
    [Autocomplete(typeof(MetricAutocompleteHandler))] string Metric,
    string StartDate,
    string EndDate,
    int? groupId,
    string? groupVerificationCode,
    string[]? Participants,
    Team[]? teams)
        {
            await DeferAsync();

            try
            {
                var User = (SocketGuildUser)Context.User;
                if (!User.Roles.Any(r => r.Id == _allowedRoleId))
                {
                    await FollowupAsync("❌ You do not have permission to use this command!", ephemeral: true);
                    return;
                }

                if (!Enum.TryParse<Metric>(Metric, true, out var MetricEnum))
                {
                    await FollowupAsync($"❌ Invalid metric: {Metric}", ephemeral: true);
                    return;
                }

                if (!DateTime.TryParse(StartDate, out var Start))
                {
                    await FollowupAsync("❌ Invalid start date. Use format YYYY-MM-DD.", ephemeral: true);
                    return;
                }

                if (!DateTime.TryParse(EndDate, out var End))
                {
                    await FollowupAsync("❌ Invalid end date. Use format YYYY-MM-DD.", ephemeral: true);
                    return;
                }

                if (End <= Start)
                {
                    await FollowupAsync("❌ End date must be after start date.", ephemeral: true);
                    return;
                }

                var Result = await _wiseOldManService.CreateCompetitionAsync(Title, MetricEnum, Start, End, Participants, groupId, groupVerificationCode, teams);

                await FollowupAsync($"✅ Competition **{Title}** created successfully!\nMetric: **{MetricEnum}**\nStart: **{Start:yyyy-MM-dd}**\nEnd: **{End:yyyy-MM-dd}**\n```json\n{Result}\n```", ephemeral: false);
            }
            catch (Exception Ex)
            {
                await FollowupAsync($"❌ Failed to create competition: {Ex.Message}", ephemeral: true);
            }
        }

        [SlashCommand("tileracecompetition", "Create a preconfigured Wise Old Man Competition for a team")]
        public async Task QuickCompetition(
            [Choice("Team 1", "Team 1")]
            [Choice("Team 2", "Team 2")]
            string Team)
        {
            await DeferAsync();

            try
            {
                string[] participants;
                string Title = $"TileTaceCompetition XP Competition - {Team}";
                Metric MetricEnum = Metric.Overall;
                DateTime Start = DateTime.UtcNow;
                DateTime End = DateTime.UtcNow.AddDays(14);

                if(Team == "Team 1")
                {
                    participants = new string[] { "Stans iron", "tz-tok-tizm", "axle2024", "", "" };
                }
                else if (Team == "Team 2")
                {
                    participants = new string[] { "Vedr", "Oll0", "Joeverload", "Loveskippy", "GIM M0FFY", "" };
                }
                else
                {
                    await FollowupAsync($"❌ Invalid Team Input", ephemeral: true);
                }

                // Call the service
                var Result = await _wiseOldManService.CreateCompetitionAsync(Title, MetricEnum, Start, End, null, null,null,null);

                await FollowupAsync(
                    $"✅ Quick competition created for **{Team}**!\n" +
                    $"Title: **{Title}**\n" +
                    $"Metric: **{MetricEnum}**\n" +
                    $"Start: **{Start:yyyy-MM-dd}**\n" +
                    $"End: **{End:yyyy-MM-dd}**\n" +
                    $"```json\n{Result}\n```",
                    ephemeral: false
                );
            }
            catch (Exception Ex)
            {
                await FollowupAsync($"❌ Failed to create quick competition: {Ex.Message}", ephemeral: true);
            }
        }
    }
}
