namespace MasonMusicBot
{
    using System;
    using System.Linq;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.Interactivity;
    using Microsoft.Extensions.Configuration;
    using DSharpPlus.Interactivity.Extensions;
    using System.Collections;
    using System.Collections.Generic;

    internal class Program
    {
        static void Main()
        {
            var bot = new Bot();
            bot.Run().GetAwaiter().GetResult();
        }

        
    }


}



