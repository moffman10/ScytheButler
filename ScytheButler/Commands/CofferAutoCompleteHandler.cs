using Discord;
using Discord.Interactions;
using ScytheButler.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace ScytheButler.Commands
{
    public class CofferAutoCompleteHandler : AutocompleteHandler
    {
        private readonly CofferService _cofferService;

        public CofferAutoCompleteHandler(CofferService cofferService)
        {
            _cofferService = cofferService;
        }

        public override Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            string current = autocompleteInteraction.Data.Current.Value?.ToString() ?? "";

            var suggestions = _cofferService.GetAllCoffers().Where(u => u.StartsWith(current, StringComparison.OrdinalIgnoreCase)).Take(25).Select(u => new AutocompleteResult(u, u)).ToList();

            return Task.FromResult(AutocompletionResult.FromSuccess(suggestions));
        }
    }
}
