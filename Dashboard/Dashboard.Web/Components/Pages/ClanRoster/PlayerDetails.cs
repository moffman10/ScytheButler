namespace Dashboard.Web.Components.Pages.PlayerDetails
{
    using Dashboard.Web.Models.PlayerDetails;
    using Dashboard.Web.Helpers; 
    using Microsoft.AspNetCore.Components;
    using System.Net.Http.Json;

    public partial class PlayerDetails : ComponentBase
    {
        [Parameter]
        public int PlayerId { get; set; }

        [Inject]
        private HttpClient Http { get; set; } = default!;

        [Inject]
        private NavigationManager Navigation { get; set; } = default!;

        private WomPlayerDto? player;
        private string? playerRole;
        private string? errorMessage;
        private bool isLoading = true;
        private string? statusMessage;
        private bool isSavingNotes = false;
        protected string FormatRole(string role) => RankHelper.FormatRole(role);
        protected string CalculateTimeInClan(DateTime joinedAt) => CalculateTimeInClanHelper.CalculateTimeInClan(joinedAt);

        protected override async Task OnInitializedAsync()
        {
            await LoadPlayerDetailsAsync();
        }

        private async Task LoadPlayerDetailsAsync()
        {
            try
            {
                isLoading = true;
                errorMessage = null;

                var result = await Http.GetFromJsonAsync<PlayerDetailResponseDto>($"api/player/{PlayerId}");
                if (result != null)
                {
                    player = result.Player;
                    playerRole = result.Role;
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Failed to load player details: {ex.Message}";
            }
            finally
            {
                isLoading = false;
            }
        }

        private void GoBack()
        {
            Navigation.NavigateTo("/clan-roster");
        }

        private async Task SaveNotesAsync()
        {
            try
            {
                isSavingNotes = true;
                statusMessage = null;

                var payload = new { Notes = player?.Notes };
                var response = await Http.PostAsJsonAsync($"api/player/{PlayerId}/notes", payload);

                if (response.IsSuccessStatusCode)
                {
                    statusMessage = "Notes saved successfully!";
                }
                else
                {
                    errorMessage = "Failed to save notes.";
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Error saving notes: {ex.Message}";
            }
            finally
            {
                isSavingNotes = false;
            }
        }
    }

    
}