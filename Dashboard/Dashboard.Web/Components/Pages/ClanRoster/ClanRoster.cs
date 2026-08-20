namespace Dashboard.Web.Components.Pages.ClanRoster
{
    using Dashboard.Web.Models.ClanRoster;
    using Microsoft.AspNetCore.Components;
    using System.Net.Http.Json;

    public partial class ClanRoster
    {
        [Inject]
        private HttpClient Http { get; set; } = default!;

        private WomGroupDto? clanData;
        private string? errorMessage;

        private SortColumn CurrentSortColumn = SortColumn.Name;
        private bool SortAscending = true;

        private static readonly Dictionary<string, string> RankNames =
            new(StringComparer.OrdinalIgnoreCase)
            {
            { "recruit", "1 Banana" },
            { "corporal", "2 Banana" },
            { "sergeant", "3 Banana" },
            { "general", "1 Star" },
            { "officer", "2 Star" },
            { "commander", "3 Star" },
            { "colonel", "1 Gem" },
            { "brigadier", "2 Gem" },
            { "admiral", "3 Gem" },
            { "xerician", "Raid coach" },
            { "illusionist", "Event staff" },
            { "administrator", "Bronze Key" },
            { "deputy_owner", "Silver Key" },
            { "owner", "Gold Key" }
            };

        protected override async Task OnInitializedAsync()
        {
            try
            {
                clanData = await Http.GetFromJsonAsync<WomGroupDto>("api/clan");
            }
            catch (Exception ex)
            {
                errorMessage = $"Failed to fetch clan roster: {ex.Message}";
            }
        }

        private string CalculateTimeInClan(DateTime joinedAt)
        {
            if (joinedAt == DateTime.MinValue)
                return "N/A";

            var span = DateTime.UtcNow - joinedAt;

            int years = span.Days / 365;
            int months = (span.Days % 365) / 30;
            int days = (span.Days % 365) % 30;

            if (years > 0)
                return $"{years}y {months}m {days}d";

            if (months > 0)
                return $"{months}m {days}d";

            return $"{days} days";
        }

        private string FormatRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role))
                return "1 Banana";

            return RankNames.TryGetValue(role, out var customRank)
                ? customRank
                : role;
        }

        private void SortBy(SortColumn column)
        {
            if (CurrentSortColumn == column)
            {
                SortAscending = !SortAscending;
            }
            else
            {
                CurrentSortColumn = column;
                SortAscending = column switch
                {
                    SortColumn.Name => true,
                    SortColumn.Rank => false,
                    SortColumn.JoinedDate => true,
                    SortColumn.TimeInClan => true,
                    _ => true
                };
            }
        }

        private IEnumerable<WomMembershipDto> GetSortedMembers()
        {
            if (clanData == null)
                return Enumerable.Empty<WomMembershipDto>();

            IEnumerable<WomMembershipDto> members = clanData.Memberships;

            return CurrentSortColumn switch
            {
                SortColumn.Name => SortAscending
                    ? members.OrderBy(m => m.Player.DisplayName)
                    : members.OrderByDescending(m => m.Player.DisplayName),

                SortColumn.Rank => SortAscending
                    ? members.OrderBy(m => GetRankOrder(m.Role))
                    : members.OrderByDescending(m => GetRankOrder(m.Role)),

                SortColumn.JoinedDate => SortAscending
                    ? members.OrderBy(m => m.CreatedAt)
                    : members.OrderByDescending(m => m.CreatedAt),

                SortColumn.TimeInClan => SortAscending
                    ? members.OrderBy(m => GetTimeInClan(m.CreatedAt))
                    : members.OrderByDescending(m => GetTimeInClan(m.CreatedAt)),

                _ => members
            };
        }

        private int GetRankOrder(string role)
        {
            return role?.ToLowerInvariant() switch
            {
                "bob" => 1,
                "recruit" => 11,
                "corporal" => 12,
                "sergeant" => 13,
                "general" => 21,
                "officer" => 22,
                "commander" => 23,
                "colonel" => 31,
                "brigadier" => 32,
                "admiral" => 33,
                "xerician" => 41,
                "illusionist" => 42,
                "administrator" => 51,
                "deputy_owner" => 52,
                "owner" => 53,
                _ => 0
            };
        }

        private TimeSpan GetTimeInClan(DateTime joinedAt)
        {
            if (joinedAt == DateTime.MinValue)
                return TimeSpan.Zero;

            return DateTime.UtcNow - joinedAt;
        }

        private MarkupString SortIndicator(SortColumn column)
        {
            if (CurrentSortColumn != column)
                return new MarkupString("");

            return SortAscending
                ? new MarkupString("<span class=\"sort-indicator\">↑</span>")
                : new MarkupString("<span class=\"sort-indicator\">↓</span>");
        }
    }
}
