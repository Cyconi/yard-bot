using DSharpPlus.Entities;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace yard.Core;

public enum Channel : ulong
{
    alerts = 1368860451873620039,
    publicAlerts = 1373602466263994390,
    server = 1372811138508783646,
    command = 1372811182758695033,
    invite = 1343288628935397486,
    exiled = 1330579751517028503,
    error = 1372811232569983038
}
internal class CLog
{
    private static readonly SemaphoreSlim logSemaphore = new(1, 1);

    private static readonly Dictionary<Channel, List<string>> messageQueues = 
        ((Channel[])Enum.GetValues(typeof(Channel))).ToDictionary(channel => channel, _ => new List<string>());

    private static readonly Dictionary<Channel, DateTime> lastMessageTimes = 
        ((Channel[])Enum.GetValues(typeof(Channel))).ToDictionary(channel => channel, _ => DateTime.MinValue);

    private static readonly Dictionary<Channel, SemaphoreSlim> semaphores = 
        ((Channel[])Enum.GetValues(typeof(Channel))).ToDictionary(channel => channel, _ => new SemaphoreSlim(1, 1));


    /*private static readonly Dictionary<Channel, List<string>> messageQueues = new()
    {
        { Channel.server, new List<string>() },
        { Channel.command, new List<string>() },
        { Channel.invite, new List<string>() },
        { Channel.error, new List<string>() }
    };
    private static readonly Dictionary<Channel, DateTime> lastMessageTimes = new()
    {
        { Channel.server, DateTime.MinValue },
        { Channel.command, DateTime.MinValue },
        { Channel.invite, DateTime.MinValue },
        { Channel.error, DateTime.MinValue }
    };
    private static readonly Dictionary<Channel, SemaphoreSlim> semaphores = new()
    {
        { Channel.server, new SemaphoreSlim(1, 1) },
        { Channel.command, new SemaphoreSlim(1, 1) },
        { Channel.invite, new SemaphoreSlim(1, 1) },
        { Channel.error, new SemaphoreSlim(1, 1) }
    };*/

