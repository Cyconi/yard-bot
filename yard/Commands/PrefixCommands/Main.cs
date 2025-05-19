using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace yard.Commands.PrefixCommands
{
    public class Main : BaseCommandModule
    {       
        [Command("Cyconi")]
        public async Task Cyconi(CommandContext ctx)
        {
            var random = new Random();
            var list = new List<string> { "https://media.discordapp.net/attachments/1037120319569281024/1065789021755805798/Cyconi.gif", "https://media.discordapp.net/attachments/974195445490384896/1070160701945024673/56C6E373-526B-4344-8F2D-841E467B3662_1.png?width=331&height=468" };
            int index = random.Next(list.Count);

            await ctx.Channel.SendMessageAsync(list[index]);
        }        
        [Command("Cum")]
        public async Task Cum(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("Cum");
            await ctx.Channel.SendMessageAsync("Cum");
            await ctx.Channel.SendMessageAsync("Cum");
            await ctx.Channel.SendMessageAsync("Cum");
            await ctx.Channel.SendMessageAsync("Cum");
        }
        [Command("Snow")]
        public async Task FuckuSnow(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("<@955530719088762881>");
        }
        [Command("TheMissle")]
        public async Task TheMissle(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("The missile knows where it is at all times. It knows this because it knows where it isn't. By subtracting where it is from where it isn't, or where it isn't from where it is - whichever is greater - it obtains a difference or deviation. The guidance subsystem uses deviations to generate corrective commands to drive the missile from a position where it is to a position where it isn't, and arriving at a position that it wasn't, it now is. Consequently, the position where it is is now the position that it wasn't, and if follows that the position that it was is now the position that it isn't. In the event that the position that it is in is not the position that it wasn't, the system has acquired a variation. The variation being the difference between where the missile is and where it wasn't. If variation is considered to be a significant factor, it too may be corrected by the GEA. However, the missile must also know where it was. The missile guidance computer scenario works as follows: Because a variation has modified some of the information that the missile has obtained, it is not sure just where it is. However, it is sure where it isn't, within reason, and it know where it was. It now subtracts where it should be from where it wasn't, or vice versa. And by differentiating this from the algebraic sum of where it shouldn't be and where it was, it is able to obtain the deviation and its variation, which is called error.");
        }       
        [Command("erp")]
        public async Task erp(CommandContext ctx)
        {
            await ctx.RespondAsync("https://tenor.com/view/moistcritikal-moistcr1tikal-charlie-gif-25485863");
        }
        [Command("eacmelon")]
        public async Task EACmelon(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("no maps?");
        }
        [Command("gay")]
        public async Task gay(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("ur gay");
        }
    }
}
