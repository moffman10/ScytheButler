using Dashboard.Web.Models.ClanCoffer;

namespace Dashboard.ApiService.Models
{ 
    public class CofferState
    {
        public int CurrentPage { get; set; } = 1;
        public string SearchTerm { get; set; } = string.Empty;
        public CofferSortColumn SortColumn { get; set; } = CofferSortColumn.Date;
        public bool SortAscending { get; set; } = false;
    }
}
