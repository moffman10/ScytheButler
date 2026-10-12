using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using ScytheButler.Services;

namespace ScytheButler.Commands
{
    public class LfrCommand : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly LfrService _lfrService;

        /*
         * ============================================================
         * LFR DISCORD ROLE IDS
         * ============================================================
         *
         * These are the Discord role IDs that get pinged when
         * an LFR of that type is created.
         */
        private const ulong ToaRoleId = 1392585930459844678;
        private const ulong ToBRoleId = 1392585768970617082;
        private const ulong HmtRoleId = 1392585833810366604;
        private const ulong CoxRoleId = 1392585878161064007;
        private const ulong CmCoxRoleId = 1392585897282768927;
        private const ulong TFARoleId = 1559107226063339580;

        public LfrCommand(LfrService lfrService)
        {
            _lfrService = lfrService;
        }

        /*
         * ============================================================
         * /LFR
         * ============================================================
         */
        [SlashCommand(
            "lfr",
            "Create a Looking For Raid party.")]
        public async Task LfrAsync()
        {
            var menu = new SelectMenuBuilder()
                .WithCustomId("lfr_raid_select")
                .WithPlaceholder("Select a raid...")
                .WithMinValues(1)
                .WithMaxValues(1)
                .AddOption(
                    "ToA",
                    "toa",
                    "Create an 8-player ToA party.",
                    emote: new Emoji("🟡"))
                .AddOption(
                    "ToB",
                    "tob",
                    "Create a ToB party.",
                    emote: new Emoji("🔴"))
                .AddOption(
                    "HMT",
                    "hmt",
                    "Create a Hard Mode ToB party.",
                    emote: new Emoji("💀"))
                .AddOption(
                    "CoX",
                    "cox",
                    "Create an unlimited-size CoX party.",
                    emote: new Emoji("🟢"))
                .AddOption(
                    "CM CoX",
                    "cmcox",
                    "Create an unlimited-size CM CoX party.",
                    emote: new Emoji("🟢"))
                .AddOption(
                    "TFA",
                    "tfa",
                    "Create an 8-player TFA party.",
                    emote: new Emoji("📚"));

            var component = new ComponentBuilder()
                .WithSelectMenu(menu)
                .Build();

            await RespondAsync(
                "Select the raid you want to create an LFR for:",
                components: component,
                ephemeral: true);
        }

        /*
         * ============================================================
         * RAID SELECTION
         * ============================================================
         */
        [ComponentInteraction("lfr_raid_select")]
        public async Task RaidSelectedAsync(
            string[] selectedValues)
        {
            if (selectedValues.Length == 0)
            {
                await RespondAsync(
                    "❌ You didn't select a raid.",
                    ephemeral: true);

                return;
            }

            var raid = selectedValues[0];

            switch (raid)
            {
                case "toa":
                    await CreateGenericPartyAsync(
                        LfrService.ToA);
                    break;
                case "tfa":
                    await CreateGenericPartyAsync(
                        LfrService.TFA);
                    break;

                case "cox":
                    await CreateGenericPartyAsync(
                        LfrService.CoX);
                    break;

                case "cmcox":
                    await CreateGenericPartyAsync(
                        LfrService.CmCoX);
                    break;

                case "tob":
                    await ShowTeamSizeMenuAsync(
                        LfrService.ToB);
                    break;

                case "hmt":
                    await ShowTeamSizeMenuAsync(
                        LfrService.HMT);
                    break;

                default:
                    await RespondAsync(
                        "❌ Invalid raid selection.",
                        ephemeral: true);

                    break;
            }
        }

        /*
         * ============================================================
         * TEAM SIZE MENU
         * ============================================================
         */
        private async Task ShowTeamSizeMenuAsync(
            string raid)
        {
            var menu = new SelectMenuBuilder()
                .WithCustomId($"lfr_size_select:{raid}")
                .WithPlaceholder("Select team size...")
                .WithMinValues(1)
                .WithMaxValues(1)
                .AddOption(
                    "3 Man",
                    "3",
                    "3-player team.")
                .AddOption(
                    "4 Man",
                    "4",
                    "4-player team.")
                .AddOption(
                    "5 Man",
                    "5",
                    "5-player team.");

            var component = new ComponentBuilder()
                .WithSelectMenu(menu)
                .Build();

            await RespondAsync(
                $"You selected **{raid}**.\n\n" +
                "Select the team size:",
                components: component,
                ephemeral: true);
        }

