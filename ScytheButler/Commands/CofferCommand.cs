using Discord;
using Discord.Interactions;
using ScytheButler.Services;
using System.Linq;
using System.Threading.Tasks;

namespace ScytheButler.Commands
{
    public class CofferCommand : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly CofferService _cofferService;

        public CofferCommand(CofferService cofferService)
        {
            _cofferService = cofferService;
        }

        [SlashCommand("coffer-total", "Show the total clan coffer and breakdown per bank.")]
        public async Task TotalCoffer()
        {
            double total = _cofferService.GetTotalBalance();
            var balances = _cofferService.GetAllBalances();

            var embed = new EmbedBuilder()
                .WithTitle("💰 Clan Coffer Balance")
                .WithColor(Color.Gold)
                .WithCurrentTimestamp()
                .AddField("Total Coffer", $"**{total:N0} coins**", inline: false);

            if (balances.Count == 0)
            {
                embed.AddField("Banks", "No banks holding data yet.", inline: false);
            }
            else
            {
                var breakdown = string.Join("\n",
                    balances.OrderByDescending(x => x.Value)
                            .Select(x => $"**{x.Key}** — {x.Value:N0} coins"));

                if (breakdown.Length > 1024)
                    breakdown = breakdown.Substring(0, 1020) + "...";

                embed.AddField("Banks", breakdown, inline: false);
            }

            await RespondAsync(embed: embed.Build());
        }

        [SlashCommand("coffer-user", "Show a specific user's coffer balance.")]
        public async Task UserCoffer([Autocomplete(typeof(CofferAutoCompleteHandler))] string username)
        {
            double balance = _cofferService.GetCofferBalance(username);

            var embed = new EmbedBuilder()
                .WithTitle($"💰 {username}'s Coffer")
                .WithDescription($"{username} has **{balance:N0} coins**")
                .WithColor(Color.DarkBlue)
                .WithCurrentTimestamp();

            await RespondAsync(embed: embed.Build());
        }

        [SlashCommand("coffer-adduser", "Add a new bank/user to the coffer database.")]
        public async Task AddNewCoffer(string username)
        {
            bool success = _cofferService.AddCoffer(username);

            var embed = new EmbedBuilder()
                .WithColor(success ? Color.Green : Color.Red)
                .WithCurrentTimestamp();

            if (success)
                embed.WithTitle("✅ User Added")
                     .WithDescription($"`{username}` has been added to the coffer database.");
            else
                embed.WithTitle("❌ User Exists")
                     .WithDescription($"`{username}` already exists in the coffer database.");

            await RespondAsync(embed: embed.Build());
        }

        [SlashCommand("coffer-removeuser", "Remove a bank/user from the coffer database.")]
        public async Task RemoveCoffer(string username)
        {
            bool success = _cofferService.RemoveCoffer(username);

            var embed = new EmbedBuilder()
                .WithColor(success ? Color.Green : Color.Red)
                .WithCurrentTimestamp();

            if (success)
                embed.WithTitle("✅ User Removed")
                     .WithDescription($"`{username}` has been removed from the coffer database.");
            else
                embed.WithTitle("❌ User Not Found")
                     .WithDescription($"`{username}` does not exist in the coffer database."); 

            await RespondAsync(embed: embed.Build());
        }

        [SlashCommand("coffer-deposit", "Deposit coins into a user's coffer.")]
        public async Task DepositCoffer(
    [Autocomplete(typeof(CofferAutoCompleteHandler))] string bank,
    string amountInput,
    [Choice("Donation", "Donation")]
    [Choice("Buy-In", "Buy-In")]
    string type,
    [Autocomplete(typeof(ReasonAutocompleteHandler))] string reason,
    string username
)
        {
            double amount;

            try
            {
                amount = _cofferService.ParseAmount(amountInput);
            }
            catch
            {
                var embedError = new EmbedBuilder()
                    .WithTitle("❌ Invalid Amount")
                    .WithDescription("Use numbers like `1000`, `10K`, or `2.5M`.")
                    .WithColor(Color.Red)
                    .WithCurrentTimestamp();

                await RespondAsync(embed: embedError.Build(), ephemeral: true);
                return;
            }

            var validReasons = _cofferService.GetAllReasons();

            if (!validReasons.Contains(reason))
            {
                await RespondAsync("❌ Invalid reason selected.", ephemeral: true);
                return;
            }

            bool success = _cofferService.AddToCoffer(bank, amount);

            var embed = new EmbedBuilder().WithCurrentTimestamp();

            if (!success)
            {
                embed.WithTitle("❌ Deposit Failed")
                     .WithDescription($"User `{bank}` was not found in the coffer.")
                     .WithColor(Color.Red);

                await RespondAsync(embed: embed.Build());
                return;
            }

            await _cofferService.AddTransactionAsync(
                username,
                amount,
                type,
                reason,
                bank
            );

            double userTotal = _cofferService.GetCofferBalance(bank);

            embed.WithTitle("✅ Deposit Successful")
                 .WithDescription(
                    $"Added **{amount:N0} coins** to `{bank}`'s coffer.\n\n" +
                    $"**Type:** {type}\n" +
                    $"**Reason:** {reason}\n\n" +
                    $"New balance: **{userTotal:N0} coins**")
                 .WithColor(Color.Green);

            await RespondAsync(embed: embed.Build());
        }

        [SlashCommand("coffer-withdraw", "Withdraw coins from a user's coffer.")]
        public async Task WithdrawCoffer( [Autocomplete(typeof(CofferAutoCompleteHandler))] string bank,
               string amountInput,
               [Choice("Payout", "Payout")][Choice("Refund", "Refund")] string type,
               [Autocomplete(typeof(ReasonAutocompleteHandler))] string reason,
               string username
        )
        {
            double amount;
            try
            {
                amount = _cofferService.ParseAmount(amountInput);
            }
            catch
            {
                var embedError = new EmbedBuilder()
                    .WithTitle("❌ Invalid Amount")
                    .WithDescription("Use numbers like `1000`, `10K`, or `2.5M`.")
                    .WithColor(Color.Red)
                    .WithCurrentTimestamp();

                await RespondAsync(embed: embedError.Build(), ephemeral: true);
                return;
            }

            var validReasons = _cofferService.GetAllReasons();
            if (!validReasons.Contains(reason))
            {
                await RespondAsync("❌ Invalid reason selected.", ephemeral: true);
                return;
            }

            bool success = _cofferService.RemoveFromCoffer(bank, amount);
            var embed = new EmbedBuilder().WithCurrentTimestamp();

            if (!success)
            {
                embed.WithTitle("❌ Withdrawal Failed")
                     .WithDescription($"Not enough coins in `{bank}`'s coffer to remove **{amount:N0} coins**.")
                     .WithColor(Color.Red);

                await RespondAsync(embed: embed.Build());
                return;
            }

            await _cofferService.AddTransactionAsync(
                username,
                -amount, 
                type,
                reason,
                bank
            );

            double userTotal = _cofferService.GetCofferBalance(bank);

            embed.WithTitle("✅ Withdrawal Successful")
                 .WithDescription(
                    $"Removed **{amount:N0} coins** from `{bank}`'s coffer.\n\n" +
                    $"**Type:** {type}\n" +
                    $"**Reason:** {reason}\n\n" +
                    $"New balance: **{userTotal:N0} coins**")
                 .WithColor(Color.Green);

            await RespondAsync(embed: embed.Build());
        }
    }
}
