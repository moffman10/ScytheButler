using Discord;
using Discord.Interactions;
using ScytheButler.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScytheButler.AutoCompleteHandlers
{
    public class DonorAutoCompleteHandler : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
            IInteractionContext context,
            IAutocompleteInteraction autocompleteInteraction,
            IParameterInfo parameter,
            IServiceProvider services)
        {
            var cofferService = (CofferService?)services.GetService(typeof(CofferService));
            if (cofferService == null)
            {
                return AutocompletionResult.FromSuccess();
            }

            string userInput = autocompleteInteraction.Data.Current.Value?.ToString() ?? string.Empty;

            // Retrieve all distinct past donors from DB
            var donors = cofferService.GetAllDonors();

            // Filter by what user typed (case-insensitive) and return top 25 choices (Discord max limit)
            var results = donors
                .Where(d => d.Contains(userInput, StringComparison.OrdinalIgnoreCase))
                .Take(25)
                .Select(d => new AutocompleteResult(d, d))
                .ToList();

            return AutocompletionResult.FromSuccess(results);
        }
    }
}