        /*
         * ============================================================
         * TEAM SIZE SELECTED
         * ============================================================
         */
        [ComponentInteraction("lfr_size_select:*")]
        public async Task TeamSizeSelectedAsync(
            string raid,
            string[] selectedValues)
        {
            if (selectedValues.Length == 0)
            {
                await RespondAsync(
                    "❌ You didn't select a team size.",
                    ephemeral: true);

                return;
            }

            if (!int.TryParse(
                    selectedValues[0],
                    out var teamSize))
            {
                await RespondAsync(
                    "❌ Invalid team size.",
                    ephemeral: true);

                return;
            }

            if (teamSize != 3 &&
                teamSize != 4 &&
                teamSize != 5)
            {
                await RespondAsync(
                    "❌ Invalid team size.",
                    ephemeral: true);

                return;
            }

            var raidType = raid switch
            {
                "ToB" => LfrService.ToB,
                "HMT" => LfrService.HMT,
                _ => null
            };

            if (raidType == null)
            {
                await RespondAsync(
                    "❌ Invalid raid.",
                    ephemeral: true);

                return;
            }

            await CreateRolePartyAsync(
                raidType,
                teamSize);
        }

        /*
         * ============================================================
         * CREATE GENERIC PARTY
         * ============================================================
         *
         * Creating an LFR does NOT make the creator a member.
         *
         * A user can create/own multiple LFRs.
         */
        private async Task CreateGenericPartyAsync(
            string raidType)
        {
            var user = Context.User;

            var guildId =
                ToDatabaseId(Context.Guild.Id);

            var channelId =
                ToDatabaseId(Context.Channel.Id);

            var userId =
                ToDatabaseId(user.Id);

            await DeferAsync(
                ephemeral: true);

            var channel = Context.Channel;

            if (channel is not IMessageChannel messageChannel)
            {
                await FollowupAsync(
                    "❌ I could not access this channel.",
                    ephemeral: true);

                return;
            }

            /*
             * Send the role ping as a completely separate message.
             */
            await SendRolePingAsync(
                messageChannel,
                raidType);

            try
            {
                /*
                 * Send the actual LFR message.
                 */
                var publicMessage =
                    await messageChannel.SendMessageAsync(
                        embed: BuildCreatingEmbed(
                            raidType,
                            0,
                            userId));

                var party =
                    await _lfrService.CreatePartyAsync(
                        guildId,
                        channelId,
                        ToDatabaseId(publicMessage.Id),
                        userId,
                        raidType);

                /*
                 * Creator is NOT automatically added.
                 */
                await UpdatePartyMessageAsync(
                    party.Id);

                await FollowupAsync(
                    "✅ Your LFR party has been created. " +
                    "You can create additional LFRs as well. " +
                    "Use the **Join Party** button if you want to join this one.",
                    ephemeral: true);
            }
            catch (Exception ex)
            {
                await FollowupAsync(
                    $"❌ {ex.Message}",
                    ephemeral: true);
            }
        }

        /*
         * ============================================================
         * CREATE TOB/HMT PARTY
         * ============================================================
         */
        private async Task CreateRolePartyAsync(
            string raidType,
            int teamSize)
        {
            var user = Context.User;

            var guildId =
                ToDatabaseId(Context.Guild.Id);

            var channelId =
                ToDatabaseId(Context.Channel.Id);

            var userId =
                ToDatabaseId(user.Id);

            await DeferAsync(
                ephemeral: true);

            var channel = Context.Channel;

            if (channel is not IMessageChannel messageChannel)
            {
                await FollowupAsync(
                    "❌ I could not access this channel.",
                    ephemeral: true);

                return;
            }

            /*
             * Send the role ping as a completely separate message.
             */
            await SendRolePingAsync(
                messageChannel,
                raidType);

            try
            {
                /*
                 * Send the actual LFR message.
                 */
                var publicMessage =
                    await messageChannel.SendMessageAsync(
                        embed: BuildCreatingEmbed(
                            raidType,
                            teamSize,
                            userId));

                var party =
                    await _lfrService.CreatePartyAsync(
                        guildId,
                        channelId,
                        ToDatabaseId(publicMessage.Id),
                        userId,
                        raidType,
                        teamSize);

                /*
                 * Creator is NOT automatically assigned a role.
                 */
                await UpdatePartyMessageAsync(
                    party.Id);

                await FollowupAsync(
                    $"✅ Your **{raidType} {teamSize}-man** LFR has been created. " +
                    "You can create additional LFRs as well. " +
                    "Select a role below if you want to join this one.",
                    ephemeral: true);
            }
            catch (Exception ex)
            {
                await FollowupAsync(
                    $"❌ {ex.Message}",
                    ephemeral: true);
            }
        }

