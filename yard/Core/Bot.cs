using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using yard.Commands;
using yard.Commands.PrefixCommands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using yard.Commands.SlashCommands;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace yard.Core;

public class Bot
{
    public static DiscordClient Client { get; private set; }
    public CommandsNextExtension Commands { get; private set; }
    public SlashCommandsExtension SlashCommands { get; private set; }
    internal static ConfigJson Config { get; private set; }

    public async Task RunAsync()
    {
        await CLog.ToConsole("Discord Bot Started");
        var httpClient = new HttpClient();
        var json = string.Empty;

        try
        {
            using var fs = File.OpenRead(Program.config);
            using var sr = new StreamReader(fs, new UTF8Encoding(false));
            json = await sr.ReadToEndAsync();
        }
        catch (Exception ex) { await CLog.ErrorToConsole($"Error reading config.json", ex); }

        await CLog.ToConsole("Set up steam reader");
        Config = JsonConvert.DeserializeObject<ConfigJson>(json);

        await CLog.ToConsole("Creating discord client...");
        Client = new DiscordClient(new DiscordConfiguration()
        {
            Intents = DiscordIntents.All,
            Token = Config.Token,
            TokenType = TokenType.Bot,
            AutoReconnect = true,
        });
        Client.UseInteractivity(new InteractivityConfiguration() { Timeout = TimeSpan.FromSeconds(2) });

        InventoryManager.LoadFromFile();

        await CLog.ToConsole("Settings up command config...");
        Commands = Client.UseCommandsNext(new CommandsNextConfiguration()
        {
            StringPrefixes = [Config.Prefix],
            EnableMentionPrefix = true,
            EnableDms = true,
            EnableDefaultHelp = false
        });

        await CLog.ToConsole("Settings up slash command config...");
        SlashCommands = Client.UseSlashCommands();

        SlashCommands.RegisterCommands<AdminCommands>(1330576181891694592);
        SlashCommands.RegisterCommands<UserCommands>(1330576181891694592);
        Commands.CommandErrored += CommandErrorHandler;

        await CLog.ToConsole("Adding listeners...");
        Client.SocketOpened += OnSocketOpen;
        Client.SocketClosed += OnSocketClosed;
        Client.SocketErrored += OnSocketErrored;

        Client.Ready += OnClientReady;
        Client.Resumed += OnClientResumed;

        Client.GuildMemberUpdated += async (sender, e) =>
        {
            ulong requiredRoleId = 1330584332297048105;

            if (!e.RolesAfter.Any(r => r.Id == requiredRoleId)) 
                return;

            if (InventoryManager.Get(e.Member.Id) == null && await InventoryManager.Create(e.Member, e.Guild))                
                CLog.L("Server", e.Member, $"Auto-created inventory for `<@{e.Member.Id}>`", Channel.command);
            
        };

        await CLog.ToConsole("Connecting bot to discord...");
        await Client.ConnectAsync();
        await Task.Delay(-1);
    }

    private async Task OnClientReady(DiscordClient sender, ReadyEventArgs e) => await CLog.ToConsole("Client Ready!");
    private async Task CommandErrorHandler(CommandsNextExtension sender, CommandErrorEventArgs e)
    {
        if (e.Exception is ChecksFailedException checkCoolDown)
        {
            string coolDownTimer = string.Empty;

            foreach (var check in checkCoolDown.FailedChecks)
            {
                var coolDown = (CooldownAttribute)check;
                TimeSpan timeLeft = coolDown.GetRemainingCooldown(e.Context);
                coolDownTimer = timeLeft.ToString(@"mm\:ss");
            }

            var cooldownMsg = new DiscordEmbedBuilder()
            {
                Title = "Chill out lil nigga your going to fast",
                Description = "Wait " + coolDownTimer,
                Color = DiscordColor.Orange
            };

            await e.Context.RespondAsync(cooldownMsg);
        }
    }
    private async Task OnSocketOpen(DiscordClient sender, SocketEventArgs e) => await CLog.ToConsole($"Connected!");
    private async Task OnSocketClosed(DiscordClient sender, SocketCloseEventArgs e) => await CLog.ToConsole($"Connection terminated (code: {e.CloseCode}, reason: \"{e.CloseMessage}\"). Reconnecting...");
    private async Task OnSocketErrored(DiscordClient sender, SocketErrorEventArgs e) => await CLog.ToConsole($"Connection error!\"{e.Exception}");       
    private async Task OnClientResumed(DiscordClient sender, ReadyEventArgs e) => await CLog.ToConsole("Client Resumed successfully!");

}
