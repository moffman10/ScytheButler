using Dashboard.Web.Models.ClanRoster;
using ScytheButler.data.DatabaseModels.WomModels;

namespace Dashboard.ApiService.Helpers
{
    public static class ClanMappingExtensions
    {
        public static WomGroupDto MapToGroupDto(this WomGroup group)
        {
            return new WomGroupDto
            {
                Id = group.Id,
                Name = group.Name,
                LastSyncedAt = group.LastSyncedAt,
                Memberships = group.Memberships?.Select(m =>
                {
                    DateTime playerCreatedAt = DateTime.MinValue;
                    if (m.Player != null)
                    {
                        if (m.Player.CreatedAt != DateTime.MinValue)
                        {
                            playerCreatedAt = m.Player.CreatedAt;
                        }
                        else if (m.Player.RegisteredAt.HasValue)
                        {
                            playerCreatedAt = m.Player.RegisteredAt.Value;
                        }
                    }

                    DateTime membershipCreatedAt = (m.CreatedAt != DateTime.MinValue)
                        ? m.CreatedAt
                        : playerCreatedAt;

                    return new WomMembershipDto
                    {
                        PlayerId = m.PlayerId,
                        Role = m.Role,
                        CreatedAt = membershipCreatedAt,
                        Player = m.Player != null ? new WomPlayerDto
                        {
                            Id = m.Player.Id,
                            Username = m.Player.Username,
                            DisplayName = m.Player.DisplayName,
                            Type = m.Player.Type,
                            Build = m.Player.Build,
                            Country = m.Player.Country,
                            Status = m.Player.Status,
                            Patron = m.Player.Patron,
                            Exp = m.Player.Exp,
                            Ehp = m.Player.Ehp,
                            Ehb = m.Player.Ehb,
                            Events = m.Player.Events,
                            DiscordId = m.Player.DiscordId,
                            Referals = m.Player.Referals,
                            RegisteredAt = m.Player.RegisteredAt,
                            UpdatedAt = m.Player.UpdatedAt,
                            CreatedAt = playerCreatedAt
                        } : null!
                    };
                }).ToList() ?? new List<WomMembershipDto>()
            };
        }
    }
}