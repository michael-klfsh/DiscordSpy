using System;
using System.Threading.Tasks;

namespace DiscordSpy
{
    class Program
    {
        static void Main(string[] args)
        {
            new Bot().MainAsync().GetAwaiter().GetResult();
        }

    }
}
