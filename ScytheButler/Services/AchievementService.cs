using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using ScytheButler.Models;

namespace ScytheButler.Services
{
    public class DeploymentItem
    {
        public bool IsHeader { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string RoleId { get; set; } = string.Empty;
        public int Color { get; set; } = 3447003; // Default to Blue
    }

    public class AchievementService
    {
        private readonly IConfiguration _configuration;

        public static readonly List<DeploymentItem> DeploymentSequence = new()
        {
            // --- TOP BANNER / INTRO ---
            new DeploymentItem
            {
                IsHeader = true,
                Title = "👑 ScytheBound Legends",
                Description = "A collection of some of the most prestigious achievements earned by our members.\n" +
                            "From conquering Gelinor's toughest challenges to completing some of its rarest collections, these are the members who have gone above and beyond.\n\n" +
                            "**Think you belong here?**\n" +
                            "Send proof of your achievement to a member of Staff and we'll add your name to the ScytheBound Legends and award the corresponding role.\n\n" +
                            "*Achievement lists are updated regularly, so if you spot a missing name or a recent achievement, let us know!*",
                FileName = "",
                Color = 3447003
            },

            // --- MEGA-RARE OWNERS ---
            new DeploymentItem
            {
                IsHeader = true,
                Title = "💎 Mega-Rare Owners",
                Description = "The rarest weapons in Gielinor are reserved for those who have conquered some of its greatest challenges. These members have earned one or more of the game's most coveted weapons.",
                Color = 3447003
            },
            new DeploymentItem { Key = "tbow", Title = "Twisted Bow", FileName = "m-twisted-one.png", RoleId = "1527934465014235207", Color = 65280 },
            new DeploymentItem { Key = "scythe", Title = "Scythe of Vitur", FileName = "m-blood-tax-collector.png", RoleId = "1408915346907201566", Color = 16711680 },
            new DeploymentItem { Key = "shadow", Title = "Tumeken's Shadow", FileName = "m-shadow-wielder.png", RoleId = "1527934718912368720", Color = 16776960 },

            // --- PVM ACHIEVEMENTS ---
            new DeploymentItem
            {
                IsHeader = true,
                Title = "⚔️ PvM Achievements",
                Description = "The greatest PvM accomplishments demand exceptional skill, perseverance, and mastery of Gielinor's toughest encounters. These members have proven themselves through Combat Achievements and some of the game's most prestigious cosmetic rewards.",
                Color = 3447003
            },
            new DeploymentItem { Key = "grandmaster_ca", Title = "Grandmaster CA's", FileName = "p-grandmaster.png", RoleId = "1516760672870989905", Color = 7699941 },
            new DeploymentItem { Key = "master_ca", Title = "Master CA's", FileName = "p-master.png", RoleId = "1516760617233682462", Color = 6295817 },
            new DeploymentItem { Key = "elite_ca", Title = "Elite CA's", FileName = "p-elite.png", RoleId = "1516760401713565808", Color = 4348002 },
            new DeploymentItem { Key = "blood_torva", Title = "Blood Torva", FileName = "p-blood-torva.png", RoleId = "1516759543705501798", Color = 7346189 },
            new DeploymentItem { Key = "radiant_oathplate", Title = "Radiant Oathplate", FileName = "p-radiant-oathplate.png", RoleId = "1516759435475812352", Color = 14805228 },

            // --- ACHIEVEMENT CAPES ---
            new DeploymentItem
            {
                IsHeader = true,
                Title = "👑 Achievement Capes",
                Description = "Achievement capes represent some of the greatest milestones an account can reach. Whether earned through questing, exploration, combat, or countless hours of dedication, these capes symbolize commitment across every corner of Gielinor.",
                Color = 3447003
            },
            new DeploymentItem { Key = "max_cape", Title = "MAX Cape", FileName = "a-max.png", RoleId = "1516759197562441850", Color = 9253929 },
            new DeploymentItem { Key = "infernal_cape", Title = "Infernal Cape", FileName = "a-infernal.png", RoleId = "1516757582105018479", Color = 16735744 },
            new DeploymentItem { Key = "quiver", Title = "Dizana's Quiver", FileName = "a-quiver.png", RoleId = "1527930235977207818", Color = 16500480 },
            new DeploymentItem { Key = "fire_cape", Title = "Fire Cape", FileName = "a-fire.png", RoleId = "1516758699530653807", Color = 16751872 },
            new DeploymentItem { Key = "quest_cape", Title = "Quest Cape", FileName = "a-quest.png", RoleId = "1516758254229786664", Color = 1745373 },
            new DeploymentItem { Key = "diary_cape", Title = "Diary Cape", FileName = "a-diary.png", RoleId = "1516758799950675988", Color = 1154840 },
            new DeploymentItem { Key = "music_cape", Title = "Music Cape", FileName = "a-music.png", RoleId = "1516758981064921148", Color = 15844367 },
            new DeploymentItem { Key = "champion_cape", Title = "Champion's Cape", FileName = "a-champion.png", RoleId = "1516759686479740992", Color = 1378646 },

            // --- COLLECTION LOG MILESTONES ---
            new DeploymentItem
            {
                IsHeader = true,
                Title = "📚 Collection Log Milestones",
                Description = "The Collection Log celebrates dedication across every corner of Gielinor. From rare drops and boss unlocks to clue rewards and minigame treasures, these milestones reflect countless hours of persistence and completionism.",
                Color = 3447003
            },
            new DeploymentItem { Key = "gilded_clogger", Title = "Clogger Gilded", FileName = "c-gilded.png", RoleId = "1516761833048899696", Color = 12948504 },
            new DeploymentItem { Key = "dragon_clogger", Title = "Clogger Dragon", FileName = "c-dragon.png", RoleId = "1516761753290014751", Color = 6295817 },
            new DeploymentItem { Key = "rune_clogger", Title = "Clogger VII", FileName = "c-rune.png", RoleId = "1516761472393285716", Color = 4348002 },
            new DeploymentItem { Key = "adamant_clogger", Title = "Clogger VI", FileName = "c-adamant.png", RoleId = "1516761371801419866", Color = 4347202 },
            new DeploymentItem { Key = "mithril_clogger", Title = "Clogger V", FileName = "c-mithril.png", RoleId = "1516761270999715892", Color = 4210782 },
            new DeploymentItem { Key = "black_clogger", Title = "Clogger IV", FileName = "c-black.png", RoleId = "1516760925863022633", Color = 1 }
        };

