namespace Dashboard.Web.Models.ClanCoffer
{
    public class BankBalanceDto
    {
        public string BankName { get; set; } = string.Empty;
        public double Balance { get; set; }
        public double PercentageOfCoffer { get; set; }
    }
}
