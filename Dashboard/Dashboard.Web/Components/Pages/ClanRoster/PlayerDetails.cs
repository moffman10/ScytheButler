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

        // Events editing state variables
        private bool isEditingEvents = false;
        private bool isSavingEvents = false;
        private int tempEvents;
        private string? eventsStatusMessage;

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

        // --- Events Editing Methods ---
        private void ToggleEditEvents()
        {
            if (player != null)
            {
                tempEvents = player.Events;
            }
            isEditingEvents = true;
            eventsStatusMessage = null;
        }

        private void CancelEditEvents()
        {
            isEditingEvents = false;
            eventsStatusMessage = null;
        }

        private async Task SaveEventsAsync()
        {
            try
            {
                isSavingEvents = true;
                eventsStatusMessage = null;
                errorMessage = null;

                var payload = new { Events = tempEvents };
                var response = await Http.PostAsJsonAsync($"api/player/{PlayerId}/events", payload);

                if (response.IsSuccessStatusCode)
                {
                    if (player != null)
                    {
                        player.Events = tempEvents;
                    }
                    isEditingEvents = false;
                    eventsStatusMessage = "Events updated successfully!";
                }
                else
                {
                    errorMessage = "Failed to save events.";
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Error saving events: {ex.Message}";
            }
            finally
            {
                isSavingEvents = false;
            }
        }
    }
}