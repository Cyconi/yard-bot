using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using yard.External_Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace yard.Commands.PrefixCommands
{
    public class Game : BaseCommandModule
    {
        [Command("cardgame")]
        public async Task CardGame(CommandContext ctx)
        {
            var usersCard = new Cardbuilder();            

            var userCardMsg = new DiscordMessageBuilder()
                .AddEmbed(new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Green)
                .WithTitle("Your Card")
                .WithDescription("You drew a " + usersCard.selectedCard)
                );
            await ctx.Channel.SendMessageAsync(userCardMsg);

            var botsCard = new Cardbuilder();

            var botCardMsg = new DiscordMessageBuilder()
                .AddEmbed(new DiscordEmbedBuilder()
                .WithColor(DiscordColor.DarkRed)
                .WithTitle("My Card Card")
                .WithDescription("I drew a " + botsCard.selectedCard)
                );
            await ctx.Channel.SendMessageAsync(botCardMsg);

            if (usersCard.selectedNumber > botsCard.selectedNumber)
                await ctx.Channel.SendMessageAsync("You Win!");
            else
                await ctx.Channel.SendMessageAsync("LMAO imagine losing to a bot");
        }
    }
}
