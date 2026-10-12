namespace Dashboard.Web.Models.ClanCoffer
{
    public class CofferResponseDto
    {
        public ClanCofferDto CofferData { get; set; } = default!;
        public DateTime? LastSyncedAt { get; set; }
    }
}
