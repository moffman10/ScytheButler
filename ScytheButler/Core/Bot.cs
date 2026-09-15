using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using ScytheButler.AutoCompleteHandlers;
using ScytheButler.Commands;
using ScytheButler.Data;
using ScytheButler.Services;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace ScytheButler.Core
{
    public class Bot
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _interactions;
        private readonly IServiceProvider _services;
        private readonly string _token;
        private readonly ulong _allowedRoleId;

        public Bot()
        {
            var config = new ConfigurationBuilder().AddUserSecrets<Bot>().Build();

            _token = config["DiscordToken"];
            

            if (string.IsNullOrEmpty(_token))
                throw new Exception("DiscordToken not found in user secrets!");

            var roleIdString = config["AllowedRoleId"];

            if (!ulong.TryParse(roleIdString, out _allowedRoleId))
                throw new Exception("AllowedRoleId is missing or invalid in user secrets!");

            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.Guilds
            });

            _interactions = new InteractionService(_client.Rest);

            var services = new ServiceCollection();

            services.AddSingleton<IConfiguration>(config);
            services.AddSingleton(_client);
            services.AddSingleton(_interactions);

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    config.GetConnectionString("DefaultConnection")));

            services.AddScoped<CofferService>();
            services.AddSingleton<WiseOldManService>();
            services.AddSingleton<DiceService>();
            services.AddSingleton<AccountService>();
            services.AddSingleton<AchievementService>();
            services.AddSingleton<LfrService>();

            services.AddSingleton<CofferAutoCompleteHandler>();
            services.AddSingleton<ReasonAutocompleteHandler>();
            services.AddSingleton<MetricAutocompleteHandler>();

            // Register ReferralService + HttpClient
            services.AddHttpClient<ReferralService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7521/");
            });

            // Build the IServiceProvider separately
            _services = services.BuildServiceProvider();

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
