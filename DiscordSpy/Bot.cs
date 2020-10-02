using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSpy
{
    class Bot
    {
        private DiscordSocketClient client;

        /**
         * Creates a new bot instance
         */
        public Bot()
        {
            client = new DiscordSocketClient();
        }

        public async Task MainAsync()
        {
            await Initialize();
            var token = "";
            await client.LoginAsync(Discord.TokenType.Bot, token);
            await client.StartAsync();
            await Task.Delay(-1);
        }

        /**
         * Use this method to connect events with methods
         */
        private async Task Initialize()
        {

        }
    }
}
