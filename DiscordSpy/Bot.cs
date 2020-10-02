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
            try
            {
                await Initialize();
                var token = Environment.GetEnvironmentVariable("DiscordSpyToken");      //TODO: Add discord token to personal environment
                await client.LoginAsync(Discord.TokenType.Bot, token);
                await client.StartAsync();
                await Task.Delay(-1);
            }
            catch(Exception e)      //TODO: Add TokenNotFoundException 
            {
                Console.WriteLine("Es ist ein Fehler aufgetreten! Haben Sie ein Token hinterlegt?");
            }
        }

        /**
         * Use this method to connect events with methods
         */
        private async Task Initialize()
        {
            client.UserJoined += NewUserJoin;
            client.UserVoiceStateUpdated += UserMoves;
            client.GuildMemberUpdated += ManageRole;
            client.Disconnected += CloseStats;
        }

        public async Task NewUserJoin(SocketGuildUser newUser)
        {

        }

        public async Task UserMoves(SocketUser user, SocketVoiceState before, SocketVoiceState after)
        {

        }

        public async Task ManageRole(SocketGuildUser before, SocketGuildUser after)
        {

        }

        public async Task CloseStats(Exception e)
        {

        }
    }
}