        /*
         * ============================================================
         * SEND ROLE PING
         * ============================================================
         */
        private async Task SendRolePingAsync(
            IMessageChannel channel,
            string raidType)
        {
            var roleId =
                GetRoleId(raidType);

            if (roleId == 0)
                return;

            var role =
                Context.Guild.GetRole(roleId);

            if (role == null)
            {
                return;
            }

            var allowedMentions =
                new AllowedMentions
                {
                    RoleIds = new List<ulong>
                    {
                        role.Id
                    }
                };

            await channel.SendMessageAsync(
                text: role.Mention,
                allowedMentions: allowedMentions);
        }

        /*
         * ============================================================
         * JOIN PARTY
         * ============================================================
         */
        [ComponentInteraction("lfr_join:*")]
        public async Task JoinPartyButtonAsync(
            string partyIdValue)
        {
            if (!int.TryParse(
                    partyIdValue,
                    out var partyId))
            {
                await RespondAsync(
                    "❌ Invalid LFR party.",
                    ephemeral: true);

                return;
            }

            await DeferAsync(
                ephemeral: true);

            try
            {
                await _lfrService.JoinPartyAsync(
                    partyId,
                    ToDatabaseId(Context.User.Id),
                    Context.User.Username);

                await UpdatePartyMessageAsync(
                    partyId);

                await FollowupAsync(
                    $"✅ You joined the LFR party, {Context.User.Mention}.",
                    ephemeral: true);
            }
            catch (Exception ex)
            {
                await FollowupAsync(
                    $"❌ {ex.Message}",
                    ephemeral: true);
            }
        }

        /*
         * ============================================================
         * ROLE BUTTON
         * ============================================================
         */
        [ComponentInteraction("lfr_role:*:*")]
        public async Task RoleButtonAsync(
            string partyIdValue,
            string role)
        {
            if (!int.TryParse(
                    partyIdValue,
                    out var partyId))
            {
                await RespondAsync(
                    "❌ Invalid LFR party.",
                    ephemeral: true);

                return;
            }

            await DeferAsync(
                ephemeral: true);

            try
            {
                await _lfrService.JoinWithRoleAsync(
                    partyId,
                    ToDatabaseId(Context.User.Id),
                    Context.User.Username,
                    role);

                await UpdatePartyMessageAsync(
                    partyId);

                await FollowupAsync(
                    $"✅ You are now **{role}** in the LFR party.",
                    ephemeral: true);
            }
            catch (Exception ex)
            {
                await FollowupAsync(
                    $"❌ {ex.Message}",
                    ephemeral: true);
            }
        }

        /*
         * ============================================================
         * LEAVE PARTY
         * ============================================================
         */
        [ComponentInteraction("lfr_leave:*")]
        public async Task LeavePartyButtonAsync(
            string partyIdValue)
        {
            if (!int.TryParse(
                    partyIdValue,
                    out var partyId))
            {
                await RespondAsync(
                    "❌ Invalid LFR party.",
                    ephemeral: true);

                return;
            }

            await DeferAsync(
                ephemeral: true);

            try
            {
                await _lfrService.LeavePartyAsync(
                    partyId,
                    ToDatabaseId(Context.User.Id));

                await UpdatePartyMessageAsync(
                    partyId);

                await FollowupAsync(
                    "✅ You left the LFR party.",
                    ephemeral: true);
            }
            catch (Exception ex)
            {
                await FollowupAsync(
                    $"❌ {ex.Message}",
                    ephemeral: true);
            }
        }

