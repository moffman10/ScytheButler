namespace Dashboard.ApiService.Services;

using Microsoft.EntityFrameworkCore;
using ScytheButler.Data;
using System.Net.Http.Headers;
using System.Text.Json;

public class DiscordRoleSyncService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DiscordRoleSyncService> _logger;

    public DiscordRoleSyncService(AppDbContext db, IConfiguration configuration, ILogger<DiscordRoleSyncService> logger)
    {
        _db = db;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SyncGroupRolesAsync(int groupId)
    {
        string? botToken = _configuration["Discord:Token"] ?? _configuration["DiscordToken"];
        string? guildId = _configuration["Discord:GuildId"];

        if (string.IsNullOrEmpty(botToken) || string.IsNullOrEmpty(guildId))
        {
            _logger.LogWarning("Discord Bot Token or Guild ID is missing in configuration. Skipping role sync.");
            return;
        }

        // Load configurations
        var ladderMappings = _configuration.GetSection("Discord:LadderRoles").Get<Dictionary<string, string>>()
            ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var highTierMappings = _configuration.GetSection("Discord:HighTierExclusiveRoles").Get<Dictionary<string, string>>()
            ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var stackableMappings = _configuration.GetSection("Discord:StackableStatusRoles").Get<Dictionary<string, string>>()
            ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var allLadderIds = ladderMappings.Values.ToHashSet();
        var allHighTierIds = highTierMappings.Values.ToHashSet();
        var allStackableIds = stackableMappings.Values.ToHashSet();

        var memberships = await _db.Memberships
            .Include(m => m.Player)
            .Where(m => m.GroupId == groupId && m.Player.DiscordId != null)
            .ToListAsync();

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "ScytheButler (Clan Dashboard, v1.0)");
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", botToken);

        foreach (var membership in memberships)
        {
            long discordId = membership.Player.DiscordId!.Value;
            string currentRoleName = membership.Role?.ToLowerInvariant() ?? string.Empty;

            string? targetRoleId = null;
            bool isLadder = false;
            bool isHighTier = false;
            bool isStackable = false;

            if (ladderMappings.TryGetValue(currentRoleName, out var ladderId))
            {
                targetRoleId = ladderId;
                isLadder = true;
            }
            else if (highTierMappings.TryGetValue(currentRoleName, out var highId))
            {
                targetRoleId = highId;
                isHighTier = true;
            }
            else if (stackableMappings.TryGetValue(currentRoleName, out var stackId))
            {
                targetRoleId = stackId;
                isStackable = true;
            }

            if (string.IsNullOrEmpty(targetRoleId))
            {
                continue;
            }

            try
            {
                string memberUrl = $"https://discord.com/api/v10/guilds/{guildId}/members/{discordId}";
                var memberResponse = await httpClient.GetAsync(memberUrl);

                if (!memberResponse.IsSuccessStatusCode)
                {
                    continue;
                }

                var memberJson = await memberResponse.Content.ReadFromJsonAsync<JsonElement>();
                var userRoles = memberJson.GetProperty("roles").EnumerateArray()
                    .Select(r => r.GetString() ?? string.Empty)
                    .ToHashSet();

                // 1. Add the target role if they don't have it
                if (!userRoles.Contains(targetRoleId))
                {
                    await httpClient.PutAsync($"{memberUrl}/roles/{targetRoleId}", null);
                }

                // 2. If they have a LADDER role: remove old ladder roles AND strip any stackable/coach roles (mutual exclusion)
                if (isLadder)
                {
                    foreach (var oldId in allLadderIds)
                    {
                        if (oldId != targetRoleId && userRoles.Contains(oldId))
                        {
                            await httpClient.DeleteAsync($"{memberUrl}/roles/{oldId}");
                        }
                    }
                    foreach (var stackId in allStackableIds)
                    {
                        if (userRoles.Contains(stackId))
                        {
                            await httpClient.DeleteAsync($"{memberUrl}/roles/{stackId}");
                        }
                    }
                }

                // 3. If they have a STACKABLE/COACH role: add it, but strip ALL ladder roles so they can't hold both simultaneously
                if (isStackable)
                {
                    foreach (var ladderIdToRemove in allLadderIds)
                    {
                        if (userRoles.Contains(ladderIdToRemove))
                        {
                            await httpClient.DeleteAsync($"{memberUrl}/roles/{ladderIdToRemove}");
                        }
                    }
                    foreach (var oldStackId in allStackableIds)
                    {
                        if (oldStackId != targetRoleId && userRoles.Contains(oldStackId))
                        {
                            await httpClient.DeleteAsync($"{memberUrl}/roles/{oldStackId}");
                        }
                    }
                }

                // 4. If they have a High-Tier Exclusive role: clear competing high-tier roles
                if (isHighTier)
                {
                    foreach (var oldHighId in allHighTierIds)
                    {
                        if (oldHighId != targetRoleId && userRoles.Contains(oldHighId))
                        {
                            await httpClient.DeleteAsync($"{memberUrl}/roles/{oldHighId}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing Discord roles for Discord ID {DiscordId}", discordId);
            }
        }

        _logger.LogInformation("Completed Discord role sync for group {GroupId}.", groupId);
    }
}