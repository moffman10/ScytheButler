namespace Dashboard.Web.Components.Pages.ClanRoster
{
    using Dashboard.Web.Models.ClanRoster;
    using Microsoft.AspNetCore.Components;
    using Dashboard.Web.Helpers;
    using System.Net.Http.Json;

    public partial class ClanRoster
    {
        [Inject]
        private HttpClient Http { get; set; } = default!;

        private WomGroupDto? clanData;
        private string? errorMessage;
        private bool isUpdating = false;
        private DateTime? lastSyncTime;

        private const int TargetGroupId = 13022;

        private SortColumn CurrentSortColumn = SortColumn.Name;
        private bool SortAscending = true;

        private int currentPage = 1;
        private int pageSize = 20;

        private int TotalPages => clanData == null ? 0 : (int)Math.Ceiling((double)TotalFilteredCount / pageSize);

        private string searchTerm = string.Empty;

        protected string FormatRole(string role) => RankHelper.FormatRole(role);
        protected string CalculateTimeInClan(DateTime joinedAt) => CalculateTimeInClanHelper.CalculateTimeInClan(joinedAt);
        protected string GetRankBadgeClass(string role) => GetBadgeClassHelper.GetRankBadgeClass(role);

        private IEnumerable<WomMembershipDto> GetFilteredMembers()
        {
            if (clanData == null)
                return Enumerable.Empty<WomMembershipDto>();

            if (string.IsNullOrWhiteSpace(searchTerm))
                return clanData.Memberships;

            return clanData.Memberships.Where(m => m.Player.DisplayName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }
        private int TotalFilteredCount => GetFilteredMembers().Count();

        protected override async Task OnInitializedAsync()
        {
            await LoadDataAsync(forceWomSync: false);
        }

        // Button action: Just pull from local DB
        private async Task RefreshFromDatabaseAsync()
        {
            await LoadDataAsync(forceWomSync: false);
        }

        // Button action: Force WOM API fetch + DB upsert
        private async Task ForceUpdateFromWomAsync()
        {
            await LoadDataAsync(forceWomSync: true);
        }

        private async Task LoadDataAsync(bool forceWomSync)
        {
            try
            {
                isUpdating = true;
                errorMessage = null;

                var response = await Http.GetFromJsonAsync<RosterResponseDto>(
                    $"api/clan/{TargetGroupId}?forceSync={forceWomSync}"
                );

                if (response != null)
                {
                    clanData = response.ClanData;
                    lastSyncTime = response.LastSyncedAt;
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Failed to load roster data: {ex.Message}";
            }
            finally
            {
                isUpdating = false;
            }
        }

        private void GoToPage(int page)
        {
            if (page >= 1 && page <= TotalPages)
            {
                currentPage = page;
            }
        }
        private void OnSearchInput(Microsoft.AspNetCore.Components.ChangeEventArgs e)
        {
            searchTerm = e.Value?.ToString() ?? string.Empty;
            currentPage = 1;
        }

        private int GetStartIndex() => TotalFilteredCount == 0 ? 0 : ((currentPage - 1) * pageSize) + 1;

        private int GetEndIndex() => Math.Min(currentPage * pageSize, TotalFilteredCount);

        private IEnumerable<WomMembershipDto> GetPagedMembers()
        {
            return GetSortedMembers()
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize);
        }

        private void SortBy(SortColumn column)
        {
            currentPage = 1;

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
                    SortColumn.Events => false,
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

            IEnumerable<WomMembershipDto> members = GetFilteredMembers();

            return CurrentSortColumn switch
            {
                SortColumn.Name => SortAscending
                    ? members.OrderBy(m => m.Player.DisplayName)
                    : members.OrderByDescending(m => m.Player.DisplayName),

                SortColumn.Rank => SortAscending
                    ? members.OrderBy(m => GetRankOrder(m.Role))
                    : members.OrderByDescending(m => GetRankOrder(m.Role)),
                SortColumn.Events => SortAscending 
                    ? members.OrderBy(m => m.Player.Events)
                    : members.OrderByDescending(m => m.Player.Events),

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