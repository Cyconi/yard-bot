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

internal class KeyManager
{
    internal static bool AssignKeyToUser(Key key, DiscordMember member)
    {
        if (!InventoryManager.AddKey(member.Id, key))
            return false;

        InventoryManager.SaveToFile();
        return true;
    }
}