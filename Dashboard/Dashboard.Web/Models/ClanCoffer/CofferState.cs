namespace Dashboard.Web.Models.ClanCoffer
{
    public class CofferState
    {
        public int CurrentPage { get; set; } = 1;
        public string SearchTerm { get; set; } = string.Empty;
        public string SelectedEvent { get; set; } = "All Events";
        public string SelectedPerson { get; set; } = "All People";
        public Dashboard.Web.Models.ClanCoffer.CofferSortColumn SortColumn { get; set; } = Dashboard.Web.Models.ClanCoffer.CofferSortColumn.Date;
        public bool SortAscending { get; set; } = false;
    }
}
