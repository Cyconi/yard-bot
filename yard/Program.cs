using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using yard.Core;

namespace yard;

internal class Program
{
    internal readonly static string config = "config.json";
    internal readonly static ulong[] adminIds =
    [
        1318804999811367002, // hh
        964947247286616074,  // cyconi
        960962596180197426,  // ywids
        1024847118579539971  // karma
    ];
    static void Main() => new Bot().RunAsync().GetAwaiter().GetResult();

}
