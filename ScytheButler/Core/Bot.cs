using Discord;
using Discord.WebSocket;
using Discord.Interactions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading.Tasks;
using ScytheButler.Services;
using ScytheButler.Commands;

namespace ScytheButler.Core
{
    public class Bot
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _interactions;
        private readonly IServiceProvider _services;
        private readonly string _token;

        public Bot()
        {
            _token = Environment.GetEnvironmentVariable("DiscordToken");


            if(string.IsNullOrEmpty(_token))
                throw new Exception("DiscordToken environment variable not set!");

            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.Guilds
            });

            _interactions = new InteractionService(_client.Rest);

            _services = new ServiceCollection().AddSingleton(_client).AddSingleton(_interactions).AddSingleton(new CofferService()).AddSingleton<CofferAutoCompleteHandler>().BuildServiceProvider();
        }

        public async Task RunAsync()
        {
            _client.Log += Log;
            _client.Ready += ReadyAsync;
            _client.InteractionCreated += HandleInteraction;

            await _client.LoginAsync(TokenType.Bot, _token);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private async Task ReadyAsync()
        {
            await _interactions.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            await _interactions.RegisterCommandsGloballyAsync();
        }

        private async Task HandleInteraction(SocketInteraction interaction)
        {
            var context = new SocketInteractionContext(_client, interaction);
            await _interactions.ExecuteCommandAsync(context, _services);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg);
            return Task.CompletedTask;
        }
    }
}