        /*
         * ============================================================
         * CLOSE PARTY
         * ============================================================
         */
        [ComponentInteraction("lfr_close:*")]
        public async Task ClosePartyButtonAsync(
            string partyIdValue)
        {
            if (!int.TryParse(
                    partyIdValue,
                    out var partyId))
            {
                await RespondAsync(
                    "❌ Invalid LFR party.",
                    ephemeral: true);

                return;
            }

            await DeferAsync(
                ephemeral: true);

            try
            {
                await _lfrService.ClosePartyAsync(
                    partyId,
                    ToDatabaseId(Context.User.Id));

                await UpdatePartyMessageAsync(
                    partyId);

                await FollowupAsync(
                    "🔒 The LFR party has been closed.",
                    ephemeral: true);
            }
            catch (Exception ex)
            {
                await FollowupAsync(
                    $"❌ {ex.Message}",
                    ephemeral: true);
            }
        }

        [ComponentInteraction("lfr_bump:*")]
        public async Task BumpPartyButtonAsync(
            string partyIdValue)
        {
            if (!int.TryParse(
                    partyIdValue,
                    out var partyId))
            {
                await RespondAsync(
                    "❌ Invalid LFR party.",
                    ephemeral: true);

                return;
            }

            await DeferAsync(
                ephemeral: true);

            try
            {
                var party =
                    await _lfrService.GetPartyAsync(
                        partyId);

                if (party == null)
                {
                    await FollowupAsync(
                        "❌ That LFR party no longer exists.",
                        ephemeral: true);

                    return;
                }

                if (party.IsClosed)
                {
                    await FollowupAsync(
                        "❌ This LFR party is closed.",
                        ephemeral: true);

                    return;
                }

                /*
                 * Only the creator can bump their own LFR.
                 */
                if (party.CreatorId !=
                    ToDatabaseId(Context.User.Id))
                {
                    await FollowupAsync(
                        "❌ Only the creator of this LFR can bump it.",
                        ephemeral: true);

                    return;
                }

                var channel =
                    Context.Client.GetChannel(
                        ToDiscordId(party.ChannelId))
                    as IMessageChannel;

                if (channel == null)
                {
                    await FollowupAsync(
                        "❌ I could not access the LFR channel.",
                        ephemeral: true);

                    return;
                }

                var oldMessageId =
                    ToDiscordId(party.MessageId);

                /*
                 * Build the exact current state of the LFR.
                 */
                var embed =
                    BuildPartyEmbed(party);

                var components =
                    BuildPartyComponents(party);

                /*
                 * Create the replacement message FIRST.
                 */
                var newMessage =
                    await channel.SendMessageAsync(
                        embed: embed,
                        components: components);

                /*
                 * Point the database at the replacement message.
                 */
                await _lfrService.UpdatePartyMessageIdAsync(
                    party.Id,
                    ToDatabaseId(newMessage.Id));

                /*
                 * Delete the old message.
                 */
                try
                {
                    await channel.DeleteMessageAsync(
                        oldMessageId);
                }
                catch
                {
                    /*
                     * The new message and database are already valid.
                     *
                     * If the old message was already deleted or
                     * Discord refuses deletion, don't destroy the
                     * newly-created LFR.
                     */
                }

                await FollowupAsync(
                    "📢 LFR bumped to the bottom of the channel.",
                    ephemeral: true);
            }
            catch (Exception ex)
            {
                await FollowupAsync(
                    $"❌ {ex.Message}",
                    ephemeral: true);
            }
        }

        /*
         * ============================================================
         * UPDATE PARTY MESSAGE
         * ============================================================
         */
        public async Task UpdatePartyMessageAsync(
            int partyId)
        {
            var party =
                await _lfrService.GetPartyAsync(
                    partyId);

            if (party == null)
                return;

            var channel =
                Context.Client.GetChannel(
                    ToDiscordId(party.ChannelId))
                as IMessageChannel;

            if (channel == null)
                return;

            var message =
                await channel.GetMessageAsync(
                    ToDiscordId(party.MessageId))
                as IUserMessage;

            if (message == null)
                return;

            var embed =
                BuildPartyEmbed(party);

            var components =
                BuildPartyComponents(party);

            await message.ModifyAsync(
                properties =>
                {
                    properties.Content = string.Empty;
                    properties.Embed = embed;
                    properties.Components = components;
                });
        }

