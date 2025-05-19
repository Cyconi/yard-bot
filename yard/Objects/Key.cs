using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace yard.Objects
{
    public class Key(string keyValue, ulong userId)
    {
        public string KeyValue { get; set; } = keyValue;
        public DateTime DateCreated { get; } = DateTime.UtcNow;
        public ulong CreatedBy { get; set; } = userId;
        public ulong? RedeemedBy { get; set; }
        public DateTime DateRedeemed { get; set; }
    }
}
