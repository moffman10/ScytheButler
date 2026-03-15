using Discord;
using Discord.Interactions;
using ScytheButler.Services;
using SixLabors.ImageSharp;

namespace ScytheButler.Commands
{
    public class DiceCommand : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly DiceService _diceService;

        public DiceCommand(DiceService diceService)
        {
            _diceService = diceService;
        }

        [SlashCommand("roll", "Roll dice using XdY format E.G. 3d6")]
        public async Task Roll(string dice)
        {
            try
            {
                var result = _diceService.RollDice(dice);

                using var stream = new MemoryStream();

                result.Image.SaveAsPng(stream);
                stream.Position = 0;

                var embed = new EmbedBuilder()
                    .WithAuthor(Context.User.Username, Context.User.GetAvatarUrl())
                    .WithTitle("🎲 Dice Roll")
                    .WithDescription($"Rolled **{dice}**\nResults: {string.Join(", ", result.Rolls)}\n**Total: {result.Total}**")
                    .WithColor(Discord.Color.DarkBlue)
                    .WithImageUrl("attachment://roll.png")
                    .Build();
            }
            catch (Exception ex)
            {
                await RespondAsync(ex.Message);
            }
        }
    }
}