        /*
         * ============================================================
         * PARTY CREATION EMBED
         * ============================================================
         */
        private Embed BuildCreatingEmbed(
            string raidType,
            int teamSize,
            long creatorId)
        {
            var title =
                $"{GetRaidEmoji(raidType)} {raidType}";

            if (raidType == LfrService.ToB ||
                raidType == LfrService.HMT)
            {
                title += $" {teamSize}-Man";
            }

            return new EmbedBuilder()
                .WithTitle(title)
                .WithDescription(
                    $"👑 **Leader:** {MentionUser(creatorId)}\n\n" +
                    "Creating LFR party...")
                .Build();
        }

        /*
         * ============================================================
         * PARTY EMBED
         * ============================================================
         */
        private Embed BuildPartyEmbed(
            ScytheButler.data.DatabaseModels.Lfr.LfrParty party)
        {
            var creator =
                MentionUser(party.CreatorId);

            var builder =
                new EmbedBuilder()
                    .WithTitle(
                        $"{GetRaidEmoji(party.RaidType)} " +
                        $"{party.RaidType}" +
                        GetTeamSizeText(party))
                    .WithDescription(
                        $"👑 **Leader:** {creator}")
                    .WithFooter(
                        party.IsClosed
                            ? "LFR Closed"
                            : "LFR Party")
                    .WithTimestamp(
                        party.CreatedAt);

            if (party.RaidType == LfrService.ToB ||
                party.RaidType == LfrService.HMT)
            {
                AddRoleFields(
                    builder,
                    party);
            }
            else
            {
                AddGenericMemberField(
                    builder,
                    party);
            }

            if (party.IsClosed)
            {
                builder.Description +=
                    "\n\n🔒 **This LFR is closed.**";
            }

            return builder.Build();
        }

        /*
         * ============================================================
         * TOB/HMT ROLE FIELDS
         * ============================================================
         */
        private void AddRoleFields(
            EmbedBuilder builder,
            ScytheButler.data.DatabaseModels.Lfr.LfrParty party)
        {
            var roles =
                GetRolesForTeamSize(
                    party.TeamSize);

            foreach (var role in roles)
            {
                var member =
                    party.Members.FirstOrDefault(
                        x => x.Role == role);

                var value =
                    member == null
                        ? "Empty"
                        : MentionUser(member.UserId);

                builder.AddField(
                    role,
                    value,
                    inline: true);
            }

            builder.AddField(
                "Players",
                $"{party.Members.Count}/{party.TeamSize}");
        }

        /*
         * ============================================================
         * NORMAL PARTY MEMBER FIELD
         * ============================================================
         */
        private void AddGenericMemberField(
            EmbedBuilder builder,
            ScytheButler.data.DatabaseModels.Lfr.LfrParty party)
        {
            var lines =
                new List<string>();

            foreach (var member in party.Members)
            {
                lines.Add(
                    $"• {MentionUser(member.UserId)}");
            }

            if (lines.Count == 0)
            {
                lines.Add(
                    "No players yet.");
            }

            lines.Add("");

            if (party.RaidType == LfrService.ToA || party.RaidType == LfrService.TFA)
            {
                lines.Add(
                    $"**Players:** {party.Members.Count}/8");
            }
            else
            {
                lines.Add(
                    $"**Players:** {party.Members.Count}");
            }

            builder.AddField(
                "Players",
                string.Join(
                    "\n",
                    lines));
        }

