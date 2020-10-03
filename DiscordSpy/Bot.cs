using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSpy
{
    class Bot
    {
        private DiscordSocketClient client;
        private List<ulong> onlineUsers;

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
            if (user.IsBot)
            {
                return;
            }
            if (before.VoiceChannel == null && after.VoiceChannel != null)
            {
                UserJoin(user.Id);
            }
            else if (before.VoiceChannel != null && after.VoiceChannel == null)
            {
                UserLeave(user.Id, user.Username);
            }
        }

        public async Task ManageRole(SocketGuildUser before, SocketGuildUser after)
        {
            
        }

        public async Task CloseStats(Exception e)
        {
            var voiceChannels = client.GetGuild(0000).VoiceChannels;    //TODO: Exchange with Guild ID
            foreach (var channel in voiceChannels)
            {
                //Vllt ersetzen durch onlineUser List
                var users = channel.Users; //Gibt das alle User aus dem Channel wieder oder alle die den sehen können??
                if (users != null)
                {
                    foreach (var user in users)
                    {
                        onlineUsers.Remove(user.Id);
                        UserLeave(user.Id, user.Username);
                    }
                }
            }
        }


        private void UserJoin(ulong uId)
        {
            string path = "" + uId + ".txt";        //TODO: Add own path where stats should be stored
            if (File.Exists(path))
            {
                onlineUsers.Add(uId);

                String[] file = File.ReadAllLines(path);
                String[] time = file[0].Split(" ");

                time[1] = "" + (int)(DateTime.UtcNow.Subtract(new DateTime(2020, 1, 1))).TotalSeconds;      //Store joining time
                file[0] = "" + time[0] + " " + time[1] + " " + time[2];
                using (StreamWriter sw = new StreamWriter(path))
                {
                    for (int i = 0; i < file.Length; i++)
                    {
                        sw.WriteLine(file[i]);
                    }
                }
            }
            else
            {
                String[] layout = { "TimeInVoice: 0 0", "MessagesSend: 0 0 0" };        //Layout of the statistic file
                File.WriteAllLinesAsync(path, layout).Wait();
                UserJoin(uId);
            }
        }

        private void UserLeave(ulong uId, String uName = "Unknown")
        {
            string path = "" + uId + ".txt";        //TODO: Add own path where stats should be stored
            if (File.Exists(path))
            {
                if (onlineUsers.Contains(uId))
                {
                    onlineUsers.Remove(uId);
                }
                else
                {
                    Console.WriteLine($"Der Nutzer {uName} war auf dem Server, er konnte aber nicht aus der Liste entfernt werden.");
                }
                String [] file = File.ReadAllLines(path);
                String[] time = file[0].Split(" ");

                int endTime = (int)(DateTime.UtcNow.Subtract(new DateTime(2020, 1, 1))).TotalSeconds;
                int startTime = int.Parse(time[1]);
                if (startTime != 0 && endTime - startTime < 86400)      //Don't add time if longer than 24h (maby adjust)
                {
                    int total = int.Parse(time[2]) + endTime - startTime;
                    time[2] = "" + total;
                    file[0] = "" + time[0] + " " + 0 + " " + time[2];
                    using (StreamWriter sw = new StreamWriter(path))
                    {
                        for (int i = 0; i < file.Length; i++)
                        {
                            sw.WriteLine(file[i]);
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"Der User: {uName} war auf dem Server, aber seine Startzeit wurde nicht erfasst.");
                }
            }
            else
            {
                Console.WriteLine($"Der User: {uName} ist auf dem Server gewesen, hat aber keine User Datei.");
            }

        }
    }
}
