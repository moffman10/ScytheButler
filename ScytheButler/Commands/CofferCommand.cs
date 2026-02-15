using Discord;
using Discord.Interactions;
using ScytheButler.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace ScytheButler.Commands
{
    public class CofferCommand : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly CofferService _cofferService;

        public CofferCommand(CofferService cofferService)
        {
            _cofferService = cofferService;
        }


        [SlashCommand("coffer-total", "Check the current clan coffer balance")]
        public async Task TotalCoffer()
        {
            long total = _cofferService.GetTotalBalance();
            var balances = _cofferService.GetAllBalances();

            var embedBuilder = new EmbedBuilder().WithTitle("💰 Clan Coffer Balance").WithColor(Color.Gold).WithCurrentTimestamp();

            embedBuilder.AddField("Total Coffer", $"**{total:N0} coins**", inline: false);

            if (balances.Count == 0)
            {
                embedBuilder.AddField("Banks", "No banks holding data yet.", inline: false);
            }
            else
            {
                // Build a simple breakdown
                var breakdown = string.Join("\n",
                    balances.OrderByDescending(x => x.Value)
                            .Select(x => $"**{x.Key}** — {x.Value:N0} coins"));

                // Truncate if too long
                if (breakdown.Length > 1024)
                    breakdown = breakdown.Substring(0, 1020) + "...";

                embedBuilder.AddField("Banks", breakdown, inline: false);
            }
            var embed = embedBuilder.Build();
            await RespondAsync(embed: embed);
        }
        [SlashCommand("coffer-user", "Check the selected users coffer")]
        public async Task UserCoffer([Autocomplete(typeof(CofferAutoCompleteHandler))] string username)
        {
            long balance = _cofferService.GetCofferBalance(username);
            await RespondAsync($"💰 {username} has **{balance:N0} coins**");
        }
        [SlashCommand("coffer-adduser", "Add a new user to the coffer database")]
        public async Task AddNewCoffer(string username)
        {
            bool success = _cofferService.AddCoffer(username);
            if (success)
                await RespondAsync($"✅ Added new user `{username}` to the coffer.");
            else
                await RespondAsync($"❌ User `{username}` already exists.");
        }
        [SlashCommand("coffer-removeuser", "Remove a user to the coffer database.")]
        public async Task removeCoffer(string username)
        {
            bool success = _cofferService.RemoveCoffer(username);
            if (success)
                await RespondAsync($"✅ Added new user `{username}` to the coffer.");
            else
                await RespondAsync($"❌ User `{username}` already exists.");
        }
        [SlashCommand("coffer-deposit", "Deposit gp into the coffer")]
        public async Task DepositCoffer([Autocomplete(typeof(CofferAutoCompleteHandler))] string username, string amountInput)
        {
            long amount;
            try
            {
                amount = _cofferService.ParseAmount(amountInput);
            }
            catch
            {
                await RespondAsync("❌ Invalid amount format. Use numbers like `1000`, `10K`, `2.5M`.");
                return;
            }

            _cofferService.AddToCoffer(username, amount);
            long userTotal = _cofferService.GetCofferBalance(username);
            await RespondAsync($"✅ Added {amount:N0} coins to {username}, coffer now has: {userTotal:N0} coins");
        }

        [SlashCommand("coffer-withdraw", "Withdraw gp from the coffer")]
        public async Task WithdrawCoffer([Autocomplete(typeof(CofferAutoCompleteHandler))] string username, string amountInput)
        {
            long amount;
            try
            {
                amount = _cofferService.ParseAmount(amountInput);
            }
            catch
            {
                await RespondAsync("❌ Invalid amount format. Use numbers like `1000`, `10K`, `2.5M`.");
                return;
            }
            bool success = _cofferService.RemoveFromCoffer(username, amount);
            if (success)
            {
                long userTotal = _cofferService.GetCofferBalance(username);
                await RespondAsync($"✅ Removed {amount:N0} coins to {username}, coffer now has: {userTotal:N0} coins");
            }
            else
            {
                await RespondAsync($"❌ Not enough coins in the coffer to remove {amount:N0}.");
            }
        }
    }
}
