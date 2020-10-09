using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace DiscordSpy
{
    class Bot
    {
        private DiscordSocketClient client;
        private List<ulong> onlineUsers;
        string pathS;
        string pathD;
        ulong channelID;
        ulong afkChannel;
        ulong roleID;
        double interval;

        /**
         * Creates a new bot instance
         */
        public Bot()
        {
            client = new DiscordSocketClient();
            afkChannel = ulong.Parse(ConfigurationManager.AppSettings["afkChannel"]);
            roleID = ulong.Parse(ConfigurationManager.AppSettings["role"]);
            interval = double.Parse(ConfigurationManager.AppSettings["interval"]);
            pathS = ConfigurationManager.AppSettings["pathS"];
            pathD = ConfigurationManager.AppSettings["pathD"];
            channelID = ulong.Parse(ConfigurationManager.AppSettings["channelId"]);
        }

        public async Task MainAsync()
        {
            try
            {
                await Initialize();
                string token = ConfigurationManager.AppSettings["token"];
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
            client.MessageReceived += RespondOnMessage;
            client.Disconnected += CloseStats;
        }

        public async Task NewUserJoin(SocketGuildUser newUser)
        {
            Console.WriteLine($"Der Nutzer {newUser.Nickname} ist dem Server beigetreten.");
        }

        public async Task UserMoves(SocketUser user, SocketVoiceState before, SocketVoiceState after)
        {
            if (user.IsBot)
            {
                return;
            }
            if (before.VoiceChannel == null && after.VoiceChannel != null && after.VoiceChannel.Id != afkChannel || before.VoiceChannel.Id == afkChannel && after.VoiceChannel != null && after.VoiceChannel.Id != afkChannel)
            {
                UserJoin(user.Id);
            }
            else if (before.VoiceChannel != null && after.VoiceChannel == null && before.VoiceChannel.Id != afkChannel || before.VoiceChannel != null && before.VoiceChannel.Id != afkChannel && after.VoiceChannel.Id == afkChannel)
            {
                UserLeave(user.Id, user.Username);
            }
        }

        public async Task ManageRole(SocketGuildUser before, SocketGuildUser after)
        {
            if (File.Exists(pathD))
            {
                /*Check date*/
                String[] file = File.ReadAllLines(pathD);
                if (file.Length == 1)
                {
                    if (DateTime.Compare(DateTime.Parse(file[0]).AddDays(interval), DateTime.Now) <= 0)
                    {
                        using (StreamWriter sw = new StreamWriter(pathD))
                        {
                            sw.WriteLine(DateTime.Now);
                        }
                        /*Evaluate user with longest time on guild*/
                        int longestTime = -1;
                        String maxUserPath = "";        //""+pathS + "000";
                        String[] files = Directory.GetFiles(pathS);
                        foreach (String user in files)
                        {
                            String time;
                            time = File.ReadAllLines(user)[0].Split(" ")[2];       //Stores the total time of the user
                            if (int.Parse(time) > longestTime)          //unlikely: two have same time -> first win
                            {
                                longestTime = int.Parse(time);
                                maxUserPath = user;
                            }
                        }
                        /*Remove role of old member and add role to new one*/
                        try
                        {
                            var guild = before.Guild;
                            var winningUser = guild.GetUser(ulong.Parse(maxUserPath.Split("/")[7].Split(".")[0]));
                            var role = guild.GetRole(roleID);
                            var oldMembers = role.Members;
                            foreach (var member in oldMembers)       //Alle mit der VIP Rolle wird diese entzogen
                            {
                                Console.WriteLine($"Loesche die VIP Rolle von: {member}");
                                await member.RemoveRoleAsync(role, RequestOptions.Default);     //TODO: Check if await needed
                            }
                            await (winningUser as IGuildUser).AddRoleAsync(role);       //Der neue bekommt die VIP Rolle.
                            Console.WriteLine($"Der User {winningUser} hat mit {longestTime} Sekunden gewonnen.");
                            await (client.GetChannel(channelID) as IMessageChannel).SendMessageAsync($"{winningUser} war mit {longestTime / 3600}h am längsten auf dem Server und bekommt für diese Woche die VIP Rolle");
                            /*Delete all files*/
                            foreach (String delFile in files)
                            {

                                File.Delete(delFile);      //Jede Datei wird gelöscht
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Es ist ein Fehler aufgetreten und die Dateien sind schon gelöscht worden");
                        }
                    }
                }
            }
        }

        public async Task RespondOnMessage(SocketMessage message)
        {
            if(message.Author.IsBot)
            {
                return;
            }
            if(message.Content.ToLower().StartsWith("-spy"))
            {
                String[] m = message.Content.Split("/");
                try
                {
                    if(m[1].ToLower().Equals("getstats"))
                    {
                        if(File.Exists(pathS + message.Author.Id + ".txt"))
                        {
                            String[] file = File.ReadAllLines(pathS + message.Author.Id + ".txt");
                            String[] time = file[0].Split(" ");
                            int endTime = (int)(DateTime.UtcNow.Subtract(new DateTime(2020, 1, 1))).TotalSeconds;
                            int startTime = int.Parse(time[1]);
                            if(startTime != 0 && endTime - startTime < 86400)
                            {
                                int total = int.Parse(time[2]) + endTime - startTime;
                                if(File.Exists(pathD))
                                {
                                    String[] date = File.ReadAllLines(pathD);
                                    if(date.Length == 1)
                                    {
                                        Console.WriteLine($"{DateTime.Now}: {message.Author.Username} hat seine Serverzeit erfragt.");
                                        DateTime lastCheck = DateTime.Parse(date[0]);
                                        await message.Author.SendMessageAsync($"Du warst seid dem {lastCheck.ToShortDateString()} ca. {total / 3600}h auf dem Server.");
                                    }
                                    else
                                    {
                                        Console.WriteLine("Die Datumsdatei entspricht nicht dem richtigen Format.");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Es existiert keine Datumsdatei unter dem angegebenen Pfad.");
                                }
                            }
                            else if(startTime == 0)
                            {
                                int total = int.Parse(time[2]);
                                if(File.Exists(pathD))
                                {
                                    String[] date = File.ReadAllLines(pathD);
                                    if(date.Length == 1)
                                    {
                                        Console.WriteLine($"{DateTime.Now}: {message.Author.Username} hat seine Serverzeit erfragt.");
                                        DateTime lastCheck = DateTime.Parse(date[0]);
                                        await message.Author.SendMessageAsync($"Du warst seid dem {lastCheck.ToShortDateString()} ca. {total / 3600}h auf dem Server.");
                                    }
                                    else
                                    {
                                        Console.WriteLine("Die Datumsdatei entspricht nicht dem richtigen Format.");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Es existiert keine Datumsdatei unter dem angegebenen Pfad.");
                                }
                            }
                            else
                            {
                                Console.WriteLine("Der User {message.Author.Username} wollte seine Zeit wissen, es ist aber ein Fehler aufgetreten.");
                            }
                        }
                    }
                }
                catch(Exception e)
                {

                }
            }
        }

        public async Task CloseStats(Exception e)
        {
            var voiceChannels = client.GetGuild(channelID).VoiceChannels;
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
            string path = pathS + uId + ".txt";
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
            string path = pathS + uId + ".txt";
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
