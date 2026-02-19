using Discord;
using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;
using ScytheButler.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace ScytheButler.Commands
{
    public class ReasonAutocompleteHandler : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
            IInteractionContext context,
            IAutocompleteInteraction interaction,
            IParameterInfo parameter,
            IServiceProvider services)
        {
            var cofferService = services.GetRequiredService<CofferService>();

            var reasons = cofferService.GetAllReasons(); ;

            var results = reasons
                .Where(r => r.StartsWith(interaction.Data.Current.Value?.ToString() ?? "", StringComparison.OrdinalIgnoreCase))
                .Take(25)
                .Select(r => new AutocompleteResult(r, r));

            return AutocompletionResult.FromSuccess(results);
        }
    }
}
