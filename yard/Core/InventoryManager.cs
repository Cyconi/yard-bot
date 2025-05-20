using DSharpPlus.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using yard.Objects;

namespace yard.Core;

internal class InventoryManager
{
    internal readonly static string jsonFile = "inventory.json";
    private static List<Inventory> _inventories = [];

    internal static List<Inventory> Inventories
    {
        get
        {
            SaveToFile();
            return _inventories;
        }
        set
        {
            _inventories = value;
            SaveToFile();
        }
    }

    internal static Inventory Get(ulong userId) => _inventories.FirstOrDefault(inv => inv.UserId == userId);

    internal static async Task<bool> Create(DiscordUser user, DiscordGuild guild)
    {
        var member = await guild.GetMemberAsync(user.Id);

        if (member == null || !member.Roles.Any(role => role.Id == 1330584332297048105) || Get(user.Id) != null)
            return false;

        _inventories.Add(new Inventory(user.Id));
        SaveToFile();
        return true;
    }

    internal static bool AddKey(ulong userId, Key key)
    {
        var inventory = Get(userId);
        if (inventory == null) 
            return false;

        inventory.Keys.Add(key);
        SaveToFile();
        return true;
    }

    public static async void SaveToFile()
    {
        await CLog.ToConsole("Saving inventories...");
        File.WriteAllText(jsonFile, JsonConvert.SerializeObject(_inventories, Formatting.Indented));
    }

    public static async void LoadFromFile()
    {
        await CLog.ToConsole("Loading inventories...");
        if (File.Exists(jsonFile))
            _inventories = JsonConvert.DeserializeObject<List<Inventory>>(File.ReadAllText(jsonFile)) ?? [];
        else
            File.WriteAllText(jsonFile, "[]");
    }
}
