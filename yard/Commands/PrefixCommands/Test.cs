using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace yard.Commands.PrefixCommands
{
    public class Test : BaseCommandModule
    {
        [Command("test")]
        [Cooldown(3, 10, CooldownBucketType.User)]
        public async Task TestCommand(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("test");
            await ctx.RespondAsync("test reply");
            Console.Write("test command used in " + ctx.Channel);
        }
        [Command("role")]
        [RequireRoles(RoleCheckMode.MatchNames, "EXO Dev")]
        public async Task Role(CommandContext ctx)
        {
                await ctx.Channel.SendMessageAsync("you have EXO role!"); 
        }
        [Command("owner")]
        [RequireOwner]
        public async Task owner(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("hi owner!");
        }
        [Command("admin")]
        [RequireUserPermissions(DSharpPlus.Permissions.Administrator)]
        public async Task admin(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("hi admin!");
        }
        [Command("server")]
        public async Task ThisGuild(CommandContext ctx)
        {
            if (ctx.Guild.Id == 1048702077263691776)
                await ctx.Channel.SendMessageAsync("This is the EXO server!");
            else
                await ctx.Channel.SendMessageAsync("uh you cant do this here :P");
        }
        [Command("channel")]
        public async Task ThisChannel(CommandContext ctx)
        {
            if (ctx.Channel.Id == 1069463418555342939)
                await ctx.Channel.SendMessageAsync("This is the bot channel!");
            else
                await ctx.Channel.SendMessageAsync("uh you cant do this here :P");
        }
        [Command("add")]
        public async Task Add(CommandContext ctx, int num1, int num2)
        {
            await ctx.Channel.SendMessageAsync($"{num1 + num2}");
        }
        [Command("ping")]
        public async Task pinguser(CommandContext ctx, string user)
        {
            await ctx.Channel.SendMessageAsync($"{user}");
        }
        [Command("GetId")]
        public async Task GetUserID(CommandContext ctx, string user)
        {
            await ctx.Channel.SendMessageAsync($"{user.Remove(0, 2).Remove(18, 1)}");
        }
        [Command("embed")]
        public async Task EmbedMsg(CommandContext ctx)
        {
            var embedMsg = new DiscordMessageBuilder()
                .AddEmbed(new DiscordEmbedBuilder()
                .WithColor(DiscordColor.DarkRed)

                .WithAuthor("name", "https://vrchat.com/home/")
                .WithThumbnail("https://res.cloudinary.com/dnpfga98m/image/upload/v1670059621/EXO_Icon_xe6jn7.png")
                .WithTitle("this is a title")
                .WithUrl("https://vrchat.com/home/")
                .WithDescription("This is desc")
                .AddField("field", "top")
                .AddField("field", "bottom")
                .AddField("field", "inline", true)
                .AddField("field", "inline2", true)
                .WithImageUrl("https://i.pinimg.com/736x/74/e3/8f/74e38ff4a4e1c96fc10e9c84640e23b8.jpg")
                .WithFooter("footer")
                .WithTimestamp(new DateTime())
                );
            await ctx.Channel.SendMessageAsync(embedMsg);
        }
        [Command("embed2")]
        public async Task EmbedMsg2(CommandContext ctx)
        {
            var embedMsg = new DiscordEmbedBuilder()
            {
                Title = "This is a title",
                Description = "This is desc",
                Color = DiscordColor.DarkRed,
            };
            await ctx.Channel.SendMessageAsync(embed: embedMsg);
            //await ctx.Channel.SendMessageAsync(embedMsg); this works to
        }
        [Command("poll")]
        public async Task Poll(CommandContext ctx, int timeLimit, string opt1, string opt2, string opt3, string opt4, params string[] question)
        {
            var interact = ctx.Client.GetInteractivity();
            TimeSpan timer = TimeSpan.FromSeconds(timeLimit);
            // gettings discord emojis
            DiscordEmoji[] optEmojis = 
                { DiscordEmoji.FromName(ctx.Client, ":one:", false),
                  DiscordEmoji.FromName(ctx.Client, ":two:", false),
                  DiscordEmoji.FromName(ctx.Client, ":three:", false),
                  DiscordEmoji.FromName(ctx.Client, ":four:", false) };
            // making the msg as a string
            string optString = 
                optEmojis[0] + " - " + opt1 + "\n" +
                optEmojis[1] + " - " + opt2 + "\n" +
                optEmojis[2] + " - " + opt3 + "\n" +
                optEmojis[3] + " - " + opt4;
            // making the embed msg
            var pollMsg = new DiscordMessageBuilder()
                .AddEmbed(new DiscordEmbedBuilder()
                .WithColor(DiscordColor.DarkRed)
                .WithTitle(string.Join(" ", question))
                .WithDescription(optString)
                );
            // sending the embed msg
            var placeReact = await ctx.Channel.SendMessageAsync(pollMsg);
            // adds reactions
            foreach (var emoji in optEmojis) 
            {
                await placeReact.CreateReactionAsync(emoji);
            }
            // emoji reactions [this is fucking borken]
            var result = await interact.CollectReactionsAsync(placeReact, timer);

            int count = 0;
            int count1 = 0;
            int count2 = 0;
            int count3 = 0;
            // counts each reaction [broken
            foreach (var emoji in result)
            {
                if (emoji.Emoji == optEmojis[0])
                    count++;
                if (emoji.Emoji == optEmojis[1])
                    count1++;
                if (emoji.Emoji == optEmojis[2])
                    count2++;
                if (emoji.Emoji == optEmojis[3])
                    count3++;
            }
            // writes total
            int totalVotes = count + count1 + count2 +count3;
            // writes votes
            string resultString = optEmojis[0] + " - " + count + " Votes \n" +
                                  optEmojis[1] + " - " + count1 + " Votes \n" +
                                  optEmojis[2] + " - " + count2 + " Votes \n" +
                                  optEmojis[3] + " - " + count3 + " Votes \n\n" +
                                  "Total Votes " + totalVotes;
            // builds embed msg
            var resultsMsg = new DiscordMessageBuilder()
                .AddEmbed(new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Green)
                .WithTitle("Poll Results")
                .WithDescription(resultString)
                );
            // sends msg
            await ctx.Channel.SendMessageAsync(resultsMsg);
        }
    }
}
