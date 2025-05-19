using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using yard.Core;
using yard.Objects;

namespace yard.Commands.SlashCommands
{
    internal class UserCommands : ApplicationCommandModule
    {
        [SlashCommand("redeem", "Redeem an invite key")]
        [SlashCooldown(1, 5, SlashCooldownBucketType.User)]
        public async Task RedeemKey(InteractionContext ctx, [Option("key", "key")] string key)
        {
            CLog.L("Server", ctx.User, $"`<@{ctx.User.Id}>` called /redeem command", Channel.command);

            var user = await ctx.Guild.GetMemberAsync(ctx.User.Id);
            var role = ctx.Guild.GetRole(1330584332297048105);

            if (user.Roles.Any(r => r.Id == role.Id))
            {
                CLog.L("Server", ctx.User, $"`<@{ctx.User.Id}>` already has access and attempted to redeem a key.", Channel.command);
                await ctx.CreateResponseAsync("You already have access and cannot redeem a key!", true);
                return;
            }

            // Find the inventory where the key exists
            var inventory = InventoryManager.Inventories.FirstOrDefault(inv => inv.Keys.Any(k => k.KeyValue == key));
            var keyObject = inventory?.Keys.FirstOrDefault(k => k.KeyValue == key);

            if (keyObject == null || keyObject.RedeemedBy != null)
            {
                CLog.L("Server", ctx.User, $"Key `{key}` is invalid or has already been redeemed.", Channel.command);
                await ctx.CreateResponseAsync("This key is invalid or has already been redeemed!", true);
                return;
            }

            // Mark key as redeemed
            keyObject.RedeemedBy = ctx.User.Id;
            keyObject.DateRedeemed = DateTime.UtcNow;

            await user.GrantRoleAsync(role);
            CLog.L("Server", ctx.User, $"Key {key} has been redeemed by `<@{ctx.User.Id}>`", Channel.auth);
            CLog.L("Server", ctx.User, $"Key {key} successfully redeemed by `<@{ctx.User.Id}>`", Channel.command);

            await ctx.CreateResponseAsync($"Key successfully redeemed by <@{ctx.User.Id}>", true);
        }

        [SlashCommand("inventory", "Displays your inventory and stored keys")]
        [SlashCooldown(1, 5, SlashCooldownBucketType.User)]
        public async Task ShowInventory(InteractionContext ctx)
        {
            CLog.L("Server", ctx.User, $"`<@{ctx.User.Id}>` called /inventory command", Channel.command);

            var inventory = InventoryManager.Get(ctx.User.Id);

            if (inventory == null || inventory.Keys.Count == 0)
            {
                CLog.L("Server", ctx.User, $"Inventory check: No inventory found for `<@{ctx.User.Id}>`", Channel.command);
                await ctx.CreateResponseAsync("Your inventory is empty.", true);
                return;
            }

            var builder = new StringBuilder();
            builder.AppendLine($"**Inventory:**");

            foreach (var key in inventory.Keys)
            {
                string redemptionStatus = key.RedeemedBy.HasValue ? $"Redeemed by: <@{key.RedeemedBy}>" : "Not redeemed";
                builder.AppendLine($"- `{key.KeyValue}` | {redemptionStatus}");
            }

            CLog.L("Server", ctx.User, $"Displayed inventory for `<@{ctx.User.Id}>`.", Channel.command);
            await ctx.CreateResponseAsync(builder.ToString(), true);
        }
    }
}
