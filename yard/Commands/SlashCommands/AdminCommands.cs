using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using yard.Core;
using yard.Objects;

namespace yard.Commands.SlashCommands;

internal class AdminCommands : ApplicationCommandModule
{
    [SlashCommand("test", "test")]
    [SlashRequireUserPermissions(Permissions.Administrator), SlashCommandPermissions(Permissions.Administrator)]
    public async Task TestMethod(InteractionContext ctx) => await ctx.CreateResponseAsync("Test response", true);

    [SlashCommand("key", "Generate a new invite key")]
    [SlashRequireUserPermissions(Permissions.ModerateMembers), SlashCommandPermissions(Permissions.ModerateMembers)]
    public async Task GenerateKey(InteractionContext ctx, [Option("user", "User to receive key")] DiscordUser user)
    {
        if (!Program.adminIds.Contains(ctx.User.Id))
        {
            CLog.E(ctx.User, $"`<@{ctx.User.Id}>` tried calling /key command with insufficient permissions", Channel.command);
            await ctx.CreateResponseAsync("You do not have access to this command", true);
            return;
        }
        CLog.L(ctx.User, $"`<@{ctx.User.Id}>` called /key command", Channel.command);

        if (InventoryManager.Get(user.Id) == null && !await InventoryManager.Create(user, ctx.Guild))
        {
            await ctx.CreateResponseAsync("User does not have the required role for an inventory.", true);
            return;
        }

        byte[] bytes = new byte[32];
        using (var rng = new RNGCryptoServiceProvider()) rng.GetBytes(bytes);
        string key = $"yard-0x{BitConverter.ToString(bytes).Replace("-", "").ToLower()}";

        if (!InventoryManager.AddKey(user.Id, new Key(key, ctx.User.Id)))
        {
            await ctx.CreateResponseAsync("Failed to assign the key. User may not have an inventory.", true);
            return;
        }
        CLog.L(ctx.User, $"Generated Key: {key}", Channel.command);

        try
        {
            var member = await ctx.Guild.GetMemberAsync(user.Id);
            await member.SendMessageAsync($"A new key has been added to your inventory, please do `/inventory` in the yard server to view");
            CLog.L(ctx.User, $"Inventory update message sent DM to `<@{user.Id}>` for key {key}.", Channel.command);
        }
        catch (Exception ex) { CLog.E(ctx.User, $"Failed to send DM to `<@{user.Id}>` Error: {ex.Message}", Channel.error); }

        await ctx.CreateResponseAsync($"Generated Key: `{key}` assigned to <@{user.Id}>", true);
    }

    [SlashCommand("invitewave", "Generates and adds a new key to every inventory")]
    [SlashRequireUserPermissions(Permissions.Administrator), SlashCommandPermissions(Permissions.Administrator)]
    public async Task GenerateMassKeys(InteractionContext ctx)
    {
        if (!Program.adminIds.Contains(ctx.User.Id))
        {
            CLog.E(ctx.User, $"`<@{ctx.User.Id}>` called /masskey command with insufficient permissions", Channel.command);
            await ctx.CreateResponseAsync("You do not have access to this command.", true);
            return;
        }
        CLog.L(ctx.User, $"`<@{ctx.User.Id}>` called /masskey command", Channel.command);

        int keysGenerated = 0;

        foreach (var inventory in InventoryManager.Inventories)
        {
            byte[] bytes = new byte[32];
            using (var rng = new RNGCryptoServiceProvider()) rng.GetBytes(bytes);
            string newKey = $"yard-0x{BitConverter.ToString(bytes).Replace("-", "").ToLower()}";

            if (!InventoryManager.AddKey(inventory.UserId, new Key(newKey, ctx.User.Id)))
            {
                CLog.L(ctx.User, $"Failed to generate key for `<@{inventory.UserId}>`", Channel.command);
                continue;
            }

            keysGenerated++;
            CLog.L(ctx.User, $"Generated Key: {newKey} for `<@{inventory.UserId}>`", Channel.command);

            // Send user a DM notifying them
            try
            {
                var member = await ctx.Guild.GetMemberAsync(inventory.UserId);
                await member.SendMessageAsync($"A key has been added to your inventory, please do `/inventory` in the yard server to view");
                CLog.L(ctx.User, $"Sent DM to `<@{inventory.UserId}>` about new key {newKey}.", Channel.command);
            }
            catch (Exception ex) { CLog.E(ctx.User, $"Failed to send DM to `<@{inventory.UserId}>`. Error: {ex.Message}", Channel.error); }
        }

        await ctx.CreateResponseAsync($"Successfully generated `{keysGenerated}` new keys and added them to inventories.", true);
    }

