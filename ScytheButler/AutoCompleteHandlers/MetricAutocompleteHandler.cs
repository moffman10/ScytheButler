using Discord;
using Discord.Interactions;
using ScytheButler.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ScytheButler.AutoCompleteHandlers
{
    public class MetricAutocompleteHandler : AutocompleteHandler
    {
        public override Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context,IAutocompleteInteraction autocompleteInteraction,IParameterInfo parameter,IServiceProvider services)
        {
            var userInput = autocompleteInteraction.Data.Current.Value?.ToString() ?? "";

            var matches = Enum.GetNames(typeof(Metric)).Where(x => x.StartsWith(userInput,StringComparison.OrdinalIgnoreCase)).Select(x => new AutocompleteResult(x,x)).ToList();

            return Task.FromResult(AutocompletionResult.FromSuccess(matches));
        }
    }
}
