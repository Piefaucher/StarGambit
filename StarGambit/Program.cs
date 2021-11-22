
using Autofac;
using StarGambit.System;

namespace StarGambit
{
    class Program
    {
        static void Main(string[] _)
        {
            new DiscordClient().RunBotAsync().GetAwaiter().GetResult();
        }
    }
}
