namespace Dashboard.Web.Services
{
    public class ClanRosterState
    {
        public int CurrentPage { get; set; } = 1;
        public string SearchTerm { get; set; } = string.Empty;
        public Dashboard.Web.Models.ClanRoster.SortColumn SortColumn { get; set; } = Dashboard.Web.Models.ClanRoster.SortColumn.Name;
        public bool SortAscending { get; set; } = true;
    }
}