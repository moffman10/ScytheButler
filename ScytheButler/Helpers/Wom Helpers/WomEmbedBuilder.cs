using Discord;
using ScytheButler.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ScytheButler.Helpers.Wom_Helpers
{
    public static class WomEmbedBuilder
    {
        public static Embed BuildCompetitionEmbedAsync(CompetitionResult result)
        {
            var embed = new EmbedBuilder();

            if (!result.Success)
            {
                embed.Title = "❌ Competition Creation Failed";
                embed.Description = result.Error;
                embed.Color = Color.Red;
                return embed.Build();
            }

            embed.Title = $"✅ {result.Title}";
            embed.Color = Color.Green;

            embed.AddField("Metric", result.Metric, true);
            embed.AddField("Start", result.Start.ToString("yyyy-MM-dd"), true);
            embed.AddField("End", result.End.ToString("yyyy-MM-dd"), true);

            if (result.Participants != null)
                embed.AddField("Participants", string.Join("\n", result.Participants), false);

            if (result.Teams != null)
            {
                embed.AddField(
                    "Teams",
                    string.Join("\n", result.Teams.Select(t =>
                        $"{t.name}: {string.Join(", ", t.participants)}")),
                    false);
            }

            return embed.Build();
        }
    }
}

