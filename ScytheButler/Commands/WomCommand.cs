using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using ScytheButler.AutoCompleteHandlers;
using ScytheButler.Helpers.Wom_Helpers;
using ScytheButler.Models;
using ScytheButler.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
    string title,
    [Autocomplete(typeof(MetricAutocompleteHandler))] string metric,
    string startDate,
    string endDate,
    string? participants = null,
    string? teams = null,
    int? groupId = null,
    string? groupVerificationCode = null)
        {
            await DeferAsync();

            var user = (SocketGuildUser)Context.User;

            if (!user.Roles.Any(r => r.Id == _allowedRoleId))
            {
                await FollowupAsync("❌ You do not have permission to use this command!", ephemeral: true);
                return;
            }

            var result = await _wiseOldManService.CreateCompetitionFromInputAsync(
                title,
                metric,
                startDate,
                endDate,
                participants,
                teams,
                groupId,
                groupVerificationCode);

            var embed = WomEmbedBuilder.BuildCompetitionEmbedAsync(result);

            await FollowupAsync(embed: embed);
        }

    }
}