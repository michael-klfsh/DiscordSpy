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

/*
 * using System;
using System.Net.Sockets;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Statistik
{
    class Program
    {
        private DiscordSocketClient client;
        /**
         * Verweist auf eine async Main
         */
        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        private Program()
        {
            client = new DiscordSocketClient();
        }

        public async Task MainAsync()
        {
            await InitCommands();
            client.Log += Log;  //Optional: Loggt manche Daten auf der Konsole
            var token = "Njc2MzkxMjg3MTQxOTU3NjMy.XsgOgA.BAaC0HrFwtz3RDrMUG6RXyd1cus";     //Enter Token of Bot
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();
            await Task.Delay(-1);   //Sorg dafür dass das Programm nie endet
        }

        /**
         * Hier werden die Events den Methoden zugeordnet. Siehe Bsp.
         */
        private async Task InitCommands()
        {
            client.GuildMemberUpdated += ManageRole;
            client.UserJoined += UserJoint;
            client.UserVoiceStateUpdated += UserMoves;
            client.MessageReceived += SendTimeThisWeek;
        }

        private async Task UserMoves(SocketUser user, SocketVoiceState before, SocketVoiceState after)
        {
            if (user.IsBot)
            {
                return;
            }
            //Prüft ob jemand auf den Server kommt (nicht AFK Channel)
            if(before.VoiceChannel == null && after.VoiceChannel != null  && after.VoiceChannel.Id != 500715863628972033 || before.VoiceChannel != null && before.VoiceChannel.Id == 500715863628972033 && after.VoiceChannel != null && after.VoiceChannel.Id != 500715863628972033)
            {
                string path = "/home/pi/Desktop/DiscordBot/Statistik/Stats/" + user.Id+".txt" ;
                if (File.Exists(path))
                {
                    String[] time;
                    String[] file;
                    file = File.ReadAllLines(path);
                    time = file[0].Split(" ");
                    time[1] = ""+ (int)(DateTime.UtcNow.Subtract(new DateTime(2020, 1, 1))).TotalSeconds;
                    file[0] = "" + time[0] +" "+ time[1] +" "+ time[2];
                    using (StreamWriter sw = new StreamWriter(path))
                    {
                        for(int i=0; i<file.Length; i++)
                        {
                            sw.WriteLine(file[i]);
                        }
                    }
                }
                else
                {
                    String[] layout = { "TimeInVoice: 0 0", "MessagesSend: 0 0 0"};
                    File.WriteAllLinesAsync(path, layout).Wait();
                    UserMoves(user, before, after);
                }
            }
            //Prüft ob jemand den Server verlässt (oder in AFK Channel geht)
            else if(before.VoiceChannel != null && before.VoiceChannel.Id != 500715863628972033 && (after.VoiceChannel == null || after.VoiceChannel.Id == 500715863628972033))
            {
                string path = "/home/pi/Desktop/DiscordBot/Statistik/Stats/" + user.Id +".txt";
                if (File.Exists(path))
                {
                    String[] time;
                    String[] file;
                    file = File.ReadAllLines(path);
                    time = file[0].Split(" ");
                    int endTime = (int) (DateTime.UtcNow.Subtract(new DateTime(2020, 1, 1))).TotalSeconds;
                    int startTime = int.Parse(time[1]);
                    if(startTime != 0 && endTime -startTime < 86400)
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
                        Console.WriteLine($"Der User: {user.Username} war auf dem Server, aber seine Startzeit wurde nicht erfasst.\n Oder ein anderer Fehler.");
                    }
                }
                else
                {
                    Console.WriteLine($"Der User: {user.Username} ist auf dem Server gewesen, hat aber keine User Datei.");
                }
            }
            //Alle anderen Fälle werden ignoriert
        }
        private async Task ManageRole(SocketGuildUser before, SocketGuildUser after)
        {
            string pathS = "/home/pi/Desktop/DiscordBot/Statistik/Stats/";
            string pathD = "/home/pi/Desktop/DiscordBot/Statistik/Date/Datum.txt";
            if(File.Exists(pathD))
            {
                String[] file = File.ReadAllLines(pathD);
                if(file.Length == 1)
                {
                    if(DateTime.Compare(DateTime.Parse(file[0]).AddDays(7.0), DateTime.Now) <= 0)       //Prüft ob ob schon mehr als 7 Tage um sind
                    {
                        using(StreamWriter sw = new StreamWriter(pathD))
                        {
                            sw.WriteLine(DateTime.Now);     //Setzt Zeit von Heute in die Datei ein
                        }
                        int longestTime = -1;
                        String maxUserPath = "" + pathS + "000";
                        String[] files = Directory.GetFiles(pathS);
                        foreach(String user in files)
                        {
                            String time;
                            time = File.ReadAllLines(user)[0].Split(" ")[2];
                            if(int.Parse(time) > longestTime)       //Bei gleicher Zeit gewinnt der erste User
                            {
                                longestTime = int.Parse(time);
                                maxUserPath = user;
                            }
                        }
                        try
                        {
                        var guild = before.Guild;
                        var winningUser = guild.GetUser(ulong.Parse(maxUserPath.Split("/")[7].Split(".")[0]));
                        var role = guild.GetRole(713433199899967529);
                        var oldMembers = role.Members;
                        foreach(var member in oldMembers)       //Alle mit der VIP Rolle wird diese entzogen
                        {
                            Console.WriteLine($"Loesche die VIP Rolle von: {member}");
                            member.RemoveRoleAsync(role, RequestOptions.Default);
                        }
                        await (winningUser as IGuildUser).AddRoleAsync(role);       //Der neue bekommt die VIP Rolle.
                        Console.WriteLine($"Der User {winningUser} hat mit {longestTime} Sekunden gewonnen.");
                        await (client.GetChannel(500714761227337751) as IMessageChannel).SendMessageAsync($"{winningUser} war mit {longestTime/3600}h am längsten auf dem Server und bekommt für diese Woche die VIP Rolle");
                        foreach(String delFile in files)
                        {
                            
                            File.Delete(delFile);      //Jede Datei wird gelöscht
                        }
                        }
                        catch(Exception e)
                        {
                            Console.WriteLine("Es ist ein Fehler aufgetreten und die Dateien sind schon gelöscht worden");
                        }
                    }
                }
            }
        }
        private async Task UserJoint(SocketGuildUser user)
        {
            Console.WriteLine($"User: {user.Username} betritt den Server.");
        }

        private async Task SendTimeThisWeek(SocketMessage message)
        {
            if (message.Author.IsBot)
            {
                return;
            }
            if (message.Content.ToLower().StartsWith("-spy/getstats"))
            {
                string pathS = "/home/pi/Desktop/DiscordBot/Statistik/Stats/" + message.Author.Id + ".txt";
                string pathD = "/home/pi/Desktop/DiscordBot/Statistik/Date/Datum.txt";
                if (File.Exists(pathS))
                {
                    String[] time;
                    String[] file;
                    file = File.ReadAllLines(pathS);
                    time = file[0].Split(" ");
                    int endTime = (int)(DateTime.UtcNow.Subtract(new DateTime(2020, 1, 1))).TotalSeconds;
                    int startTime = int.Parse(time[1]);
                    if (startTime != 0 && endTime - startTime < 86400)      //User ist im VoiceChat
                    {
                        int total = int.Parse(time[2]) + endTime - startTime;
                        if (File.Exists(pathD))
                        {
                            String[] date = File.ReadAllLines(pathD);
                            if (date.Length == 1)
                            {
                                Console.WriteLine($"{DateTime.Now}: {message.Author.Username} hat seine Serverzeit erfragt.");
                                DateTime lastCheck = DateTime.Parse(date[0]);
                                await message.Author.SendMessageAsync($"Du warst seid dem {lastCheck.ToShortDateString()} ca. {total/3600}h auf dem Server.");
                            }
                            else
                            {
                                Console.WriteLine("Die Datumsdatei entspricht nicht dem richtigen Format");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Es existiert keine Datumsdatei unter dem angegebenen Pfad");
                        }
                    }
                    else if(startTime == 0)     //User ist nicht im VoiceChat
                    {
                        int total = int.Parse(time[2]);
                        if (File.Exists(pathD))
                        {
                            String[] date = File.ReadAllLines(pathD);
                            if (date.Length == 1)
                            {
                                Console.WriteLine($"{DateTime.Now}: {message.Author.Username} hat seine Serverzeit aus dem Off erfragt.");
                                DateTime lastCheck = DateTime.Parse(date[0]);
                                await message.Author.SendMessageAsync($"Du warst seid dem {lastCheck.ToShortDateString()} ca. {total/3600}h auf dem Server.");
                            }
                            else
                            {
                                Console.WriteLine("Die Datumsdatei entspricht nicht dem richtigen Format");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Es existiert keine Datumsdatei unter dem angegebenen Pfad");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Der User {message.Author.Username} wollte seine Zeit wissen, es ist aber ein Fehler aufgetreten.");
                    }
                }
            }
        }

        /**
         * Loggt die ereignisse des Bots. Eventuell später verbessern
         */
        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
 */
