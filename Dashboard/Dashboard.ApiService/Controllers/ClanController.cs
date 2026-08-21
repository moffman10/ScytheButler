using Dashboard.ApiService.Services;
using Dashboard.Web.Components.Pages.ClanRoster;
using Dashboard.Web.Models.ClanRoster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScytheButler.data.DatabaseModels.WomModels;
using ScytheButler.Data;
using Dashboard.ApiService.Helpers;

namespace Dashboard.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClanController : ControllerBase
{
    private readonly ScytheButler.Data.AppDbContext _db;
    private readonly WomService _womService;

    public ClanController(AppDbContext db, WomService womService)
    {
        _db = db;
        _womService = womService;
    }

    [HttpGet("{groupId}")]
    public async Task<ActionResult<RosterResponseDto>> GetClanRoster(int groupId, [FromQuery] bool forceSync = false)
    {
        // 1. Fetch group with memberships and players from internal database
        var existingGroup = await _db.Groups
            .Include(g => g.Memberships)
                .ThenInclude(m => m.Player)
            .FirstOrDefaultAsync(g => g.Id == groupId);

        // 2. Determine if cache is stale (>15 minutes) or missing
        bool isStale = existingGroup == null ||
                       existingGroup.LastSyncedAt == null ||
                       (DateTime.UtcNow - existingGroup.LastSyncedAt.Value).TotalMinutes >= 15;

        // 3. Upsert if forced or stale
        if (forceSync || isStale)
        {
            var freshData = await _womService.GetGroupDetailsAsync(groupId);
            if (freshData != null)
            {
                await UpsertGroupDataAsync(freshData);

                // Reload from DB to verify saved state
                existingGroup = await _db.Groups
                    .Include(g => g.Memberships)
                        .ThenInclude(m => m.Player)
                    .FirstOrDefaultAsync(g => g.Id == groupId);
            }
        }

        if (existingGroup == null)
        {
            return NotFound($"Clan group with ID {groupId} could not be retrieved.");
        }

        return Ok(new RosterResponseDto
        {
            ClanData = existingGroup.MapToGroupDto(),
            LastSyncedAt = existingGroup.LastSyncedAt ?? DateTime.UtcNow
        });
    }

    private async Task UpsertGroupDataAsync(WomGroup freshGroup)
    {
        var syncTime = DateTime.UtcNow;

        // 1. Upsert Group Entity
        var existingGroup = await _db.Groups.FindAsync(freshGroup.Id);
        if (existingGroup == null)
        {
            existingGroup = new WomGroup
            {
                Id = freshGroup.Id,
                Name = freshGroup.Name,
                ClanChat = freshGroup.ClanChat,
                Description = freshGroup.Description,
                Homeworld = freshGroup.Homeworld,
                Verified = freshGroup.Verified,
                CreatedAt = freshGroup.CreatedAt,
                UpdatedAt = freshGroup.UpdatedAt,
                LastSyncedAt = syncTime
            };
            _db.Groups.Add(existingGroup);
        }
        else
        {
            existingGroup.Name = freshGroup.Name;
            existingGroup.ClanChat = freshGroup.ClanChat;
            existingGroup.Description = freshGroup.Description;
            existingGroup.Homeworld = freshGroup.Homeworld;
            existingGroup.Verified = freshGroup.Verified;
            existingGroup.UpdatedAt = freshGroup.UpdatedAt;
            existingGroup.LastSyncedAt = syncTime;
        }

        // 2. Upsert Players & Memberships cleanly
        if (freshGroup.Memberships != null)
        {
            foreach (var membership in freshGroup.Memberships)
            {
                // Handle Player Entity
                if (membership.Player != null)
                {
                    var existingPlayer = await _db.Players.FindAsync(membership.Player.Id);
                    if (existingPlayer == null)
                    {
                        _db.Players.Add(new WomPlayer
                        {
                            Id = membership.Player.Id,
                            Username = membership.Player.Username,
                            DisplayName = membership.Player.DisplayName
                        });
                    }
                    else
                    {
                        existingPlayer.Username = membership.Player.Username;
                        existingPlayer.DisplayName = membership.Player.DisplayName;
                    }
                }

                // Handle Membership Entity
                var existingMembership = await _db.Memberships
                    .FirstOrDefaultAsync(m => m.PlayerId == membership.PlayerId && m.GroupId == freshGroup.Id);

                if (existingMembership == null)
                {
                    _db.Memberships.Add(new WomMembership
                    {
                        PlayerId = membership.PlayerId,
                        GroupId = freshGroup.Id,
                        Role = membership.Role
                    });
                }
                else
                {
                    existingMembership.Role = membership.Role;
                }
            }
        }

        await _db.SaveChangesAsync();
    }
}