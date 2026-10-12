namespace Dashboard.Web.Models.ClanCoffer
{
    public class ClanCofferDto
    {
        public double TotalValue { get; set; }
        public double TotalDeposits { get; set; }
        public double TotalWithdrawals { get; set; }
        public List<BankBalanceDto> BankBalances { get; set; } = new();
        public List<TransactionDto> Transactions { get; set; } = new();
    }
}
