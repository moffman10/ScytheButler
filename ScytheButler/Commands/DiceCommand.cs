using Discord;
using Discord.Interactions;
using ScytheButler.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;
using System.Threading.Tasks;

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
            // Step 1: defer immediately
            await DeferAsync();

            try
            {
                // Step 2: heavy work in background thread
                var (rollData, imageStream) = await Task.Run(() =>
                {
                    var data = _diceService.RollDiceData(dice);
                    var image = _diceService.GenerateDiceImage(data.Rolls);
                    var stream = new MemoryStream();
                    image.SaveAsPng(stream);
                    stream.Position = 0;
                    return (data, stream);
                });

                // Step 3: send followup with image
                var embed = new EmbedBuilder()
                    .WithAuthor(Context.User.Username, Context.User.GetAvatarUrl())
                    .WithTitle("🎲 Dice Roll")
                    .WithDescription($"Rolled **{dice}**\nResults: {string.Join(", ", rollData.Rolls)}\n**Total: {rollData.Total}**")
                    .WithColor(Discord.Color.DarkBlue)
                    .WithImageUrl("attachment://roll.png")
                    .Build();

                await FollowupWithFileAsync(imageStream, "roll.png", embed: embed);
            }
            catch (Exception ex)
            {
                await FollowupAsync($"Error: {ex.Message}");
            }
        }
    }
}