    [SlashCommand("findkey", "Finds a key by its value and displays all its properties")]
    [SlashRequireUserPermissions(Permissions.ModerateMembers), SlashCommandPermissions(Permissions.ModerateMembers)]
    public async Task FindKey(InteractionContext ctx, [Option("key", "The key to find")] string key)
    {
        if (!Program.adminIds.Contains(ctx.User.Id))
        {
            CLog.E(ctx.User, $"`<@{ctx.User.Id}>` called /findkey for {key} with insufficient permissions", Channel.command);
            await ctx.CreateResponseAsync("You do not have access to this command.", true);
            return;
        }
        CLog.L(ctx.User, $"`<@{ctx.User.Id}>` called /findkey for {key}", Channel.command);

        // Locate the inventory containing the key
        var inventory = InventoryManager.Inventories.FirstOrDefault(inv => inv.Keys.Any(k => k.KeyValue == key));
        var keyObject = inventory?.Keys.FirstOrDefault(k => k.KeyValue == key);

        if (keyObject == null)
        {
            CLog.L(ctx.User, $"Key {key} not found in any inventory.", Channel.command);
            await ctx.CreateResponseAsync($"Key `{key}` not found.", true);
            return;
        }

        // Format key details
        string redemptionStatus = keyObject.RedeemedBy.HasValue
            ? $"<@{keyObject.RedeemedBy}>\n**Date Redeemed:** `{keyObject.DateRedeemed:yyyy-MM-dd HH:mm}`" : "Not redeemed";

        string response = $"""
            **Key Found:** `{keyObject.KeyValue}`
            **Redeemed by:** {redemptionStatus}
            **Owner:** <@{inventory.UserId}>
            **Date Created:** `{keyObject.DateCreated:yyyy-MM-dd HH:mm}`
            **Created By:** <@{keyObject.CreatedBy}>
            """;

        CLog.L(ctx.User, $"Displayed full details of key {key}.", Channel.command);
        await ctx.CreateResponseAsync(response, true);
    }

    [SlashCommand("deletekey", "Deletes a key from an inventory")]
    [SlashRequireUserPermissions(Permissions.ModerateMembers), SlashCommandPermissions(Permissions.ModerateMembers)]
    public async Task DeleteKey(InteractionContext ctx, [Option("key", "The key to delete")] string key)
    {
        if (!Program.adminIds.Contains(ctx.User.Id))
        {
            CLog.E(ctx.User, $"`<@{ctx.User.Id}>` called /deletekey for {key} with insufficient permissions", Channel.command);
            await ctx.CreateResponseAsync("You do not have access to this command.", true);
            return;
        }
        CLog.L(ctx.User, $"`<@{ctx.User.Id}>` called /deletekey for {key}", Channel.command);

        // Find the inventory containing the key
        var inventory = InventoryManager.Inventories.FirstOrDefault(inv => inv.Keys.Any(k => k.KeyValue == key));
        var keyObject = inventory?.Keys.FirstOrDefault(k => k.KeyValue == key);

        if (keyObject == null)
        {
            CLog.L(ctx.User, $"Failed to delete key {key}: Key not found.", Channel.command);
            await ctx.CreateResponseAsync($"Key `{key}` not found in any inventory.", true);
            return;
        }

        // Remove the key
        inventory.Keys.Remove(keyObject);

        CLog.L(ctx.User, $"Successfully deleted key {key} from `<@{inventory.UserId}>` inventory.", Channel.command);
        await ctx.CreateResponseAsync($"Successfully deleted key `{key}` from <@{inventory.UserId}> inventory.", true);
    }

