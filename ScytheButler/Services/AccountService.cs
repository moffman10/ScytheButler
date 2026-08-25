using ScytheButler.data.DatabaseModels.WomModels;
using ScytheButler.Data;
using ScytheButler.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace ScytheButler.Services
{
    public class AccountService
    {
        private readonly AppDbContext _db;

        public AccountService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<(bool Success, string Message, int Count)> ProcessLinkAccountsAsync(
    ulong discordId,
    string accountName,
    string accountType,
    string linkType)
        {
            linkType = linkType.Trim().ToLower();
            accountName = accountName.Trim();
            accountType = accountType.Trim().ToLower();

            // Validate link type
            if (linkType != "primary" && linkType != "tertiary")
            {
                return (
                    false,
                    "❌ Link type must be either `primary` or `tertiary`.",
                    0
                );
            }

            // Validate account name
            if (string.IsNullOrWhiteSpace(accountName))
            {
                return (
                    false,
                    "❌ Please provide an account name.",
                    0
                );
            }

            // Validate account type
            if (string.IsNullOrWhiteSpace(accountType))
            {
                return (
                    false,
                    "❌ Please provide an account type.",
                    0
                );
            }

            // Find the player case-insensitively
            var player = _db.Players
                .SingleOrDefault(p =>
                    p.Username.ToLower() == accountName.ToLower());

            if (player == null)
            {
                return (
                    false,
                    $"❌ The account **{accountName}** was not found in the database.",
                    0
                );
            }

            // If assigning a primary, demote the existing primary.
            if (linkType == "primary")
            {
                var existingPrimary = _db.Players
                    .Where(p =>
                        p.DiscordId == (long)discordId &&
                        p.LinkType == "primary")
                    .ToList();

                foreach (var existingPlayer in existingPrimary)
                {
                    existingPlayer.LinkType = "tertiary";
                }
            }

            // Link the account
            player.DiscordId = (long)discordId;
            player.CustomType = accountType;
            player.LinkType = linkType;

            await _db.SaveChangesAsync();

            var summary =
                $"**{player.Username}** (*{player.CustomType}*)";

            return (
                true,
                summary,
                1
            );
        }
    }
}