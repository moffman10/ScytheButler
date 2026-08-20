using static Dashboard.Web.Components.Pages.ClanRoster.ClanRoster;

namespace Dashboard.Web.Models.ClanRoster
{
    public class WomMembershipDto
    {
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public WomPlayerDto Player { get; set; } = new();
    }
}
