using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace yard.Objects;

internal class Inventory(ulong userId)
{
    public ulong UserId { get; } = userId;
    public List<Key> Keys { get; } = [];
    public void AddKey(Key key) => Keys.Add(key);
}