    public static async Task LogMessageToDiscord(DiscordUser discordUser, string message, Channel channelId)
    {
        await semaphores[channelId].WaitAsync();
        try
        {
            messageQueues[channelId].Add(message);
            var channel = await Bot.Client.GetChannelAsync((ulong)channelId);
            var allMessages = string.Join("\n", messageQueues[channelId].Select(m => discordUser == null ? $"{m}" : $"`[{discordUser.Username}]` {m}"));

            // Log to a file for the user
            if (discordUser != null)
            {
                string logDirectory = "Logs";
                Directory.CreateDirectory(logDirectory); // Ensure the Logs directory exists
                string logFilePath = Path.Combine(logDirectory, $"{discordUser.Id}.txt");

                using StreamWriter sw = new(logFilePath, true);
                await sw.WriteLineAsync($"[{DateTime.Now:HH:mm}] {allMessages}");
                await sw.FlushAsync();
            }

            while (allMessages.Length > 0)
            {
                var flushMessage = allMessages.Length <= 2000 ? allMessages : allMessages[..2000];
                allMessages = allMessages.Length <= 2000 ? "" : allMessages[2000..];

                try
                {
                    string formattedMessage = flushMessage;

                    switch (channelId)
                    {
                        case Channel.error:
                            formattedMessage = $"```{flushMessage}```";
                            break;
                        case Channel.server:
                            formattedMessage = $"{flushMessage}";
                            break;
                        case Channel.command:
                            formattedMessage = $"{flushMessage}";
                            break;
                        case Channel.invite:
                            formattedMessage = $"{flushMessage}";
                            break;
                    }

                    await channel.SendMessageAsync(formattedMessage);
                }
                catch (Exception ex) { ErrorToConsole($"Failed to send log message to Discord: {flushMessage}", ex).Wait(); }
            }

            messageQueues[channelId].Clear();
            lastMessageTimes[channelId] = DateTime.Now;
        }
        catch (Exception ex) { ErrorToConsole("Failed to send log message to Discord: ", ex).Wait(); }
        finally { semaphores[channelId].Release(); }
    }
    internal static void Tag()
    {
        System.Console.ForegroundColor = ConsoleColor.White;
        System.Console.Write("[");
        System.Console.ForegroundColor = ConsoleColor.Red;
        System.Console.Write(DateTime.Now.ToString("HH:mm"));
        System.Console.ForegroundColor = ConsoleColor.White;
        System.Console.Write("] [");
        System.Console.ForegroundColor = ConsoleColor.DarkRed;
        System.Console.Write("yard server");
        System.Console.ForegroundColor = ConsoleColor.White;
        System.Console.Write("] ");
        Console.ResetColor();
    }
    internal static async Task ToConsole(string MessageToLog)
    {
        await logSemaphore.WaitAsync();
        try
        {
            Tag();
            System.Console.ForegroundColor = ConsoleColor.White;
            System.Console.Write("[");
            System.Console.ForegroundColor = ConsoleColor.DarkRed;
            System.Console.Write("~>");
            System.Console.ForegroundColor = ConsoleColor.White;
            System.Console.Write("] ");

            System.Console.ForegroundColor = ConsoleColor.White;
            System.Console.WriteLine(MessageToLog);
            System.Console.ResetColor();

            string logDirectory = "Logs";
            Directory.CreateDirectory(logDirectory); // Ensure the Logs directory exists
            string logFilePath = Path.Combine(logDirectory, $"!Server.txt");

            using StreamWriter sw = new(logFilePath, true);
            await sw.WriteLineAsync($"[{DateTime.Now:HH:mm}] {MessageToLog}");
            await sw.FlushAsync();
        }
        finally { logSemaphore.Release(); }
    }
    internal static async Task ErrorToConsole(string MessageToLog, Exception ex = null)
    {
        await logSemaphore.WaitAsync();
        try
        {
            Tag();
            System.Console.ForegroundColor = ConsoleColor.White;
            System.Console.Write("[");
            System.Console.ForegroundColor = ConsoleColor.DarkRed;
            System.Console.Write("~>");
            System.Console.ForegroundColor = ConsoleColor.White;
            System.Console.Write("] [");
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.Write("ERROR");
            System.Console.ForegroundColor = ConsoleColor.White;
            System.Console.Write("] ");

            System.Console.ForegroundColor = ConsoleColor.White;
            System.Console.WriteLine(MessageToLog);
            System.Console.ResetColor();

            string logDirectory = "Logs";
            Directory.CreateDirectory(logDirectory); // Ensure the Logs directory exists
            string logFilePath = Path.Combine(logDirectory, $"!Server.txt");

            using StreamWriter sw = new(logFilePath, true);
            await sw.WriteLineAsync($"[{DateTime.Now:HH:mm}] {MessageToLog}");
            if (ex != null)
            {
                await sw.WriteLineAsync($"============ERROR============ \nTIME: {DateTime.Now.ToString("HH:mm.fff", System.Globalization.CultureInfo.InvariantCulture)} \nERROR MESSAGE: {ex.Message} \nLAST INSTRUCTIONS: {ex.StackTrace} \nFULL ERROR: {ex} \n=============END=============\n");
            }
            await sw.FlushAsync();
        }
        finally { logSemaphore.Release(); }
    }
    internal static async void L(DiscordUser discordUser, string MessageToLog, Channel channel = Channel.server)
    {
        Tag();
        System.Console.ForegroundColor = ConsoleColor.White;
        System.Console.Write("[");
        System.Console.ForegroundColor = ConsoleColor.DarkRed;
        System.Console.Write("~>");
        System.Console.ForegroundColor = ConsoleColor.White;
        System.Console.Write("] ");

        if (discordUser != null)
        {
            System.Console.Write("[");
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            System.Console.Write(discordUser?.Username);
            System.Console.ForegroundColor = ConsoleColor.White;
            System.Console.Write("] ");
        }

        System.Console.ForegroundColor = ConsoleColor.White;
        System.Console.WriteLine(MessageToLog);
        System.Console.ResetColor();

        await LogMessageToDiscord(discordUser, MessageToLog, channel);
    }
    internal static async void E(DiscordUser discordUser, string MessageToLog, Channel channel = Channel.error)
    {
        Tag();
        System.Console.ForegroundColor = ConsoleColor.White;
        System.Console.Write("[");
        System.Console.ForegroundColor = ConsoleColor.Red;
        System.Console.Write("~>");
        System.Console.ForegroundColor = ConsoleColor.White;
        System.Console.Write("] [");
        System.Console.ForegroundColor = ConsoleColor.Red;
        System.Console.Write("ERROR");
        System.Console.ForegroundColor = ConsoleColor.White;
        System.Console.Write("] ");

        if (discordUser != null)
        {
            System.Console.Write("[");
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            System.Console.Write(discordUser?.Username);
            System.Console.ForegroundColor = ConsoleColor.White;
            System.Console.Write("] ");
        }

        System.Console.ForegroundColor = ConsoleColor.Red;
        System.Console.WriteLine(MessageToLog);
        System.Console.ResetColor();

        await LogMessageToDiscord(discordUser, MessageToLog, channel);
    }
    public static void E(DiscordUser discordUser, Exception ex) => E(discordUser, $"\n============ERROR============ \nTIME: {DateTime.Now.ToString("HH:mm.fff", System.Globalization.CultureInfo.InvariantCulture)} \nERROR: {ex} \n=============END=============\n");

    public static void E(DiscordUser discordUser, string MessageToLog, Exception ex) => E(discordUser, $"{MessageToLog}\n============ERROR============ \nTIME: {DateTime.Now.ToString("HH:mm.fff", System.Globalization.CultureInfo.InvariantCulture)} \nERROR: {ex} \n=============END=============\n");
}
