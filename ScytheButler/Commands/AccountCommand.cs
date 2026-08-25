using Discord;
using Discord.Interactions;
using ScytheButler.Services;
using System.Threading.Tasks;

namespace ScytheButler.Commands
{
    public class AccountCommand : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly AccountService _accountService;

        public AccountCommand(AccountService accountService)
        {
            _accountService = accountService;
        }

        [SlashCommand("link", "Link an account.")]
        public async Task LinkAccounts(
            [Summary("type", "Choose whether this is a primary or tertiary account")]
            [Choice("Primary", "primary")]
            [Choice("Tertiary", "tertiary")]
            string linkType,

            [Summary("accountname", "The account username")]
            string accountName,

            [Summary("accounttype", "The account type (e.g. main, pure, gim)")]
            string accountType)
        {
            var discordId = Context.User.Id;
            var currentUsername = Context.User.Username;

            var result = await _accountService.ProcessLinkAccountsAsync(discordId,accountName,accountType,linkType);

            var embed = new EmbedBuilder()
                .WithCurrentTimestamp();

            if (!result.Success)
            {
                embed.WithTitle("❌ Link Failed")
                     .WithDescription(result.Message)
                     .WithColor(Color.Red);

                await RespondAsync(
                    embed: embed.Build(),
                    ephemeral: true);

                return;
            }

            embed.WithTitle("✅ Account Linked Successfully")
                 .WithDescription(
                     $"Successfully linked **{accountName}** as **{linkType}**:\n{result.Message}")
                 .WithColor(Color.Green);

            await RespondAsync(
                embed: embed.Build(),
                ephemeral: true);
        }
    }
}