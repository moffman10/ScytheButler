namespace Dashboard.Web.Components.Pages.PlayerDetails
{
    using Dashboard.Web.Models.PlayerDetails;
    using Dashboard.Web.Helpers;
    using Microsoft.AspNetCore.Components;
    using System.Collections.Generic;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using System.Linq;
    using Microsoft.JSInterop;

    public partial class PlayerDetails : ComponentBase
    {
        [Parameter]
        public int PlayerId { get; set; }

        [Inject]
        private HttpClient Http { get; set; } = default!;

        [Inject]
        private NavigationManager Navigation { get; set; } = default!;
        [Inject]
        protected IJSRuntime JS { get; set; } = default!;

        private WomPlayerDto? player;
        private List<WomPlayerDto>? linkedAccounts;
        private bool showLinkedAccounts = false;
        private string? playerRole;
        private string? discordUsername;
        private string? errorMessage;
        private bool isLoading = true;
        private string? statusMessage;
        private bool isSavingNotes = false;

        // Events editing state variables
        private bool isEditingEvents = false;
        private bool isSavingEvents = false;
        private int tempEvents;
        private string? eventsStatusMessage;

        // --- Achievement Toggle State Variables ---
        private Dictionary<string, bool> achievementStates = new(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, DateTime?> achievementUnlockDates = new();
        private bool isTogglingAchievement = false;

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
                    discordUsername = result.Player.DiscordUsername;
                    linkedAccounts = result.LinkedAccounts;

                    // --- Populate achievement states from the API response ---
                    if (result.Achievements != null)
                    {
                        achievementStates = result.Achievements;
                    }
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
            isEditingEvents = !isEditingEvents;
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

        private async Task SaveAllChangesAsync()
        {
            await SaveNotesAsync();
        }

        // --- Achievement Toggle Methods ---
        protected bool IsAchievementUnlocked(string key) =>
            achievementStates.TryGetValue(key, out var unlocked) && unlocked;

        private int GetTotalAchievementsEarnedCount()
        {
            if (achievementStates == null) return 0;
            return achievementStates.Values.Count(unlocked => unlocked);
        }

        private async Task HandleAchievementToggle(string achievementKey, ChangeEventArgs e)
        {
            bool isChecked = e.Value is bool val && val;

            // Keep track of everything we're changing so we can roll back
            // if any server request fails.
            var previousStates = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

            try
            {
                isTogglingAchievement = true;
                errorMessage = null;
                statusMessage = null;

                // Get the exclusive group this achievement belongs to.
                var exclusiveGroup = ExclusivegroupHelper.GetExclusiveGroup(achievementKey).ToList();

                // Save the current states for rollback.
                foreach (var key in exclusiveGroup)
                {
                    previousStates[key] = achievementStates.TryGetValue(key, out var state)
                        ? state
                        : false;
                }

                // Update the clicked achievement.
                achievementStates[achievementKey] = isChecked;

                // If activating an achievement, deactivate all other
                // achievements in the same exclusive group.
                if (isChecked)
                {
                    foreach (var key in exclusiveGroup)
                    {
                        if (!string.Equals(key, achievementKey, StringComparison.OrdinalIgnoreCase))
                        {
                            achievementStates[key] = false;
                        }
                    }
                }

                // Send the clicked achievement to the server.
                var payload = new
                {
                    AchievementKey = achievementKey,
                    IsUnlocked = isChecked
                };

                var response = await Http.PostAsJsonAsync(
                    $"api/player/{PlayerId}/toggle-achievement",
                    payload);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Failed to update achievement on the server.");
                }

                var result = await response.Content
                    .ReadFromJsonAsync<AchievementToggleResponseDto>();

                if (result != null)
                {
                    achievementUnlockDates[achievementKey] = result.UnlockedAt;
                }

                // If we're activating an exclusive achievement,
                // also tell the server to remove the other achievements.
                if (isChecked)
                {
                    foreach (var key in exclusiveGroup)
                    {
                        if (string.Equals(key, achievementKey, StringComparison.OrdinalIgnoreCase))
                            continue;

                        // Only send requests for achievements that were
                        // actually unlocked.
                        if (previousStates.TryGetValue(key, out var wasUnlocked) &&
                            wasUnlocked)
                        {
                            var removePayload = new
                            {
                                AchievementKey = key,
                                IsUnlocked = false
                            };

                            var removeResponse = await Http.PostAsJsonAsync(
                                $"api/player/{PlayerId}/toggle-achievement",
                                removePayload);

                            if (!removeResponse.IsSuccessStatusCode)
                            {
                                throw new Exception(
                                    $"Failed to remove previous achievement '{key}'.");
                            }

                            achievementUnlockDates[key] = null;
                        }
                    }
                }

                statusMessage = "Achievement updated successfully!";
            }
            catch (Exception ex)
            {
                // Roll everything back locally if anything failed.
                foreach (var state in previousStates)
                {
                    achievementStates[state.Key] = state.Value;
                }

                errorMessage = $"Error toggling achievement: {ex.Message}";
            }
            finally
            {
                isTogglingAchievement = false;
            }
        }

        private async Task CopyDiscordId(long? discordId)
        {
            if (discordId == null)
                return;

            await JS.InvokeVoidAsync(
                "navigator.clipboard.writeText",
                discordId.Value.ToString());
        }

        private void ToggleLinkedAccounts()
        {
            showLinkedAccounts = !showLinkedAccounts;
        }
    }
}