    [SlashCommand("purgekeys", "Destroys all non-redeemed keys")]
    [SlashRequireUserPermissions(Permissions.ModerateMembers), SlashCommandPermissions(Permissions.ModerateMembers)]
    public async Task PurgeKeys(InteractionContext ctx)
    {
        if (!Program.adminIds.Contains(ctx.User.Id))
        {
            CLog.E(ctx.User, $"`<@{ctx.User.Id}>` called /purgekeys command with insufficient permissions", Channel.command);
            await ctx.CreateResponseAsync("You do not have access to this command.", true);
            return;
        }

        CLog.L(ctx.User, $"`<@{ctx.User.Id}>` called /purgekeys command", Channel.command);

        int removedCount = 0;

        foreach (var inventory in InventoryManager.Inventories)
        {
            int beforeCount = inventory.Keys.Count;
            inventory.Keys.RemoveAll(k => k.RedeemedBy == null);
            removedCount += (beforeCount - inventory.Keys.Count);
        }

        CLog.L(ctx.User, $"Purged {removedCount} non-redeemed keys from inventories.", Channel.command);
        await ctx.CreateResponseAsync($"Successfully purged `{removedCount}` non-redeemed keys.", true);
    }

    [SlashCommand("listinventories", "Lists all user inventories and their keys")]
    [SlashRequireUserPermissions(Permissions.ModerateMembers), SlashCommandPermissions(Permissions.ModerateMembers)]
    public async Task ListInventories(InteractionContext ctx)
    {
        CLog.L(ctx.User, $"`<@{ctx.User.Id}>` called /listinventories command", Channel.command);

        if (!Program.adminIds.Contains(ctx.User.Id))
        {
            CLog.E(ctx.User, $"`<@{ctx.User.Id}>` attempted /listinventory with insufficient permissions", Channel.command);
            await ctx.CreateResponseAsync("You do not have access to this command.", true);
            return;
        }

        if (ctx.Channel.Id != 1372811138508783646)
        {
            await ctx.CreateResponseAsync("Wrong channel, please try again in <@1372811138508783646>", true);
            return;
        }

        if (InventoryManager.Inventories.Count == 0)
        {
            await ctx.CreateResponseAsync("No inventories found.", true);
            return;
        }

        await ctx.CreateResponseAsync("Listing all inventories...", true);

        foreach (var inventory in InventoryManager.Inventories)
        {
            var user = await ctx.Guild.GetMemberAsync(inventory.UserId);
            var header = $"<@{user.Id}> | {inventory.Keys.Count} Keys";

            if (inventory.Keys.Count == 0)
            {
                await ctx.Channel.SendMessageAsync($"{header} : No keys assigned.");
                continue;
            }

            await ctx.Channel.SendMessageAsync(header);
            foreach (var key in inventory.Keys)
            {
                string redemptionStatus = key.RedeemedBy.HasValue ? $"Redeemed by: <@{key.RedeemedBy}>" : "Not redeemed";
                await ctx.Channel.SendMessageAsync($"- `{key.KeyValue}` | {redemptionStatus}");
            }
        }

        CLog.L(ctx.User, "Listed all user inventories.", Channel.command);
    }

