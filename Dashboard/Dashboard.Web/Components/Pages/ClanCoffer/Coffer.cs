namespace Dashboard.Web.Components.Pages.Coffer
{
    using Dashboard.Web.Models.ClanCoffer;
    using Dashboard.Web.Services;
    using Microsoft.AspNetCore.Components;
    using System.Net.Http.Json;

    public partial class Coffer
    {
        [Inject]
        private HttpClient Http { get; set; } = default!;

        [Inject]
        private CofferState CofferState { get; set; } = default!;

        private ClanCofferDto? cofferData;
        private string? errorMessage;
        private bool isUpdating = false;
        private DateTime? lastSyncTime;

        private CofferSortColumn CurrentSortColumn = CofferSortColumn.Date;
        private bool SortAscending = false;

        private int currentPage = 1;
        private int pageSize = 20;

        private string selectedEvent = "All Events";
        private string selectedPerson = "All People";
        private string searchField = "All";
        private string searchTerm = string.Empty;

        private int TotalPages => GetFilteredTransactions().Count() == 0 ? 0 : (int)Math.Ceiling((double)TotalFilteredCount / pageSize);
        private int TotalFilteredCount => GetFilteredTransactions().Count();

        protected override async Task OnInitializedAsync()
        {
            currentPage = CofferState.CurrentPage;
            searchTerm = CofferState.SearchTerm;
            selectedEvent = CofferState.SelectedEvent;
            selectedPerson = CofferState.SelectedPerson;
            CurrentSortColumn = CofferState.SortColumn;
            SortAscending = CofferState.SortAscending;

            await LoadDataAsync(forceSync: false);
        }

        private async Task RefreshFromDatabaseAsync() => await LoadDataAsync(forceSync: false);
        private async Task ForceUpdateFromApiAsync() => await LoadDataAsync(forceSync: true);

        private async Task LoadDataAsync(bool forceSync)
        {
            try
            {
                isUpdating = true;
                errorMessage = null;

                var response = await Http.GetFromJsonAsync<CofferResponseDto>(
                    $"api/coffer?forceSync={forceSync}"
                );

                if (response != null)
                {
                    cofferData = response.CofferData;
                    lastSyncTime = response.LastSyncedAt;
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Failed to load coffer data: {ex.Message}";
            }
            finally
            {
                isUpdating = false;
            }
        }

        private IEnumerable<string> GetAvailableEvents()
        {
            if (cofferData?.Transactions == null) return Enumerable.Empty<string>();
            return cofferData.Transactions
                .Select(t => t.Event)
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Distinct()
                .OrderBy(e => e);
        }

        private IEnumerable<string> GetAvailablePeople()
        {
            if (cofferData?.Transactions == null) return Enumerable.Empty<string>();
            return cofferData.Transactions
                .Select(t => t.Username)
                .Where(u => !string.IsNullOrWhiteSpace(u))
                .Distinct()
                .OrderBy(u => u);
        }

        private IEnumerable<TransactionDto> GetFilteredTransactions()
        {
            if (cofferData?.Transactions == null)
                return Enumerable.Empty<TransactionDto>();

            var query = cofferData.Transactions.AsEnumerable();

            // Optional: exclude administrative types if wanted, or leave open
            query = query.Where(t => !new[] { "Rebalance", "Payout", "Clear Coffer" }.Contains(t.Type, StringComparer.OrdinalIgnoreCase));

            if (selectedEvent != "All Events")
            {
                query = query.Where(t => t.Event != null && t.Event.Equals(selectedEvent, StringComparison.OrdinalIgnoreCase));
            }

            if (selectedPerson != "All People")
            {
                query = query.Where(t => t.Username != null && t.Username.Equals(selectedPerson, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = searchField switch
                {
                    "User" => query.Where(t => !string.IsNullOrEmpty(t.Username) && t.Username.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)),
                    "Bank" => query.Where(t => !string.IsNullOrEmpty(t.Bank) && t.Bank.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)),
                    "Type" => query.Where(t => !string.IsNullOrEmpty(t.Type) && t.Type.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)),
                    "Reason" => query.Where(t => !string.IsNullOrEmpty(t.Reason) && t.Reason.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)),
                    _ => query.Where(t =>
                        (!string.IsNullOrEmpty(t.Username) && t.Username.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(t.Reason) && t.Reason.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(t.Bank) && t.Bank.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(t.Type) && t.Type.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)))
                };
            }

            return query;
        }

        private double GetFilteredEventTotal() => GetFilteredTransactions().Sum(t => t.Amount);
        private double GetFilteredPersonTotal() => GetFilteredTransactions().Sum(t => t.Amount);

        private string GetSearchPlaceholder()
        {
            return searchField switch
            {
                "User" => "Search by username...",
                "Bank" => "Search by bank name...",
                "Type" => "Search by transaction type...",
                "Reason" => "Search by reason...",
                _ => "Search by user, bank, type, or reason..."
            };
        }

        private void OnSearchFieldChanged(ChangeEventArgs e)
        {
            searchField = e.Value?.ToString() ?? "All";
            currentPage = 1;
        }

        private void OnEventChanged(ChangeEventArgs e)
        {
            selectedEvent = e.Value?.ToString() ?? "All Events";
            currentPage = 1;
            CofferState.SelectedEvent = selectedEvent;
            CofferState.CurrentPage = currentPage;
        }

        private void OnPersonChanged(ChangeEventArgs e)
        {
            selectedPerson = e.Value?.ToString() ?? "All People";
            currentPage = 1;
            CofferState.SelectedPerson = selectedPerson;
            CofferState.CurrentPage = currentPage;
        }

        private void ClearFilters()
        {
            selectedEvent = "All Events";
            selectedPerson = "All People";
            searchField = "All";
            searchTerm = string.Empty;
            currentPage = 1;
            CofferState.SelectedEvent = selectedEvent;
            CofferState.SelectedPerson = selectedPerson;
            CofferState.SearchTerm = searchTerm;
            CofferState.CurrentPage = currentPage;
        }

        private void GoToPage(int page)
        {
            if (page >= 1 && page <= TotalPages)
            {
                currentPage = page;
                CofferState.CurrentPage = currentPage;
            }
        }

        private void OnSearchInput(ChangeEventArgs e)
        {
            searchTerm = e.Value?.ToString() ?? string.Empty;
            currentPage = 1;
            CofferState.SearchTerm = searchTerm;
            CofferState.CurrentPage = currentPage;
        }

        private int GetStartIndex() => TotalFilteredCount == 0 ? 0 : ((currentPage - 1) * pageSize) + 1;
        private int GetEndIndex() => Math.Min(currentPage * pageSize, TotalFilteredCount);

        private IEnumerable<TransactionDto> GetPagedTransactions()
        {
            return GetSortedTransactions()
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize);
        }

        private void SortBy(CofferSortColumn column)
        {
            currentPage = 1;
            CofferState.CurrentPage = currentPage;

            if (CurrentSortColumn == column)
            {
                SortAscending = !SortAscending;
            }
            else
            {
                CurrentSortColumn = column;
                SortAscending = column switch
                {
                    CofferSortColumn.Username => true,
                    CofferSortColumn.Type => true,
                    CofferSortColumn.Bank => true,
                    CofferSortColumn.Reason => true,
                    CofferSortColumn.Amount => false,
                    CofferSortColumn.Date => false,
                    _ => true
                };
            }

            CofferState.SortColumn = CurrentSortColumn;
            CofferState.SortAscending = SortAscending;
        }

        private IEnumerable<TransactionDto> GetSortedTransactions()
        {
            var transactions = GetFilteredTransactions();

            return CurrentSortColumn switch
            {
                CofferSortColumn.Username => SortAscending ? transactions.OrderBy(t => t.Username) : transactions.OrderByDescending(t => t.Username),
                CofferSortColumn.Type => SortAscending ? transactions.OrderBy(t => t.Type) : transactions.OrderByDescending(t => t.Type),
                CofferSortColumn.Bank => SortAscending ? transactions.OrderBy(t => t.Bank) : transactions.OrderByDescending(t => t.Bank),
                CofferSortColumn.Reason => SortAscending ? transactions.OrderBy(t => t.Reason) : transactions.OrderByDescending(t => t.Reason),
                CofferSortColumn.Amount => SortAscending ? transactions.OrderBy(t => t.Amount) : transactions.OrderByDescending(t => t.Amount),
                CofferSortColumn.Date => SortAscending ? transactions.OrderBy(t => t.Date) : transactions.OrderByDescending(t => t.Date),
                _ => transactions
            };
        }

        private MarkupString SortIndicator(CofferSortColumn column)
        {
            if (CurrentSortColumn != column) return new MarkupString("");
            return SortAscending ? new MarkupString("<span class=\"sort-indicator\">↑</span>") : new MarkupString("<span class=\"sort-indicator\">↓</span>");
        }

        protected string FormatGP(double amount)
        {
            string sign = amount < 0 ? "-" : "+";
            double abs = Math.Abs(amount);
            string formatted = abs >= 1_000_000_000 ? $"{(abs / 1_000_000_000.0):0.##}B" :
                               abs >= 1_000_000 ? $"{(abs / 1_000_000.0):0.##}M" :
                               abs >= 1_000 ? $"{(abs / 1_000.0):0.##}K" : abs.ToString("N0");
            return amount < 0 ? $"-{formatted} GP" : $"{formatted} GP";
        }
    }
}