        public AchievementService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<Dictionary<string, string>> DeployPanelsToChannelAsync(string channelId)
        {
            string? botToken = _configuration["DiscordToken"] ?? _configuration["Discord:Token"];
            if (string.IsNullOrEmpty(botToken)) return new Dictionary<string, string>();

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "ScytheButler (Clan Dashboard, v1.0)");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", botToken);

            string url = $"https://discord.com/api/v10/channels/{channelId}/messages";
            var deployedMessages = new Dictionary<string, string>();

            foreach (var item in DeploymentSequence)
            {
                string localFilePath = string.Empty;
                if (!string.IsNullOrEmpty(item.FileName))
                {
                    string candidate = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "scythebound-legends", item.FileName);
                    if (File.Exists(candidate))
                    {
                        localFilePath = candidate;
                    }
                    else
                    {
                        string? currentDir = AppContext.BaseDirectory;
                        while (!string.IsNullOrEmpty(currentDir))
                        {
                            candidate = Path.Combine(currentDir, "Assets", "scythebound-legends", item.FileName);
                            if (File.Exists(candidate))
                            {
                                localFilePath = candidate;
                                break;
                            }
                            currentDir = Directory.GetParent(currentDir)?.FullName;
                        }
                    }
                }

                if (item.IsHeader)
                {
                    using var form = new MultipartFormDataContent();
                    var embed = new
                    {
                        title = item.Title,
                        description = item.Description,
                        color = item.Color,
                        image = !string.IsNullOrEmpty(item.FileName) ? new { url = $"attachment://{item.FileName}" } : null
                    };

                    form.Add(new StringContent(JsonSerializer.Serialize(new { embeds = new[] { embed } }), System.Text.Encoding.UTF8, "application/json"), "payload_json");

                    if (!string.IsNullOrEmpty(localFilePath) && File.Exists(localFilePath))
                    {
                        var fileBytes = await File.ReadAllBytesAsync(localFilePath);
                        var fileContent = new ByteArrayContent(fileBytes);
                        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                        form.Add(fileContent, "files[0]", item.FileName);
                    }

                    await httpClient.PostAsync(url, form);
                }
                else
                {
                    // 1. Send the Image message first (sits on top)
                    if (!string.IsNullOrEmpty(localFilePath) && File.Exists(localFilePath))
                    {
                        using var imageForm = new MultipartFormDataContent();
                        var imageEmbed = new
                        {
                            color = item.Color,
                            image = new { url = $"attachment://{item.FileName}" }
                        };

                        imageForm.Add(new StringContent(JsonSerializer.Serialize(new { embeds = new[] { imageEmbed } }), System.Text.Encoding.UTF8, "application/json"), "payload_json");

                        var fileBytes = await File.ReadAllBytesAsync(localFilePath);
                        var fileContent = new ByteArrayContent(fileBytes);
                        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                        imageForm.Add(fileContent, "files[0]", item.FileName);

                        await httpClient.PostAsync(url, imageForm);
                        await Task.Delay(500);
                    }

                    // 2. Send the Text/Achievers list message second (targeted for updates)
                    string textContent = $"**{item.Title}**\n" +
                                         (!string.IsNullOrEmpty(item.RoleId) ? $"<@&{item.RoleId}>\n" : "") +
                                         $"**Achievers:**\n" +
                                         $"- Think you belong here? Send in your name and proof if you're eligible";

                    var textEmbed = new
                    {
                        description = textContent,
                        color = item.Color
                    };

                    using var textForm = new MultipartFormDataContent();
                    textForm.Add(new StringContent(JsonSerializer.Serialize(new { embeds = new[] { textEmbed } }), System.Text.Encoding.UTF8, "application/json"), "payload_json");

                    var textResponse = await httpClient.PostAsync(url, textForm);
                    if (textResponse.IsSuccessStatusCode)
                    {
                        var responseData = await textResponse.Content.ReadFromJsonAsync<JsonElement>();
                        string messageId = responseData.GetProperty("id").GetString()!;
                        deployedMessages[item.Key] = messageId;
                    }
                }