    [SlashCommand("checkinventory", "Lists a specific user's inventory and their keys")]
    [SlashRequireUserPermissions(Permissions.ModerateMembers), SlashCommandPermissions(Permissions.ModerateMembers)]
    public async Task ListUserInventory(InteractionContext ctx, [Option("user", "User whose inventory you want to check")] DiscordUser user)
    {
        if (!Program.adminIds.Contains(ctx.User.Id))
        {
            CLog.E(ctx.User, $"`<@{ctx.User.Id}>` called /userinventory for `<@{user.Id}>` with insufficient permissions", Channel.command);
            await ctx.CreateResponseAsync("You do not have access to this command.", true);
            return;
        }
        CLog.L(ctx.User, $"`<@{ctx.User.Id}>` called /userinventory for `<@{user.Id}>`", Channel.command);

        var inventory = InventoryManager.Get(user.Id);

        if (inventory == null || inventory.Keys.Count == 0)
        {
            await ctx.CreateResponseAsync($"No inventory found for <@{user.Id}> or they have no keys.", true);
            return;
        }

        var builder = new StringBuilder();
        builder.AppendLine($"<@{user.Id}> **Inventory:**");

        foreach (var key in inventory.Keys)
        {
            string redemptionStatus = key.RedeemedBy.HasValue ? $"Redeemed by: <@{key.RedeemedBy}>" : "Not redeemed";
            builder.AppendLine($"- `{key.KeyValue}` | {redemptionStatus}");
        }

        CLog.L(ctx.User, $"Displayed inventory for `<@{user.Id}>`", Channel.command);
        await ctx.CreateResponseAsync(builder.ToString(), true);
    }

    [SlashCommand("clearinventory", "Clears a specific user's inventory")]
    [SlashRequireUserPermissions(Permissions.ModerateMembers), SlashCommandPermissions(Permissions.ModerateMembers)]
    public async Task ClearUserInventory(InteractionContext ctx, [Option("user", "User whose inventory you want to clear")] DiscordUser user)
    {
        if (!Program.adminIds.Contains(ctx.User.Id))
        {
            CLog.E(ctx.User, $"`<@{ctx.User.Id}>` called /clearinventory for `<@{user.Id}>` with insufficient permissions", Channel.command);
            await ctx.CreateResponseAsync("You do not have access to this command.", true);
            return;
        }
        CLog.L(ctx.User, $"`<@{ctx.User.Id}>` called /clearinventory for <@{user.Id}>", Channel.command);

        var inventory = InventoryManager.Get(user.Id);

        if (inventory == null)
        {
            CLog.L( ctx.User, $"Attempted to clear inventory for `<@{user.Id}>` but no inventory was found.", Channel.command);
            await ctx.CreateResponseAsync($"No inventory found for <@{user.Id}>", true);
            return;
        }

        inventory.Keys.Clear();

        CLog.L(ctx.User, $"Successfully cleared inventory for `<@{user.Id}>`", Channel.command);
        await ctx.CreateResponseAsync($"Successfully cleared <@{user.Id}> inventory.", true);
    }

    [SlashCommand("makeinventories", "Creates inventories for all users with the required role")]
    [SlashRequireUserPermissions(Permissions.Administrator), SlashCommandPermissions(Permissions.Administrator)]
    public async Task MakeInventories(InteractionContext ctx)
    {
        if (!Program.adminIds.Contains(ctx.User.Id))
        {
            CLog.E(ctx.User, $"`<@{ctx.User.Id}>` called /makeinventories command with insufficient permissions", Channel.command);
            await ctx.CreateResponseAsync("You do not have access to this command.", true);
            return;
        }
        CLog.L(ctx.User, $"`<@{ctx.User.Id}>` called /makeinventories command", Channel.command);

        var role = ctx.Guild.GetRole(1330584332297048105);
        var members = await ctx.Guild.GetAllMembersAsync();
        int createdCount = 0;

        foreach (var member in members.Where(m => m.Roles.Contains(role)))
        {
            if (InventoryManager.Get(member.Id) == null)
            {
                if (!await InventoryManager.Create(member, ctx.Guild))
                {
                    CLog.L(ctx.User, $"Failed to create inventory for `<@{member.Id}>`", Channel.command);
                    continue;
                }

                CLog.L(ctx.User, $"Created inventory for `<@{member.Id}>`", Channel.command);
                createdCount++;
            }
        }

        await ctx.CreateResponseAsync($"Successfully created `{createdCount}` new inventories for users with the role.", true);
    }
}
