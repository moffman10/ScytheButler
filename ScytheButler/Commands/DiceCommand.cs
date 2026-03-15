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

                await Context.Channel.SendFileAsync(
                    stream,
                    "roll.png",
                    $"🎲 Rolling **{dice}**\nResults: {string.Join(", ", result.Rolls)}\n**Total: {result.Total}**"
                );
            }
            catch (Exception ex)
            {
                await RespondAsync(ex.Message);
            }
        }
    }
}