                await Task.Delay(500);
            }

            return deployedMessages;
        }

        public async Task<bool> SyncAchievementMessageContentAsync(string channelId, string itemKey, string playerName, bool isUnlocked)
        {
            string? botToken = _configuration["DiscordToken"] ?? _configuration["Discord:Token"];
            if (string.IsNullOrEmpty(botToken)) return false;

            string? messageId = _configuration[$"AchievementMessages:{itemKey}"] ?? _configuration[itemKey];
            if (string.IsNullOrEmpty(messageId)) return false;

            var item = DeploymentSequence.FirstOrDefault(x => x.Key.Equals(itemKey, StringComparison.OrdinalIgnoreCase));
            if (item == null) return false;

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "ScytheButler (Clan Dashboard, v1.0)");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", botToken);

            string url = $"https://discord.com/api/v10/channels/{channelId}/messages/{messageId}";

            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return false;

            var messageData = await response.Content.ReadFromJsonAsync<JsonElement>();
            var embeds = messageData.GetProperty("embeds");
            if (embeds.GetArrayLength() == 0) return false;

            string currentDescription = embeds[0].GetProperty("description").GetString() ?? string.Empty;
            string updatedDescription = UpdateAchieversList(currentDescription, playerName, isUnlocked);

            object embed = new
            {
                description = updatedDescription,
                color = item.Color
            };

            var payload = new { embeds = new[] { embed } };
            var patchResponse = await httpClient.PatchAsJsonAsync(url, payload);

            return patchResponse.IsSuccessStatusCode;
        }

        public string UpdateAchieversList(string content, string playerName, bool isUnlocked)
        {
            var lines = content.Split('\n').ToList();
            int achieversIndex = lines.FindIndex(l => l.Trim().Equals("Achievers:", StringComparison.OrdinalIgnoreCase));

            if (achieversIndex == -1) return content;

            int listEndIndex = achieversIndex + 1;
            while (listEndIndex < lines.Count && lines[listEndIndex].StartsWith("- "))
            {
                listEndIndex++;
            }

            var achieverLines = lines.GetRange(achieversIndex + 1, listEndIndex - (achieversIndex + 1));
            bool exists = achieverLines.Any(l => l.Contains(playerName, StringComparison.OrdinalIgnoreCase));

            lines.RemoveAll(l => l.Contains("Think you belong here?", StringComparison.OrdinalIgnoreCase));

            if (isUnlocked && !exists)
            {
                lines.Insert(achieversIndex + 1, $"- {playerName}");
            }
            else if (!isUnlocked && exists)
            {
                lines.RemoveAll(l => l.Trim().Equals($"- {playerName}", StringComparison.OrdinalIgnoreCase));
            }

            return string.Join("\n", lines);
        }
    }
}