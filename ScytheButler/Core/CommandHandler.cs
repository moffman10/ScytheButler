using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace ScytheButler.Core
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _interactions;
        private readonly IServiceProvider _services;

        public CommandHandler(DiscordSocketClient client, InteractionService interactions, IServiceProvider services)
        {
            _client = client;
            _interactions = interactions;
            _services = services;
        }

        public async Task InitializeAsync()
        {
            _client.InteractionCreated += HandleInteraction;

            // Load command modules from the assembly
            await _interactions.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private async Task HandleInteraction(SocketInteraction interaction)
        {
            var context = new SocketInteractionContext(_client, interaction);
            await _interactions.ExecuteCommandAsync(context, _services);
        }
    }
}
