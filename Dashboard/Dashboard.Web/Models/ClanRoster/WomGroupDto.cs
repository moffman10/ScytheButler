using static Dashboard.Web.Components.Pages.ClanRoster.ClanRoster;

namespace Dashboard.Web.Models.ClanRoster
{
    public class WomGroupDto
    {
        public string Name { get; set; } = string.Empty;
        public List<WomMembershipDto> Memberships { get; set; } = new();
    }
}
