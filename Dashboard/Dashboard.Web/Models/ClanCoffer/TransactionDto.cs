namespace Dashboard.Web.Models.ClanCoffer
{
    public class TransactionDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Bank { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Event { get; set; } = string.Empty;
        public double Amount { get; set; }
        public DateTime Date { get; set; }
    }
}
