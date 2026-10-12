namespace Dashboard.ApiService.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using ScytheButler.Data;
    using Dashboard.Web.Models.ClanCoffer;

    [ApiController]
    [Route("api/[controller]")]
    public class CofferController : ControllerBase
    {
        private readonly AppDbContext _db;

        public CofferController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<CofferResponseDto>> GetCofferData([FromQuery] bool forceSync = false)
        {
            try
            {
                // Fetch transactions from the database
                var dbTransactions = await _db.Transactions
                    .OrderByDescending(t => t.Date)
                    .ToListAsync();

                // Map to frontend DTOs
                var transactionDtos = dbTransactions.Select(t => new TransactionDto
                {
                    Id = t.Id,
                    Username = t.Username ?? string.Empty,
                    Amount = t.Amount,
                    Type = t.Type ?? string.Empty,
                    Reason = t.Reason ?? string.Empty,
                    Bank = t.Bank ?? string.Empty,
                    Date = t.Date
                }).ToList();

                // Calculate balances and totals
                var balances = await _db.Balances.ToListAsync();
                double totalValue = balances.Sum(b => b.Value);

                double totalDeposits = dbTransactions
                    .Where(t => t.Type != null && t.Type.Equals("Deposit", StringComparison.OrdinalIgnoreCase))
                    .Sum(t => t.Amount);

                double totalWithdrawals = dbTransactions
                    .Where(t => t.Type != null && t.Type.Equals("Withdrawal", StringComparison.OrdinalIgnoreCase))
                    .Sum(t => t.Amount);

                var cofferData = new ClanCofferDto
                {
                    TotalValue = totalValue,
                    TotalDeposits = totalDeposits,
                    TotalWithdrawals = totalWithdrawals,
                    Transactions = transactionDtos
                };

                var response = new CofferResponseDto
                {
                    CofferData = cofferData,
                    LastSyncedAt = DateTime.UtcNow
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to retrieve coffer data: {ex.Message}");
            }
        }
    }
}