        /*
         * ============================================================
         * PARTY BUTTONS
         * ============================================================
         */
        private MessageComponent BuildPartyComponents(
            ScytheButler.data.DatabaseModels.Lfr.LfrParty party)
        {
            var builder =
                new ComponentBuilder();

            /*
             * Closed parties still have Leave Party.
             */
            if (party.IsClosed)
            {
                builder.WithButton(
                    new ButtonBuilder()
                        .WithLabel("Leave Party")
                        .WithCustomId(
                            $"lfr_leave:{party.Id}")
                        .WithStyle(
                            ButtonStyle.Secondary));

                return builder.Build();
            }

            /*
             * ToB/HMT role buttons.
             */
            if (party.RaidType == LfrService.ToB ||
                party.RaidType == LfrService.HMT)
            {
                foreach (var role in
                         GetRolesForTeamSize(
                             party.TeamSize))
                {
                    builder.WithButton(
                        new ButtonBuilder()
                            .WithLabel(role)
                            .WithCustomId(
                                $"lfr_role:{party.Id}:{role}")
                            .WithStyle(
                                ButtonStyle.Primary));
                }
            }
            else
            {
                /*
                 * ToA/CoX/CM CoX Join button.
                 */
                builder.WithButton(
                    new ButtonBuilder()
                        .WithLabel("Join Party")
                        .WithCustomId(
                            $"lfr_join:{party.Id}")
                        .WithStyle(
                            ButtonStyle.Success));
            }

            /*
             * Leave.
             */
            builder.WithButton(
                new ButtonBuilder()
                    .WithLabel("Leave Party")
                    .WithCustomId(
                        $"lfr_leave:{party.Id}")
                    .WithStyle(
                        ButtonStyle.Secondary));

            /*
             * Bump.
             */
            builder.WithButton(
                new ButtonBuilder()
                    .WithLabel("Bump")
                    .WithCustomId(
                        $"lfr_bump:{party.Id}")
                    .WithStyle(
                        ButtonStyle.Primary));

            /*
             * Close.
             */
            builder.WithButton(
                new ButtonBuilder()
                    .WithLabel("Close Party")
                    .WithCustomId(
                        $"lfr_close:{party.Id}")
                    .WithStyle(
                        ButtonStyle.Danger));

            return builder.Build();
        }

        /*
         * ============================================================
         * ROLE ID
         * ============================================================
         */
        private static ulong GetRoleId(
            string raidType)
        {
            return raidType switch
            {
                LfrService.ToA => ToaRoleId,
                LfrService.ToB => ToBRoleId,
                LfrService.HMT => HmtRoleId,
                LfrService.CoX => CoxRoleId,
                LfrService.CmCoX => CmCoxRoleId,
                LfrService.TFA => TFARoleId,
                _ => 0UL
            };
        }

        /*
         * ============================================================
         * RAID EMOJIS
         * ============================================================
         */
        private static string GetRaidEmoji(
            string raidType)
        {
            return raidType switch
            {
                LfrService.ToA => "🟡",
                LfrService.ToB => "🔴",
                LfrService.HMT => "💀",
                LfrService.CoX => "🟢",
                LfrService.CmCoX => "🟢",
                LfrService.TFA => "📚",
                _ => "🟡"
            };
        }

        /*
         * ============================================================
         * ROLE LIST
         * ============================================================
         */
        private static string[] GetRolesForTeamSize(
            int teamSize)
        {
            return teamSize switch
            {
                3 => new[]
                {
                    LfrService.Mage,
                    LfrService.RDPS,
                    LfrService.MDPS
                },

                4 => new[]
                {
                    LfrService.Mage,
                    LfrService.NFRZ,
                    LfrService.RDPS,
                    LfrService.MDPS
                },

                5 => new[]
                {
                    LfrService.Mage,
                    LfrService.NFRZ,
                    LfrService.RDPS,
                    LfrService.MDPS,
                    LfrService.MDPS2
                },

                _ => Array.Empty<string>()
            };
        }

        /*
         * ============================================================
         * TEAM SIZE TEXT
         * ============================================================
         */
        private static string GetTeamSizeText(
            ScytheButler.data.DatabaseModels.Lfr.LfrParty party)
        {
            if (party.RaidType == LfrService.ToB ||
                party.RaidType == LfrService.HMT)
            {
                return $" {party.TeamSize}-Man";
            }

            return string.Empty;
        }

        /*
         * ============================================================
         * DISCORD ID <-> DATABASE ID
         * ============================================================
         */
        private static long ToDatabaseId(
            ulong discordId)
        {
            return checked((long)discordId);
        }

        private static ulong ToDiscordId(
            long databaseId)
        {
            return checked((ulong)databaseId);
        }

        /*
         * ============================================================
         * DISCORD MENTION
         * ============================================================
         */
        private static string MentionUser(
            long userId)
        {
            return $"<@{userId}>";
        }
    }
}